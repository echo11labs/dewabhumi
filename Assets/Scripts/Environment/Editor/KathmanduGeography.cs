using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dewabhumi.Environment.Editor
{
    /// <summary>Metre-scale source sampling. Source rows run north to south; Unity Z runs north.</summary>
    internal sealed class KathmanduGeography
    {
        internal const string DataPath = "Assets/Geography/Kathmandu/";
        [Serializable] internal sealed class Metadata
        {
            public int samples;
            public float extent;
            public float sampleSpacing;
            public double utmEasting;
            public double utmNorthing;
            public River[] rivers;
        }
        [Serializable] internal sealed class River
        {
            public string id;
            public string name;
            public string kind;
            public bool hidden;
            public Point[] points;
            // Artistic bank widths, not surveyed historical measurements.
            public float HalfWidth => name == "Bagmati" ? 18f :
                name == "Bishnumati" || name == "Manohara" || name == "Hanumante" ? 10f : kind == "river" ? 6f : 3f;
        }
        [Serializable] internal struct Point { public float x, y, z; }
        internal struct WaterSample
        {
            public float Distance;
            public float HalfWidth;
            public float Surface;
        }
        private struct Segment
        {
            public Vector2 A, B;
            public float Width;
            public float StartHeight, EndHeight;
        }

        internal readonly Metadata Info;
        private readonly float[] _ground;
        private readonly float[] _source;
        private readonly Dictionary<Vector2Int, List<Segment>> _riverBins = new Dictionary<Vector2Int, List<Segment>>();
        private const float BinSize = 128f;
        internal static readonly Vector2 Camp = new Vector2(-7490f, -1230f);
        internal static readonly Vector2 LocalOrigin = new Vector2(-10000f, -3750f);
        internal const float LocalSize = 5000f;

        internal KathmanduGeography()
        {
            Info = JsonUtility.FromJson<Metadata>(File.ReadAllText(DataPath + "unity-geography.json"));
            if (Info == null || Info.samples != 1025 || Info.extent != 40000 || Info.rivers == null)
                throw new InvalidDataException("Unexpected Kathmandu geography schema. Run tools/prepare_kathmandu.py.");
            _ground = ReadRaster("kathmandu-ground-height.f32");
            _source = ReadRaster("kathmandu-source-height.f32");
            foreach (River river in Info.rivers)
            {
                if (river.hidden) continue;
                for (int i = 1; i < river.points.Length; i++)
                {
                    Point a = river.points[i - 1], b = river.points[i];
                    var segment = new Segment { A = new Vector2(a.x, a.z), B = new Vector2(b.x, b.z),
                        Width = river.HalfWidth, StartHeight = Ground(a.x, a.z) + 1.2f, EndHeight = Ground(b.x, b.z) + 1.2f };
                    if ((segment.B - segment.A).sqrMagnitude < 0.001f) continue;
                    Vector2 lo = Vector2.Min(segment.A, segment.B) - Vector2.one * 70f;
                    Vector2 hi = Vector2.Max(segment.A, segment.B) + Vector2.one * 70f;
                    for (int x = Mathf.FloorToInt(lo.x / BinSize); x <= Mathf.FloorToInt(hi.x / BinSize); x++)
                    for (int z = Mathf.FloorToInt(lo.y / BinSize); z <= Mathf.FloorToInt(hi.y / BinSize); z++)
                    {
                        var key = new Vector2Int(x, z);
                        if (!_riverBins.TryGetValue(key, out var list)) _riverBins[key] = list = new List<Segment>();
                        list.Add(segment);
                    }
                }
            }
        }

        private float[] ReadRaster(string name)
        {
            byte[] bytes = File.ReadAllBytes(DataPath + name);
            if (bytes.Length != Info.samples * Info.samples * sizeof(float)) throw new InvalidDataException(name + " has the wrong dimensions.");
            var result = new float[bytes.Length / 4];
            if (!BitConverter.IsLittleEndian)
                for (int i = 0; i < bytes.Length; i += 4) Array.Reverse(bytes, i, 4);
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            foreach (float value in result)
                if (float.IsNaN(value) || float.IsInfinity(value) || value < 500 || value > 3000)
                    throw new InvalidDataException(name + " contains invalid elevation.");
            return result;
        }

        private float Sample(float[] grid, float x, float z)
        {
            float c = Mathf.Clamp01(x / Info.extent + 0.5f) * (Info.samples - 1);
            float r = Mathf.Clamp01(0.5f - z / Info.extent) * (Info.samples - 1);
            int x0 = Mathf.FloorToInt(c), z0 = Mathf.FloorToInt(r);
            int x1 = Mathf.Min(x0 + 1, Info.samples - 1), z1 = Mathf.Min(z0 + 1, Info.samples - 1);
            return Mathf.Lerp(Mathf.Lerp(grid[z0 * Info.samples + x0], grid[z0 * Info.samples + x1], c - x0),
                Mathf.Lerp(grid[z1 * Info.samples + x0], grid[z1 * Info.samples + x1], c - x0), r - z0);
        }
        internal float Ground(float x, float z) => Sample(_ground, x, z);
        internal float Source(float x, float z) => Sample(_source, x, z);

        internal WaterSample Water(float x, float z)
        {
            var result = new WaterSample { Distance = float.PositiveInfinity };
            var position = new Vector2(x, z);
            if (!_riverBins.TryGetValue(new Vector2Int(Mathf.FloorToInt(x / BinSize), Mathf.FloorToInt(z / BinSize)), out var segments)) return result;
            foreach (Segment segment in segments)
            {
                Vector2 delta = segment.B - segment.A;
                float t = Mathf.Clamp01(Vector2.Dot(position - segment.A, delta) / delta.sqrMagnitude);
                float distance = Vector2.Distance(position, segment.A + delta * t);
                if (distance >= result.Distance) continue;
                result = new WaterSample { Distance = distance, HalfWidth = segment.Width,
                    Surface = Mathf.Lerp(segment.StartHeight, segment.EndHeight, t) };
            }
            return result;
        }

        internal float LocalHeight(float x, float z)
        {
            float height = Ground(x, z);
            float edge = Mathf.Min(x - LocalOrigin.x, LocalOrigin.x + LocalSize - x,
                z - LocalOrigin.y, LocalOrigin.y + LocalSize - z);
            float detail = Mathf.SmoothStep(0, 1, Mathf.Clamp01(edge / 100f));
            WaterSample water = Water(x, z);
            float bank = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(water.HalfWidth * 0.8f, water.HalfWidth + 16f, water.Distance));
            if (water.Distance < water.HalfWidth + 16f)
                height = Mathf.Lerp(Mathf.Min(height, water.Surface - 1.5f), height, bank * detail + 1f - detail);
            float micro = (Mathf.PerlinNoise(x * 0.016f + 280, z * 0.016f + 330) - 0.5f) * 0.7f;
            height += micro * bank * detail;
            float clearingDistance = Vector2.Distance(new Vector2(x, z), Camp);
            float clearing = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(55, 110, clearingDistance));
            // A small authorial campsite, not a claimed archaeological location.
            if (water.Distance > 65)
                height = Mathf.Lerp(height, Ground(Camp.x, Camp.y) + (z - Camp.y) * 0.007f, clearing * detail);
            return height;
        }
    }
}

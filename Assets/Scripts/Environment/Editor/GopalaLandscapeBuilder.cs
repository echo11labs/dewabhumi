using System;
using System.Collections.Generic;
using System.IO;
using Dewabhumi.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Dewabhumi.Environment.Editor
{
    /// <summary>Explicit, isolated geographic reconstruction; never runs on Editor load.</summary>
    public static class GopalaLandscapeBuilder
    {
        private const string Root = "Assets/Art/Environment/GopalaLandscape";
        private const string ScenePath = "Assets/Scenes/GopalaKathmandu.unity";
        private const float BaseElevation = 500;
        private const float HeightRange = 2400;
        private static readonly string[] LayerNames = { "Meadow", "WoodlandFloor", "Alluvium", "WetMeadow", "ExposedRock", "DryGrass" };
        private static readonly Color[] LayerColors = {
            new Color(0.20f, 0.27f, 0.14f), new Color(0.10f, 0.16f, 0.085f),
            new Color(0.45f, 0.41f, 0.31f), new Color(0.24f, 0.31f, 0.17f),
            new Color(0.39f, 0.39f, 0.35f), new Color(0.46f, 0.43f, 0.25f) };

        [MenuItem("Dewabhumi/Landscape/Build Gopala Kathmandu Preview %#F8")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before building the landscape.");
            var geography = new KathmanduGeography(); // Validate inputs before touching scene state.
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory("reports/GopalaLandscape");
            // Keep every dirty scene recoverable, including the pre-existing Untitled scene.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene original = SceneManager.GetSceneAt(i);
                if (!original.isDirty) continue;
                Directory.CreateDirectory("Assets/_Recovery");
                string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/BeforeGopalaLandscape.unity");
                if (!EditorSceneManager.SaveScene(original, backup, true)) throw new IOException("Could not preserve the current scene.");
            }
            try
            {
                EditorUtility.DisplayProgressBar("Gopala Kathmandu", "Preparing terrain surfaces", 0.05f);
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                TerrainLayer[] layers = BuildLayers();
                var regional = BuildTerrain(geography, false, layers);
                EditorUtility.DisplayProgressBar("Gopala Kathmandu", "Refining the local river plain", 0.35f);
                var local = BuildTerrain(geography, true, layers);
                // The detailed tile replaces exactly 128 x 128 cells of the regional height field.
                int holesResolution = regional.terrainData.holesResolution;
                var holes = new bool[holesResolution, holesResolution];
                int x0 = Mathf.RoundToInt((KathmanduGeography.LocalOrigin.x + 20000) / 40000 * holesResolution);
                int z0 = Mathf.RoundToInt((KathmanduGeography.LocalOrigin.y + 20000) / 40000 * holesResolution);
                int count = Mathf.RoundToInt(KathmanduGeography.LocalSize / 40000 * holesResolution);
                for (int z = 0; z < holesResolution; z++)
                for (int x = 0; x < holesResolution; x++) holes[z, x] = !(x >= x0 && x < x0 + count && z >= z0 && z < z0 + count);
                regional.terrainData.SetHoles(0, 0, holes);
                EditorUtility.DisplayProgressBar("Gopala Kathmandu", "Following mapped river reaches", 0.7f);
                int rivers = BuildRivers(geography, local);
                SetupLighting();
                var cameraObject = new GameObject("Kathmandu Landscape Camera");
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.allowHDR = true;
                camera.backgroundColor = new Color(0.58f, 0.68f, 0.71f);
                camera.clearFlags = CameraClearFlags.Skybox;
                cameraObject.AddComponent<AudioListener>();
                cameraObject.AddComponent<UniversalAdditionalCameraData>();
                var navigator = cameraObject.AddComponent<KathmanduLandscapeCamera>();
                Vector2 camp = KathmanduGeography.Camp;
                navigator.Configure(regional, local, new Vector3(camp.x, geography.LocalHeight(camp.x, camp.y), camp.y));
                navigator.SetView(false);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                if (SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.LookAt(new Vector3(camp.x, geography.LocalHeight(camp.x, camp.y), camp.y),
                        Quaternion.Euler(52, 15, 0), 1500);
                File.WriteAllText("reports/GopalaLandscape/build.json", JsonUtility.ToJson(new BuildReport {
                    scene = ScenePath, riverReaches = rivers, regionMetres = 40000, localMetres = 5000,
                    sourceSpacingMetres = geography.Info.sampleSpacing, localSpacingMetres = 5000f / 1024,
                    sourceCampElevation = geography.Source(camp.x, camp.y), previewCampElevation = geography.LocalHeight(camp.x, camp.y),
                    northSource = geography.Source(0, 15000), southSource = geography.Source(0, -15000),
                    maximumBoundaryError = BoundaryError(geography),
                    notes = "No character, building, vegetation or rock models generated. Vegetation assets awaited. Modern DSM and waterways are geographic proxies, not a surveyed Gopala landscape."
                }, true));
                Debug.Log("[GopalaLandscape] Built " + ScenePath + "; " + rivers + " visible mapped reaches. Original dirty scenes preserved under Assets/_Recovery.");
                Capture();
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        [Serializable] private sealed class BuildReport
        {
            public string scene, notes;
            public int riverReaches;
            public float regionMetres, localMetres, sourceSpacingMetres, localSpacingMetres;
            public float sourceCampElevation, previewCampElevation, northSource, southSource, maximumBoundaryError;
        }

        private static float BoundaryError(KathmanduGeography geography)
        {
            float maximum = 0;
            Vector2 origin = KathmanduGeography.LocalOrigin;
            for (int i = 0; i <= 128; i++)
            {
                float t = i / 128f * KathmanduGeography.LocalSize;
                foreach (Vector2 p in new[] { origin + new Vector2(t, 0), origin + new Vector2(t, 5000),
                    origin + new Vector2(0, t), origin + new Vector2(5000, t) })
                    maximum = Mathf.Max(maximum, Mathf.Abs(geography.LocalHeight(p.x, p.y) - geography.Ground(p.x, p.y)));
            }
            if (maximum > 0.01f) throw new InvalidDataException("Local terrain edge does not match regional terrain.");
            return maximum;
        }

        private static Terrain BuildTerrain(KathmanduGeography geography, bool detailed, TerrainLayer[] layers)
        {
            string name = detailed ? "SettlementRiverPlain" : "KathmanduRegionalLandforms";
            float size = detailed ? KathmanduGeography.LocalSize : geography.Info.extent;
            Vector2 origin = detailed ? KathmanduGeography.LocalOrigin : new Vector2(-20000, -20000);
            string dataPath = Root + "/" + name + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(dataPath);
            bool newData = data == null;
            if (newData) data = new TerrainData { name = name };
            data.heightmapResolution = 1025;
            data.size = new Vector3(size, HeightRange, size);
            data.alphamapResolution = 512;
            data.baseMapResolution = 1024;
            var heights = new float[1025, 1025];
            for (int z = 0; z < 1025; z++)
            for (int x = 0; x < 1025; x++)
            {
                float wx = origin.x + x / 1024f * size, wz = origin.y + z / 1024f * size;
                heights[z, x] = ((detailed ? geography.LocalHeight(wx, wz) : geography.Ground(wx, wz)) - BaseElevation) / HeightRange;
                if (heights[z, x] < 0 || heights[z, x] > 1) throw new InvalidDataException("Height normalization would clip terrain.");
            }
            data.SetHeights(0, 0, heights);
            data.terrainLayers = layers;
            PaintTerrain(geography, data, origin, size, detailed);
            if (newData) AssetDatabase.CreateAsset(data, dataPath);
            else EditorUtility.SetDirty(data);
            GameObject obj = Terrain.CreateTerrainGameObject(data);
            obj.name = name;
            obj.transform.position = new Vector3(origin.x, BaseElevation, origin.y);
            Terrain terrain = obj.GetComponent<Terrain>();
            terrain.heightmapPixelError = 1;
            terrain.basemapDistance = GopalaTerrainTextures.Available ? 10000 : detailed ? 1800 : 6000;
            terrain.drawInstanced = true;
            terrain.allowAutoConnect = false;
            terrain.materialTemplate = GopalaTerrainTextures.Available ? GopalaTerrainTextures.SurfaceMaterial() :
                AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/TerrainLit.mat");
            if (terrain.materialTemplate == null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
                SaveAsset(material, Root + "/TerrainSurface.mat");
                terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(Root + "/TerrainSurface.mat");
            }
            return terrain;
        }

        internal static void PaintTerrain(KathmanduGeography geography, TerrainData data, Vector2 origin, float size, bool detailed)
        {
            int resolution = detailed ? 1024 : 512;
            data.alphamapResolution = resolution;
            data.baseMapResolution = detailed ? 2048 : 1024;
            int layerCount = data.terrainLayers.Length;
            var splats = new float[resolution, resolution, layerCount];
            for (int z = 0; z < resolution; z++)
            for (int x = 0; x < resolution; x++)
            {
                float u = x / (float)(resolution - 1), v = z / (float)(resolution - 1), wx = origin.x + u * size, wz = origin.y + v * size;
                float height = geography.Ground(wx, wz), slope = data.GetSteepness(u, v);
                var water = geography.Water(wx, wz);
                float patch = Mathf.PerlinNoise(wx * 0.003f + 45, wz * 0.003f + 95);
                float finePatch = Mathf.PerlinNoise(wx * 0.014f + 12, wz * 0.014f + 73);
                float rock = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(23, 44, slope));
                float shoreDistance = Mathf.Max(0, water.Distance - water.HalfWidth);
                float wet = (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(8, 55, shoreDistance))) * (1 - rock);
                float bank = (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2, 18 + finePatch * 16, shoreDistance))) * (1 - rock);
                float forest = Mathf.Clamp01((height - 1380) / 600 + (patch - 0.35f) * 0.7f) * (1 - wet);
                float dry = Mathf.Clamp01((0.64f - patch) * 0.55f + (finePatch - 0.5f) * 0.08f) * (1 - wet) * (1 - forest);
                float clearing = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(65, 140, Vector2.Distance(new Vector2(wx, wz), KathmanduGeography.Camp)));
                forest *= 1 - clearing;
                float soil = Mathf.Clamp01((finePatch - 0.65f) * 0.5f) * (1 - wet) * (1 - clearing);
                float[] weights = { Mathf.Max(0.03f, 1 - forest - dry - rock), forest, bank * 4, wet * 1.3f + soil, rock * 5, dry };
                float sum = 0;
                foreach (float weight in weights) sum += weight;
                for (int layer = 0; layer < layerCount; layer++) splats[z, x, layer] = weights[layer] / sum;
            }
            data.SetAlphamaps(0, 0, splats);
        }

        private static TerrainLayer[] BuildLayers()
        {
            if (GopalaTerrainTextures.Available) return GopalaTerrainTextures.BuildLayers();
            var result = new TerrainLayer[LayerNames.Length];
            for (int layer = 0; layer < result.Length; layer++)
            {
                var texture = new Texture2D(128, 128, TextureFormat.RGB24, true) { name = LayerNames[layer], wrapMode = TextureWrapMode.Repeat };
                var colors = new Color[128 * 128];
                for (int y = 0; y < 128; y++)
                for (int x = 0; x < 128; x++)
                {
                    float noise = 0, amplitude = 0.5f;
                    for (int octave = 0; octave < 4; octave++)
                    {
                        // Integer-frequency Fourier waves tile without the dark grid produced by edge blending noise.
                        int frequency = 1 << octave;
                        float u = x / 128f * Mathf.PI * 2, v = y / 128f * Mathf.PI * 2;
                        float value = 0;
                        for (int wave = 0; wave < 6; wave++)
                        {
                            int kx = (wave * 3 + 1) % 7 - 3;
                            int ky = (wave * 5 + 2) % 7 - 3;
                            value += Mathf.Sin((kx * u + ky * v) * frequency + wave * 2.39f + layer * 1.7f);
                        }
                        noise += value / 6 * amplitude;
                        amplitude *= 0.5f;
                    }
                    colors[y * 128 + x] = LayerColors[layer] * (1 + noise * 0.045f);
                }
                texture.SetPixels(colors);
                texture.Apply();
                string texturePath = Root + "/" + LayerNames[layer] + ".png";
                File.WriteAllBytes(texturePath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(texturePath);
                var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.anisoLevel = 8;
                importer.SaveAndReimport();
                var terrainLayer = new TerrainLayer { name = LayerNames[layer], diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath),
                    tileSize = new Vector2(23 + layer * 7, 23 + layer * 7), smoothness = 0.03f, metallic = 0 };
                string path = Root + "/" + LayerNames[layer] + ".terrainlayer";
                SaveAsset(terrainLayer, path);
                result[layer] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            }
            return result;
        }

        private static bool InLocal(Vector3 p) => p.x >= -10000 && p.x <= -5000 && p.z >= -3750 && p.z <= 1250;

        private static int BuildRivers(KathmanduGeography geography, Terrain local)
        {
            Shader shader = Shader.Find("Dewabhumi/Gopala River");
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("The Gopala river shader is unavailable.");
            var material = new Material(shader) { name = "RiverWater" };
            SaveAsset(material, Root + "/RiverWater.mat");
            material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/RiverWater.mat");
            GameObject parent = new GameObject("Mapped waterways - modern geographic proxies");
            int count = 0;
            foreach (var river in geography.Info.rivers)
            {
                if (river.hidden || river.points.Length < 2) continue;
                var centers = new List<Vector3>();
                for (int i = 1; i < river.points.Length; i++)
                {
                    var pa = river.points[i - 1]; var pb = river.points[i];
                    Vector3 a = new Vector3(pa.x, 0, pa.z), b = new Vector3(pb.x, 0, pb.z);
                    float length = Vector3.Distance(a, b);
                    int steps = Mathf.Max(1, Mathf.CeilToInt(length / (InLocal(a) || InLocal(b) ? 3 : 12)));
                    float ha = geography.Ground(a.x, a.z) + 1.2f, hb = geography.Ground(b.x, b.z) + 1.2f;
                    for (int step = 0; step < steps; step++)
                    {
                        float t = step / (float)steps;
                        Vector3 point = Vector3.Lerp(a, b, t);
                        point.y = Mathf.Lerp(ha, hb, t);
                        if (centers.Count == 0 || (centers[centers.Count - 1] - point).sqrMagnitude > 0.01f) centers.Add(point);
                    }
                }
                var last = river.points[river.points.Length - 1];
                centers.Add(new Vector3(last.x, geography.Ground(last.x, last.z) + 1.2f, last.z));
                var vertices = new Vector3[centers.Count * 2];
                var uv = new Vector2[vertices.Length];
                var triangles = new int[(centers.Count - 1) * 6];
                float travelled = 0;
                for (int i = 0; i < centers.Count; i++)
                {
                    Vector3 direction = centers[Mathf.Min(i + 1, centers.Count - 1)] - centers[Mathf.Max(i - 1, 0)];
                    direction.y = 0;
                    Vector3 side = Vector3.Cross(Vector3.up, direction.normalized) * river.HalfWidth;
                    float localSurface = centers[i].y;
                    if (InLocal(centers[i]))
                    {
                        // Sample the actual interpolated Unity terrain, not just the source raster.
                        // Narrow channels otherwise disappear between coarse heightmap samples.
                        foreach (float across in new[] { -1f, -0.5f, 0f, 0.5f, 1f })
                        {
                            Vector3 p = centers[i] + side * across;
                            if (InLocal(p)) localSurface = Mathf.Max(localSurface,
                                Mathf.Max(local.SampleHeight(p) + BaseElevation, geography.Ground(p.x, p.z)) + 0.35f);
                        }
                    }
                    for (int edge = 0; edge < 2; edge++)
                    {
                        Vector3 p = centers[i] + side * (edge == 0 ? -1 : 1);
                        p.y = localSurface;
                        // Outside the detailed tile, follow the coarse visual ground to avoid buried ribbons.
                        if (!InLocal(p)) p.y = Mathf.Max(p.y, geography.Ground(p.x, p.z) + 0.25f);
                        vertices[i * 2 + edge] = p;
                        uv[i * 2 + edge] = new Vector2(edge, travelled / 15);
                    }
                    if (i < centers.Count - 1)
                    {
                        int t = i * 6, v = i * 2;
                        triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                        triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
                        travelled += Vector3.Distance(centers[i], centers[i + 1]);
                    }
                }
                // Ensure normals face upward regardless of source line order.
                for (int i = 0; i < triangles.Length; i += 3)
                    if (Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]], vertices[triangles[i + 2]] - vertices[triangles[i]]).y < 0)
                    { int swap = triangles[i + 1]; triangles[i + 1] = triangles[i + 2]; triangles[i + 2] = swap; }
                var mesh = new Mesh { name = river.id.Replace('/', '_'), indexFormat = IndexFormat.UInt32, vertices = vertices, uv = uv, triangles = triangles };
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
                string meshPath = Root + "/" + river.id.Replace('/', '_') + ".asset";
                SaveAsset(mesh, meshPath);
                var obj = new GameObject(river.name + " - " + river.id);
                obj.transform.SetParent(parent.transform);
                obj.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                var renderer = obj.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                count++;
            }
            return count;
        }

        private static void SetupLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.65f, 0.73f, 0.80f);
            RenderSettings.ambientEquatorColor = new Color(0.47f, 0.51f, 0.43f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.25f, 0.19f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.64f, 0.72f, 0.73f);
            RenderSettings.fogDensity = 0.000018f;
            var sunObject = new GameObject("Late monsoon afternoon sun");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1, 0.93f, 0.80f);
            sun.intensity = 1.5f;
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(38, -50, 0);
            RenderSettings.sun = sun;
            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader) { name = "ValleySky" };
                sky.SetFloat("_AtmosphereThickness", 1.15f);
                sky.SetFloat("_Exposure", 1.1f);
                sky.SetColor("_SkyTint", new Color(0.48f, 0.57f, 0.65f));
                SaveAsset(sky, Root + "/ValleySky.mat");
                RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(Root + "/ValleySky.mat");
            }
        }

        private static void SaveAsset(UnityEngine.Object value, string path)
        {
            UnityEngine.Object existing = AssetDatabase.LoadMainAssetAtPath(path);
            if (existing == null) AssetDatabase.CreateAsset(value, path);
            else
            {
                EditorUtility.CopySerialized(value, existing);
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(value);
            }
        }

        private static Camera _captureCamera;
        private static KathmanduLandscapeCamera _captureNavigator;
        private static int _captureIndex;
        private static double _captureAfter;
        private static string _captureFolder;
        private static readonly string[] CaptureNames = { "settlement", "valley", "ground", "surface" };

        [MenuItem("Dewabhumi/Landscape/Capture Gopala Preview %#F9")]
        public static void Capture()
        {
            string scenePath = SceneManager.GetActiveScene().path;
            if (scenePath != ScenePath && scenePath != GopalaWaterPreview.PreviewScene) throw new InvalidOperationException("Open a Gopala Kathmandu preview first.");
            Camera camera = Camera.main;
            if (camera == null || !camera.TryGetComponent<KathmanduLandscapeCamera>(out var navigator)) throw new InvalidOperationException("Landscape camera missing.");
            _captureFolder = scenePath == GopalaWaterPreview.PreviewScene ? "reports/GopalaLandscape/Water" : "reports/GopalaLandscape";
            Directory.CreateDirectory(_captureFolder);
            _captureCamera = camera;
            _captureNavigator = navigator;
            _captureIndex = 0;
            navigator.SetView(false);
            _captureAfter = EditorApplication.timeSinceStartup + 2;
            EditorApplication.update -= AdvanceCapture;
            EditorApplication.update += AdvanceCapture;
        }

        private static void AdvanceCapture()
        {
            if (_captureCamera == null || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.update -= AdvanceCapture;
                return;
            }
            EditorApplication.QueuePlayerLoopUpdate();
            if (EditorApplication.timeSinceStartup < _captureAfter) return;
            Camera camera = _captureCamera;
            var target = new RenderTexture(1600, 900, 24);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(_captureFolder + "/" + CaptureNames[_captureIndex] + ".png", image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            catch (Exception exception)
            {
                EditorApplication.update -= AdvanceCapture;
                _captureNavigator.SetView(false);
                Debug.LogException(exception);
                return;
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
            _captureIndex++;
            if (_captureIndex == CaptureNames.Length)
            {
                EditorApplication.update -= AdvanceCapture;
                _captureNavigator.SetView(false);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                Debug.Log("[GopalaLandscape] Captured settlement, valley, riverbank and surface previews.");
            }
            else
            {
                if (_captureIndex == 1) _captureNavigator.SetView(true);
                else if (_captureIndex == 2) _captureNavigator.SetCloseView();
                else _captureNavigator.SetSurfaceView();
                _captureAfter = EditorApplication.timeSinceStartup + 2;
            }
        }
    }
}

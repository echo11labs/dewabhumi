using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dewabhumi.Environment.Editor
{
    /// <summary>Imports the retained CC0 ambientCG maps into the geographic preview's existing layers.</summary>
    internal static class GopalaTerrainTextures
    {
        private const string Sources = "Assets/Art/Environment/TerrainTextures/ambientCG/";
        private const string Generated = "Assets/Art/Environment/GopalaLandscape/";
        private static readonly string[] Assets = { "Grass004", "Ground037", "Ground054", "Ground048", "Rock030", "Grass003" };
        private static readonly string[] Layers = { "Meadow", "WoodlandFloor", "Alluvium", "WetMeadow", "ExposedRock", "DryGrass" };
        private static readonly float[] TileMetres = { 1.4f, 2.1f, 3.5f, 1.4f, 3f, 1.4f };
        private static readonly float[] NormalStrength = { 0.7f, 0.8f, 0.8f, 0.85f, 1f, 0.65f };
        internal static bool Available => File.Exists(Sources + "manifest.json");

        [MenuItem("Dewabhumi/Landscape/Apply Free PBR Terrain Textures %#F7")]
        internal static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before applying terrain textures.");
            if (SceneManager.GetActiveScene().path != "Assets/Scenes/GopalaKathmandu.unity")
                throw new InvalidOperationException("Open the GopalaKathmandu preview scene first.");
            if (!Available) throw new FileNotFoundException("Run python tools/fetch_terrain_textures.py first.");
            try
            {
                TerrainLayer[] layers = BuildLayers();
                var geography = new KathmanduGeography();
                foreach (Terrain terrain in Terrain.activeTerrains)
                {
                    string path = AssetDatabase.GetAssetPath(terrain.terrainData);
                    if (!path.StartsWith(Generated, StringComparison.Ordinal)) continue;
                    bool detailed = terrain.terrainData.size.x < 10000;
                    terrain.terrainData.terrainLayers = layers;
                    Vector3 p = terrain.transform.position;
                    GopalaLandscapeBuilder.PaintTerrain(geography, terrain.terrainData, new Vector2(p.x, p.z), terrain.terrainData.size.x, detailed);
                    terrain.materialTemplate = SurfaceMaterial();
                    // Keep the settlement view in one shading regime. Switching to the
                    // basemap halfway through the view exposes terrain patch boundaries.
                    terrain.basemapDistance = 10000;
                    terrain.Flush();
                    EditorUtility.SetDirty(terrain.terrainData);
                }
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                GopalaLandscapeBuilder.Capture();
                Debug.Log("[GopalaLandscape] Applied six CC0 ambientCG PBR sets, normal maps and packed terrain masks.");
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        internal static Material SurfaceMaterial()
        {
            string path = Generated + "TerrainSurface.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit")) { name = "TerrainSurface" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_MASKMAP");
            material.EnableKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
            // URP's height blend is limited to four layers; our six-layer terrain uses normalized control maps.
            material.DisableKeyword("_TERRAIN_BLEND_HEIGHT");
            material.SetFloat("_EnableHeightBlend", 0);
            material.SetFloat("_EnableInstancedPerPixelNormal", 1);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        internal static TerrainLayer[] BuildLayers()
        {
            var result = new TerrainLayer[Assets.Length];
            Directory.CreateDirectory(Generated);
            for (int i = 0; i < Assets.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Free terrain textures", Assets[i] + " / normal and roughness detail", i / 6f);
                string prefix = Sources + Assets[i] + "/" + Assets[i] + "_2K-JPG_";
                Texture2D color = Import(prefix + "Color.jpg", true, false);
                Texture2D normal = Import(prefix + "NormalGL.jpg", false, true);
                string maskPath = Generated + Layers[i] + "_Mask.png";
                PackMask(prefix, maskPath);
                Texture2D mask = Import(maskPath, false, false);
                string layerPath = Generated + Layers[i] + ".terrainlayer";
                var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
                if (layer == null)
                {
                    layer = new TerrainLayer { name = Layers[i] };
                    AssetDatabase.CreateAsset(layer, layerPath);
                }
                layer.diffuseTexture = color;
                layer.normalMapTexture = normal;
                layer.normalScale = NormalStrength[i];
                layer.maskMapTexture = mask;
                layer.tileSize = Vector2.one * TileMetres[i];
                layer.tileOffset = new Vector2(i * 0.37f, i * 0.61f);
                layer.metallic = 0;
                layer.smoothness = 0;
                layer.diffuseRemapMin = Vector4.zero;
                layer.diffuseRemapMax = i == 5 ? new Vector4(0.90f, 0.85f, 0.74f, 1) :
                    i == 0 ? new Vector4(0.88f, 0.91f, 0.95f, 1) :
                    i == 2 ? new Vector4(0.82f, 0.85f, 0.89f, 1) : Vector4.one;
                layer.maskMapRemapMin = Vector4.zero;
                layer.maskMapRemapMax = Vector4.one;
                EditorUtility.SetDirty(layer);
                result[i] = layer;
            }
            return result;
        }

        private static Texture2D Import(string path, bool color, bool normal)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Missing ambientCG PBR map", path);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = color;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 16;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.isReadable = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Color32[] ReadPixels(string path, out int width, out int height)
        {
            var image = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                if (!image.LoadImage(File.ReadAllBytes(path))) throw new InvalidDataException(path);
                width = image.width; height = image.height;
                return image.GetPixels32();
            }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }

        private static void PackMask(string prefix, string path)
        {
            var roughness = ReadPixels(prefix + "Roughness.jpg", out int width, out int height);
            var occlusion = ReadPixels(prefix + "AmbientOcclusion.jpg", out int aoWidth, out int aoHeight);
            var displacement = ReadPixels(prefix + "Displacement.jpg", out int dWidth, out int dHeight);
            if (aoWidth != width || aoHeight != height || dWidth != width || dHeight != height)
                throw new InvalidDataException("PBR map dimensions must match.");
            var pixels = new Color32[roughness.Length];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, occlusion[i].r, displacement[i].r, (byte)(255 - roughness[i].r));
            var output = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                output.SetPixels32(pixels);
                output.Apply();
                File.WriteAllBytes(path, output.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(output); }
        }
    }
}

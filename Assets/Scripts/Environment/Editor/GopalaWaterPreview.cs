using System;
using System.IO;
using Dewabhumi.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Dewabhumi.Environment.Editor
{
    /// <summary>Isolated LazySakana surface-water evaluation on the mapped Bagmati.</summary>
    public static class GopalaWaterPreview
    {
        internal const string PreviewScene = "Assets/Scenes/GopalaKathmanduWater.unity";
        private const string OriginalScene = "Assets/Scenes/GopalaKathmandu.unity";
        private const string MaterialPath = "Assets/Art/Environment/GopalaLandscape/Bagmati_LazySakana.mat";

        [MenuItem("Dewabhumi/Landscape/Test Free Water System %#F6")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before preparing the water preview.");
            var original = SceneManager.GetActiveScene();
            if (original.path != OriginalScene && original.path != PreviewScene)
                throw new InvalidOperationException("Open GopalaKathmandu or GopalaKathmanduWater first.");
            Material preset = AssetDatabase.LoadAssetAtPath<Material>("Assets/WaterShowcase/M_Water_Realistic_Plane.mat");
            if (preset == null || preset.shader == null || !preset.shader.isSupported)
                throw new InvalidOperationException("LazySakana Realistic Plane shader is not imported or supported.");
            if (original.path != PreviewScene)
            {
                if (File.Exists(PreviewScene))
                {
                    string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/_Recovery/BeforeWaterPreview.unity");
                    File.Copy(PreviewScene, backup);
                    AssetDatabase.ImportAsset(backup);
                }
                if (!EditorSceneManager.SaveScene(original, PreviewScene, true)) throw new IOException("Could not create isolated water preview.");
                EditorSceneManager.OpenScene(PreviewScene, OpenSceneMode.Single);
            }

            MeshRenderer selected = null;
            float nearest = float.PositiveInfinity;
            var camp = KathmanduGeography.Camp;
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.name.StartsWith("Bagmati - ", StringComparison.Ordinal)) continue;
                if (!renderer.TryGetComponent<MeshFilter>(out var filter) || filter.sharedMesh == null) continue;
                foreach (Vector3 vertex in filter.sharedMesh.vertices)
                {
                    Vector3 world = renderer.transform.TransformPoint(vertex);
                    float distance = (new Vector2(world.x, world.z) - camp).sqrMagnitude;
                    if (distance >= nearest) continue;
                    nearest = distance;
                    selected = renderer;
                }
            }
            if (selected == null) throw new InvalidOperationException("No mapped Bagmati surface found.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(preset) { name = "Bagmati_LazySakana" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.CopyPropertiesFromMaterial(preset);
            // River-scale tuning. Keep the supplied presets intact and avoid ocean-sized swells.
            material.SetFloat("_Swell_Height", 0.025f);
            material.SetFloat("_Swell_Length", 8);
            material.SetFloat("_Swell_Speed", 0.4f);
            material.SetFloat("_Wave_1_Height", 0.015f);
            material.SetFloat("_Wave_2_Height", 0.02f);
            material.SetFloat("_Enable_Crest_Foam", 0);
            material.SetFloat("_Enable_Coastal_Foam", 0);
            material.SetFloat("_Contact_Width", 0.55f);
            material.SetFloat("_Contact_Intensity", 0.4f);
            material.SetFloat("_Surface_Foam_Amount", 0.18f);
            material.SetFloat("_Surface_Foam_Intensity", 0.18f);
            material.SetFloat("_Normal_Strength", 0.13f);
            material.SetFloat("_Depth_Density", 0.35f);
            material.SetFloat("_Water_Opacity", 0.8f);
            material.SetFloat("_Caustics_Strength", 0.08f);
            material.SetFloat("_Refraction_Strength", 0.008f);
            material.SetColor("_Water_Color", new Color(0.12f, 0.29f, 0.24f, 1));
            material.SetColor("_Deep_Color", new Color(0.035f, 0.10f, 0.085f, 1));
            material.SetColor("_Water_Transmission", new Color(0.50f, 0.65f, 0.57f, 1));
            material.SetColor("_Foam_Color", new Color(0.72f, 0.76f, 0.67f, 1));
            material.SetVector("_Wave_1_Direction", new Vector4(0, -1, 0, 0));
            material.SetVector("_Wave_2_Direction", new Vector4(0.2f, -1, 0, 0));
            material.SetVector("_Surface_Foam_Direction", new Vector4(0, -1, 0, 0));
            selected.sharedMaterial = material;
            EditorUtility.SetDirty(material);

            Camera camera = Camera.main;
            if (camera == null || !camera.TryGetComponent<KathmanduLandscapeCamera>(out var navigator))
                throw new InvalidOperationException("Landscape preview camera is missing.");
            var data = camera.GetUniversalAdditionalCameraData();
            // Camera-local buffers are sufficient for the plane shader; don't run the
            // vendor's volume setup menu, which alters project layers/renderers.
            data.requiresDepthOption = CameraOverrideOption.On;
            data.requiresColorOption = CameraOverrideOption.On;
            camera.depthTextureMode |= DepthTextureMode.Depth;
            navigator.SetCloseView();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("reports/GopalaLandscape/Water");
            File.WriteAllText("reports/GopalaLandscape/Water/setup.json", JsonUtility.ToJson(new SetupReport {
                scene = PreviewScene, river = selected.name, material = MaterialPath, shader = material.shader.name,
                nearestRiverVertexMetres = Mathf.Sqrt(nearest), depthTexture = data.requiresDepthTexture,
                opaqueTexture = data.requiresColorTexture, pipelineSettingsChanged = false
            }, true));
            Debug.Log("[GopalaWater] LazySakana surface test prepared on " + selected.name + ". Camera-local depth/color buffers enabled.");
            GopalaLandscapeBuilder.Capture();
        }

        [Serializable] private sealed class SetupReport
        {
            public string scene, river, material, shader;
            public float nearestRiverVertexMetres;
            public bool depthTexture, opaqueTexture, pipelineSettingsChanged;
        }
    }
}

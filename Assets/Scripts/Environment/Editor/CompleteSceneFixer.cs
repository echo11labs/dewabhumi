using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Dewabhumi.Presentation;
using Dewabhumi.Settlement;
using Dewabhumi.Settlement.Editor;
using Dewabhumi.Citizens;
using Dewabhumi.Economy;
using Dewabhumi.Pastoral;

namespace Dewabhumi.Environment.Editor
{
    public static class CompleteSceneFixer
    {
        private const string ScenePath = "Assets/Scenes/MainSettlement.unity";
        private const string TerrainDataPath = "Assets/Art/Environment/KathmanduTerrainData.asset";

        // Scene reconstruction is an explicit menu action. Domain reloads must never
        // replace an unsaved scene or regenerate terrain assets as a side effect.

        [MenuItem("Dewabhumi/Fix & Rebuild Complete Scene")]
        public static void RebuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                Debug.LogWarning("[CompleteSceneFixer] Stopped play mode. Click 'Dewabhumi -> Fix & Rebuild Complete Scene' again to rebuild.");
                return;
            }

            Debug.Log("[CompleteSceneFixer] Starting complete Kathmandu Valley landscape rebuild...");

            // 1. Ensure URP Pipeline Asset is configured
            SetupUniversalRenderPipeline();

            // 2. Ensure Building Definitions exist with 3D models assigned
            DefaultBuildingDefinitionsCreator.CreateDefinitions();

            // 3. Create Scene directory if needed
            string sceneDir = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(sceneDir))
            {
                Directory.CreateDirectory(sceneDir);
            }

            // 4. Setup New Empty Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 5. Directional Sun Light (Warm Himalayan Sunlight)
            GameObject lightObj = new GameObject("Sun_DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.95f, 0.88f); // Warm Himalayan sun (5200K)
            light.intensity = 1.35f;
            light.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 6. Generate and Persist TerrainData Asset
            Terrain terrain = GenerateAndPersistTerrain();

            // Founder's Tar center coordinates (elevated habitable tableland)
            Vector3 centerPos = new Vector3(256f, 0f, 230f);
            centerPos.y = terrain.SampleHeight(centerPos);

            // 7. Camera Rig with 2.5D Isometric Orientation
            GameObject camRig = new GameObject("CameraRig_Isometric");
            camRig.transform.position = centerPos;
            camRig.transform.rotation = Quaternion.Euler(35f, 45f, 0f); // 35 deg pitch, 45 deg azimuth
            camRig.AddComponent<WorldViewCamera>();

            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(camRig.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0f, -48f); // 48m distance looking down
            camObj.transform.localRotation = Quaternion.identity;

            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<UniversalAdditionalCameraData>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.53f, 0.72f, 0.88f); // Himalayan azure mountain sky
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = 34f; // Low FOV for isometric perspective feel

            // 8. Settlement Center Anchor
            GameObject settlementCenter = new GameObject("Settlement_Center");
            settlementCenter.transform.position = centerPos;

            // Visual Founder's Chautara Platform (10m circular stone platform)
            GameObject chautara = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chautara.name = "Visual_FoundersChautara";
            chautara.transform.SetParent(settlementCenter.transform, false);
            chautara.transform.localScale = new Vector3(10.0f, 0.40f, 10.0f);
            chautara.transform.localPosition = new Vector3(0f, 0.20f, 0f);
            var chautaraRen = chautara.GetComponent<Renderer>();
            chautaraRen.material.color = new Color(0.48f, 0.44f, 0.40f); // Himalayan cut stone
            Object.DestroyImmediate(chautara.GetComponent<Collider>());

            // Center Stupa on Chautara
            SpawnPrefabAt("Assets/Art/Props/Models/prayer-stupa.glb", chautara.transform, new Vector3(0f, 0.25f, 0f), Vector3.one * 1.2f, null, isLocal: true);
            // Prayer Flag Line across the Chautara
            SpawnPrefabAt("Assets/Art/Props/Models/prayer-flag-line.glb", chautara.transform, new Vector3(0f, 0.25f, 0f), Vector3.one * 1.5f, null, isLocal: true);
            // Wooden Benches for resting elders
            SpawnPrefabAt("Assets/Art/Props/Models/wooden-bench.glb", chautara.transform, new Vector3(3.2f, 0.25f, 1.5f), Vector3.one * 1.1f, Quaternion.Euler(0f, -90f, 0f), isLocal: true);
            SpawnPrefabAt("Assets/Art/Props/Models/wooden-bench.glb", chautara.transform, new Vector3(-3.2f, 0.25f, -1.5f), Vector3.one * 1.1f, Quaternion.Euler(0f, 90f, 0f), isLocal: true);

            // 9. Grid Visualizer & Placement Controller
            settlementCenter.AddComponent<GridVisualizer>();
            settlementCenter.AddComponent<BlueprintPlacementController>();

            // 10. Storage Manager (spawns starting OpenStockpile & DryGranary)
            settlementCenter.AddComponent<SettlementStorageManager>();

            // 11. Population Manager (Citizens & Cattle)
            SettlementPopulationManager popManager = settlementCenter.AddComponent<SettlementPopulationManager>();
            popManager.SpawnFoundingClan();

            // 12. Resource Harvesting Manager (Sal trees, boulders, berry bushes, reeds)
            ResourceHarvestingManager resManager = settlementCenter.AddComponent<ResourceHarvestingManager>();
            resManager.SpawnInitialValleyResources();

            // 13. Desktop HUD Manager
            settlementCenter.AddComponent<SettlementHUDManager>();

            // 14. Environmental Props & 3D Terraced Rice Fields on Foothills
            SpawnEnvironmentalLandmarks(terrain);

            // 15. Save Scene & Assets
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 16. Open the newly built scene in the active editor
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Frame(new Bounds(centerPos, new Vector3(45f, 20f, 45f)), false);
            }

            Debug.Log($"[CompleteSceneFixer] Successfully rebuilt scene at {ScenePath}. Kathmandu Valley terrain is complete!");
        }

        private static void SpawnEnvironmentalLandmarks(Terrain terrain)
        {
            GameObject envRoot = new GameObject("Environment_Landmarks");

            // 1. Terraced Rice Fields on the lower foothill slopes
            Vector3 t1Pos = new Vector3(290f, 0f, 280f);
            t1Pos.y = terrain.SampleHeight(t1Pos);
            SpawnPrefabAt("Assets/Art/Environment/Models/rice-terrace-stepped-3tier.glb", envRoot.transform, t1Pos, Vector3.one * 2.2f, Quaternion.Euler(0f, 15f, 0f));

            Vector3 t2Pos = new Vector3(240f, 0f, 295f);
            t2Pos.y = terrain.SampleHeight(t2Pos);
            SpawnPrefabAt("Assets/Art/Environment/Models/rice-terrace-large-flat.glb", envRoot.transform, t2Pos, Vector3.one * 2.0f, Quaternion.Euler(0f, -20f, 0f));

            Vector3 t3Pos = new Vector3(330f, 0f, 260f);
            t3Pos.y = terrain.SampleHeight(t3Pos);
            SpawnPrefabAt("Assets/Art/Environment/Models/rice-terrace-curved-2tier.glb", envRoot.transform, t3Pos, Vector3.one * 2.0f, Quaternion.Euler(0f, 45f, 0f));

            // 2. Starting Dug Well (Kuwa) near the settlement
            Vector3 wellPos = new Vector3(245f, 0f, 236f);
            wellPos.y = terrain.SampleHeight(wellPos);
            SpawnPrefabAt("Assets/Art/Props/Models/roofed-well.glb", envRoot.transform, wellPos, Vector3.one * 1.3f, Quaternion.Euler(0f, 30f, 0f));

            // 3. Cattle Pasture Rail Fence & Water Trough
            Vector3 pasturePos = new Vector3(266f, 0f, 238f);
            pasturePos.y = terrain.SampleHeight(pasturePos);
            SpawnPrefabAt("Assets/Art/Props/Models/wooden-rail-fence.glb", envRoot.transform, pasturePos, Vector3.one * 1.5f, Quaternion.Euler(0f, 45f, 0f));
            SpawnPrefabAt("Assets/Art/Props/Models/water-trough.glb", envRoot.transform, pasturePos + new Vector3(2f, 0f, 1f), Vector3.one * 1.2f);

            // 4. Starting Stockpile timber logs & boulders
            Vector3 stockPad = new Vector3(268f, 0f, 222f);
            stockPad.y = terrain.SampleHeight(stockPad);
            SpawnPrefabAt("Assets/Art/Props/Models/log-pile.glb", envRoot.transform, stockPad, Vector3.one * 1.3f);
            SpawnPrefabAt("Assets/Art/Props/Models/stone-pile.glb", envRoot.transform, stockPad + new Vector3(3.5f, 0f, 0f), Vector3.one * 1.3f);
        }

        private static GameObject SpawnPrefabAt(string assetPath, Transform parent, Vector3 pos, Vector3 scale, Quaternion? rot = null, bool isLocal = false)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                if (isLocal)
                {
                    instance.transform.localPosition = pos;
                }
                else
                {
                    instance.transform.position = pos;
                }
                instance.transform.localScale = scale;
                if (rot.HasValue) instance.transform.rotation = rot.Value;
                return instance;
            }
            return null;
        }

        private static void SetupUniversalRenderPipeline()
        {
            string settingsDir = "Assets/Settings";
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }

            string pipelineAssetPath = Path.Combine(settingsDir, "Dewabhumi_URP.asset");
            UniversalRenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelineAssetPath);

            if (urpAsset == null)
            {
                var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, Path.Combine(settingsDir, "Dewabhumi_Renderer.asset"));

                urpAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(urpAsset, pipelineAssetPath);
                AssetDatabase.SaveAssets();
            }

            GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
        }

        private static Terrain GenerateAndPersistTerrain()
        {
            string envDir = "Assets/Art/Environment";
            if (!Directory.Exists(envDir))
            {
                Directory.CreateDirectory(envDir);
            }

            TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);

            if (terrainData == null)
            {
                terrainData = new TerrainData();
                AssetDatabase.CreateAsset(terrainData, TerrainDataPath);
            }

            GameObject terrainObj = new GameObject("KathmanduValleyTerrain");
            Terrain terrain = terrainObj.AddComponent<Terrain>();
            TerrainCollider collider = terrainObj.AddComponent<TerrainCollider>();

            terrain.terrainData = terrainData;
            collider.terrainData = terrainData;

            KathmanduTerrainGenerator generator = terrainObj.AddComponent<KathmanduTerrainGenerator>();
            generator.GenerateKathmanduTerrain();

            // Assign official URP TerrainLit material to guarantee valid shaders
            Material urpTerrainMat = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/TerrainLit.mat");
            terrain.materialTemplate = urpTerrainMat;

            EditorUtility.SetDirty(terrainData);
            AssetDatabase.SaveAssets();

            return terrain;
        }
    }
}

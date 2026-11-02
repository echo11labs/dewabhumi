using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Dewabhumi.Environment.Editor
{
    [CustomEditor(typeof(KathmanduTerrainGenerator))]
    public class KathmanduTerrainGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            KathmanduTerrainGenerator generator = (KathmanduTerrainGenerator)target;

            EditorGUILayout.Space(12);
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f, 1f);
            if (GUILayout.Button("Generate Kathmandu Valley Terrain", GUILayout.Height(36)))
            {
                AutoAssignTexturesIfMissing(generator);
                generator.GenerateKathmanduTerrain();
                EditorUtility.SetDirty(generator);
                EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
            }
            GUI.backgroundColor = Color.white;
        }

        [MenuItem("Dewabhumi/Generate Kathmandu Valley Terrain")]
        public static void GenerateTerrainMenu()
        {
            KathmanduTerrainGenerator generator = FindAnyObjectByType<KathmanduTerrainGenerator>();
            if (generator == null)
            {
                GameObject obj = new GameObject("KathmanduValleyTerrain");
                generator = obj.AddComponent<KathmanduTerrainGenerator>();
                Undo.RegisterCreatedObjectUndo(obj, "Create Kathmandu Terrain");
            }

            AutoAssignTexturesIfMissing(generator);
            generator.GenerateKathmanduTerrain();
            Selection.activeGameObject = generator.gameObject;
            EditorUtility.SetDirty(generator);
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
        }

        private static void AutoAssignTexturesIfMissing(KathmanduTerrainGenerator generator)
        {
            SerializedObject so = new SerializedObject(generator);

            AssignTextureIfNull(so, "_grassTexture", "terrain-tile-plain-grass");
            AssignTextureIfNull(so, "_rockyDirtTexture", "terrain-tile-rocky-dirt");
            AssignTextureIfNull(so, "_forestFloorTexture", "terrain-tile-forest-floor");
            AssignTextureIfNull(so, "_mossyRockTexture", "terrain-tile-mossy-rock-mound");

            so.ApplyModifiedProperties();
        }

        private static void AssignTextureIfNull(SerializedObject so, string propertyName, string textureName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null && prop.objectReferenceValue == null)
            {
                string[] guids = AssetDatabase.FindAssets($"{textureName} t:Texture2D");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null)
                    {
                        prop.objectReferenceValue = tex;
                    }
                }
            }
        }
    }
}

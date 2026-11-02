using UnityEditor;

namespace Dewabhumi.Environment.Editor
{
    public static class SettlementSceneInitializer
    {
        [MenuItem("Dewabhumi/Setup Main Settlement Scene")]
        public static void SetupScene()
        {
            CompleteSceneFixer.RebuildScene();
        }
    }
}

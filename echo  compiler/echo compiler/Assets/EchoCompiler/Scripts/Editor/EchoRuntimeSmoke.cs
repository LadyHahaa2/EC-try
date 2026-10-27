#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoCompiler.Editor
{
    public static class EchoRuntimeSmoke
    {
        private static double startedAt;
        private static int exitCode;

        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            exitCode = 0;
            startedAt = EditorApplication.timeSinceStartup;
            EditorApplication.update += Poll;
            EditorApplication.isPlaying = true;
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup - startedAt < 2.5) return;
            var manager = Object.FindObjectOfType<RunManager>();
            var loader = Object.FindObjectOfType<LevelLoader>();
            if (manager == null || loader == null || manager.CurrentLevel == null)
            {
                Debug.LogError("Echo runtime smoke test failed: bootstrap did not create runtime services.");
                exitCode = 1;
            }
            else
            {
                Debug.Log("Echo runtime smoke test passed: " + manager.CurrentLevel.levelId + " / " + manager.State);
            }

            EditorApplication.update -= Poll;
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
        }
    }
}
#endif

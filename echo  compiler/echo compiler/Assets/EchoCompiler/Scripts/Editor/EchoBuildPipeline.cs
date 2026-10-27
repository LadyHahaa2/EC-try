#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoCompiler.Editor
{
    public static class EchoBuildPipeline
    {
        [MenuItem("Tools/Echo/Build Windows Player")]
        public static void BuildWindowsPlayer()
        {
            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "EchoCompiler_Windows"));
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/SampleScene.unity" }, Path.Combine(output, "EchoCompiler.exe"), BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("Echo Compiler Windows build failed: " + report.summary.result);
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("Echo Compiler Windows build succeeded: " + report.summary.totalSize + " bytes at " + output);
            EditorApplication.Exit(0);
        }
    }
}
#endif

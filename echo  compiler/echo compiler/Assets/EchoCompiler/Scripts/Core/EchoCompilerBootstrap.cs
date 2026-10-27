using UnityEngine;

namespace EchoCompiler
{
    public sealed class EchoCompilerBootstrap : MonoBehaviour
    {
        [Tooltip("Ordered LevelDefinition assets used by the runtime. Missing entries use the documented fallback layouts.")]
        public LevelDefinition[] levelDefinitions = new LevelDefinition[0];

        private void Start()
        {
            var input = FindObjectOfType<InputRouter>() ?? new GameObject("InputRouter").AddComponent<InputRouter>();
            var loader = FindObjectOfType<LevelLoader>() ?? new GameObject("LevelLoader").AddComponent<LevelLoader>();
            loader.ConfigureDefinitions(levelDefinitions);
            var manager = FindObjectOfType<RunManager>();
            if (manager == null) manager = new GameObject("RunManager").AddComponent<RunManager>();
            var hud = FindObjectOfType<HUDController>() ?? new GameObject("HUDController").AddComponent<HUDController>();
            manager.Configure(input, loader, hud);
            manager.StartNewRun();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<EchoCompilerBootstrap>() != null) return;
            new GameObject("EchoCompilerBootstrap").AddComponent<EchoCompilerBootstrap>();
        }
    }
}

using UnityEngine;

namespace EchoCompiler.Tests
{
    public sealed class EchoCompilerSmokeSuite : MonoBehaviour
    {
        [ContextMenu("Run 8+3 Echo Compiler Smoke Tests")]
        public void RunAll()
        {
            var passed = 0;
            var total = 11;
            var buffer = new RecordingBuffer(4);
            buffer.Begin();
            for (var i = 0; i < 8; i++) buffer.Sample(i * 0.02f, Vector2.right, i == 3);
            if (buffer.Count == 4) passed++;
            if (buffer.CopyFrames().Length == 4) passed++;
            buffer.Clear();
            if (buffer.Count == 0) passed++;
            var frame = new InputFrame(0.2f, new Vector2(0.5f, -0.5f), true);
            if (frame.interact && frame.move.sqrMagnitude > 0f) passed++;

            var loader = gameObject.GetComponent<LevelLoader>() ?? gameObject.AddComponent<LevelLoader>();
            var ids = new System.Collections.Generic.HashSet<string>();
            var allDefinitionsValid = true;
            for (var i = 0; i < 3; i++)
            {
                var level = loader.CreateDefaultDefinition(i);
                if (ids.Add(level.levelId)) passed++;
                allDefinitionsValid &= level.roomSize.x > 0f && level.roomSize.y > 0f && level.echoLimit <= 3 && level.echoLimit >= 0;
                DestroyImmediate(level);
            }
            if (allDefinitionsValid) passed++;

            var manager = FindObjectOfType<RunManager>();
            if (manager != null) passed++;
            if (manager != null && manager.CurrentLevel != null) passed++;
            if (FindObjectOfType<HUDController>() != null) passed++;
            Debug.Log(string.Format("Echo Compiler smoke suite: {0}/{1} checks passed.", passed, total));
        }
    }
}

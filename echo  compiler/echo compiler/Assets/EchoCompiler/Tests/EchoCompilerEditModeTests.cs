using UnityEngine;

namespace EchoCompiler.Tests
{
    public sealed class EchoCompilerEditModeTests : MonoBehaviour
    {
        [ContextMenu("Run Echo Compiler Smoke Tests")]
        public void RunSmokeTests()
        {
            var buffer = new RecordingBuffer(4);
            buffer.Begin();
            for (var i = 0; i < 10; i++) buffer.Sample(i * 0.02f, Vector2.right, false);
            Debug.Assert(buffer.Count == 4, "RecordingBuffer must cap samples.");
            Debug.Assert(buffer.CopyFrames().Length == 4, "RecordingBuffer copy length must match count.");

            var loader = gameObject.GetComponent<LevelLoader>() ?? gameObject.AddComponent<LevelLoader>();
            for (var i = 0; i < 3; i++)
            {
                var level = loader.CreateDefaultDefinition(i);
                Debug.Assert(!string.IsNullOrEmpty(level.levelId), "Default level ID is required.");
                Debug.Assert(level.roomSize.x > 0f && level.roomSize.y > 0f, "Default level bounds are required.");
                Debug.Assert(level.echoLimit <= 3, "Echo limit must stay within MVP limit.");
                DestroyImmediate(level);
            }
            Debug.Log("Echo Compiler smoke tests passed.");
        }
    }
}

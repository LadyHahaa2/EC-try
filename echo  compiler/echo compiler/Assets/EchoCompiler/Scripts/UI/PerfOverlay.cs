using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace EchoCompiler
{
    public sealed class PerfOverlay : MonoBehaviour
    {
        private float elapsed;
        private int frames;
        private float fps;
        private ProfilerRecorder gcAllocRecorder;
        private ProfilerRecorder batchesRecorder;

        public string Snapshot { get; private set; } = "FPS -- | GC --";

        private void OnEnable()
        {
            try
            {
                gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc");
                batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            }
            catch (System.Exception)
            {
                gcAllocRecorder = default(ProfilerRecorder);
                batchesRecorder = default(ProfilerRecorder);
            }
        }

        private void OnDisable()
        {
            if (gcAllocRecorder.Valid) gcAllocRecorder.Dispose();
            if (batchesRecorder.Valid) batchesRecorder.Dispose();
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            frames++;
            if (elapsed < 0.5f) return;
            fps = frames / elapsed;
            frames = 0;
            elapsed = 0f;
            var gcBytes = gcAllocRecorder.Valid ? gcAllocRecorder.LastValue : 0L;
            var batches = batchesRecorder.Valid ? batchesRecorder.LastValue : 0L;
            Snapshot = string.Format("FPS {0:0} | GC/frame {1:0.0} KB | Batches {2} | Mem {3:0.0} MB", fps, gcBytes / 1024f, batches, Profiler.GetMonoUsedSizeLong() / (1024f * 1024f));
        }
    }
}

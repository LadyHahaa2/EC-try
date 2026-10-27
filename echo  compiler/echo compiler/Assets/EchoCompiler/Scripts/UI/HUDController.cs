using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EchoCompiler
{
    public sealed class HUDController : MonoBehaviour
    {
        private Canvas canvas;
        private TMP_Text roomText;
        private TMP_Text stateText;
        private TMP_Text objectiveText;
        private TMP_Text perfText;
        private ReplayTimelineUI timeline;
        private MainMenu menu;
        private Result result;
        private GameObject pausePanel;
        private PerfOverlay perfOverlay;
        private LevelLoader loader;
        private float fpsTimer;
        private int fpsFrames;
        private float fps;

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            perfOverlay = gameObject.AddComponent<PerfOverlay>();
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            roomText = AddText("RoomText", canvas.transform, 24, new Color(0.82f, 0.95f, 1f, 1f), TextAnchor.UpperLeft, new Vector2(30f, -30f), new Vector2(700f, 40f));
            stateText = AddText("StateText", canvas.transform, 18, new Color(0.35f, 0.95f, 0.87f, 1f), TextAnchor.UpperLeft, new Vector2(30f, -72f), new Vector2(900f, 34f));
            objectiveText = AddText("ObjectiveText", canvas.transform, 16, new Color(0.78f, 0.84f, 0.92f, 1f), TextAnchor.UpperRight, new Vector2(-30f, -32f), new Vector2(800f, 60f));
            objectiveText.rectTransform.anchorMin = new Vector2(1f, 1f);
            objectiveText.rectTransform.anchorMax = new Vector2(1f, 1f);
            objectiveText.rectTransform.pivot = new Vector2(1f, 1f);
            perfText = AddText("PerfText", canvas.transform, 12, new Color(0.46f, 0.56f, 0.65f, 1f), TextAnchor.LowerRight, new Vector2(-30f, 30f), new Vector2(500f, 30f));
            perfText.rectTransform.anchorMin = new Vector2(1f, 0f);
            perfText.rectTransform.anchorMax = new Vector2(1f, 0f);
            perfText.rectTransform.pivot = new Vector2(1f, 0f);

            var timelineGo = new GameObject("ReplayTimelineUI");
            timelineGo.transform.SetParent(canvas.transform, false);
            timeline = timelineGo.AddComponent<ReplayTimelineUI>();
            timeline.Build(canvas.transform);

            var menuGo = new GameObject("MainMenu");
            menuGo.transform.SetParent(canvas.transform, false);
            menu = menuGo.AddComponent<MainMenu>();
            menu.Build(canvas.transform);

            var resultGo = new GameObject("Result");
            resultGo.transform.SetParent(canvas.transform, false);
            result = resultGo.AddComponent<Result>();
            result.Build(resultGo.transform);

            pausePanel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            pausePanel.transform.SetParent(canvas.transform, false);
            var pauseRect = pausePanel.GetComponent<RectTransform>();
            pauseRect.anchorMin = Vector2.zero;
            pauseRect.anchorMax = Vector2.one;
            pauseRect.offsetMin = Vector2.zero;
            pauseRect.offsetMax = Vector2.zero;
            pausePanel.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.06f, 0.82f);
            var pauseText = AddText("PauseText", pausePanel.transform, 30, new Color(0.82f, 0.95f, 1f, 1f), TextAnchor.MiddleCenter, Vector2.zero, new Vector2(760f, 120f));
            pauseText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            pauseText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            pauseText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            pauseText.rectTransform.anchoredPosition = Vector2.zero;
            pauseText.text = "PAUSED\nPress ESC or P to resume";
            pausePanel.SetActive(false);
        }

        private static TMP_Text AddText(string name, Transform parent, int fontSize, Color color, TextAnchor alignment, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = ToTmpAlignment(alignment);
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return text;
        }

        private static TextAlignmentOptions ToTmpAlignment(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                default: return TextAlignmentOptions.TopLeft;
            }
        }

        public void ShowRoom(string name)
        {
            if (roomText != null) roomText.text = "ECHO COMPILER  //  " + name;
            if (result != null) result.Hide();
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void Bind(LevelLoader levelLoader)
        {
            loader = levelLoader;
        }

        public void Refresh(RunManager manager)
        {
            if (manager == null) return;
            if (stateText != null) stateText.text = string.Format("STATE {0}    TIME {1:0.0}s / {2:0}s", manager.State.ToString().ToUpperInvariant(), manager.LevelTime, manager.CurrentLevel == null ? 0f : manager.CurrentLevel.timeLimit);
            var switchTotal = manager.CurrentLevel == null || manager.CurrentLevel.switches == null ? 0 : manager.CurrentLevel.switches.Length;
            if (objectiveText != null) objectiveText.text = string.Format("NODES {0}/{1}    SIGNAL {2}/{3}    ECHOES {4}/{5}", manager.CurrentLevel == null ? 0 : CountActive(), switchTotal, manager.CollectedShards, manager.ShardTotal, manager.EchoCount, manager.MaxEchoes);
            if (timeline != null) timeline.Refresh(manager.RecordingProgress, manager.EchoCount);
            fpsFrames++;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 0.5f)
            {
                fps = fpsFrames / fpsTimer;
                fpsFrames = 0;
                fpsTimer = 0f;
            }
            if (perfText != null) perfText.text = (perfOverlay == null ? string.Format("FPS {0:0}", fps) : perfOverlay.Snapshot) + "  |  generated sprites  |  no external assets";
        }

        private int CountActive()
        {
            var count = 0;
            if (loader == null) return count;
            for (var i = 0; i < loader.Switches.Count; i++) if (loader.Switches[i].IsActive) count++;
            return count;
        }

        public void ShowResult(bool success, string message)
        {
            if (result != null) result.Show(success, message);
        }

        public void ShowPause(bool paused)
        {
            if (pausePanel != null) pausePanel.SetActive(paused);
        }
    }
}

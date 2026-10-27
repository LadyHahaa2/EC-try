using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EchoCompiler
{
    public sealed class ReplayTimelineUI : MonoBehaviour
    {
        private Image fill;
        private TMP_Text label;

        public void Build(Transform parent)
        {
            var root = new GameObject("ReplayTimeline", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 20f);
            rect.sizeDelta = new Vector2(-60f, 8f);
            root.GetComponent<Image>().color = new Color(0.12f, 0.17f, 0.26f, 0.95f);

            var fillGo = new GameObject("RecordedFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(root.transform, false);
            fill = fillGo.GetComponent<Image>();
            fill.color = new Color(0.2f, 0.95f, 0.9f, 0.85f);
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, 0f);

            var textGo = new GameObject("TimelineLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);
            label = textGo.GetComponent<TextMeshProUGUI>();
            label.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            label.fontSize = 13;
            label.color = new Color(0.66f, 0.76f, 0.86f, 1f);
            label.alignment = TextAlignmentOptions.Left;
            label.text = "RECORD 0.0 / 8.0 s";
            var labelRect = textGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 0f);
            labelRect.pivot = new Vector2(0f, 0f);
            labelRect.anchoredPosition = new Vector2(30f, 31f);
            labelRect.sizeDelta = new Vector2(220f, 20f);
        }

        public void Refresh(float progress, int echoCount)
        {
            if (fill != null)
            {
                var rect = fill.rectTransform;
                rect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            }
            if (label != null) label.text = string.Format("RECORD {0:0.0} / 8.0 s   |   ECHOES {1}/3", progress * 8f, echoCount);
        }
    }
}

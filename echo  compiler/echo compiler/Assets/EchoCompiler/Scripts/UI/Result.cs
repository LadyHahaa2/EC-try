using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EchoCompiler
{
    public sealed class Result : MonoBehaviour
    {
        private TMP_Text message;
        private Image backdrop;

        public void Build(Transform parent)
        {
            var panel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            backdrop = panel.GetComponent<Image>();
            backdrop.color = new Color(0.02f, 0.04f, 0.08f, 0.92f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 150f);

            var textGo = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(panel.transform, false);
            message = textGo.GetComponent<TextMeshProUGUI>();
            message.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            message.fontSize = 22;
            message.alignment = TextAlignmentOptions.Center;
            message.color = new Color(0.82f, 0.95f, 1f, 1f);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 15f);
            textRect.offsetMax = new Vector2(-20f, -15f);
            panel.SetActive(false);
        }

        public void Show(bool success, string text)
        {
            if (message == null) return;
            message.text = (success ? "SUCCESS\n" : "RESET\n") + text;
            message.color = success ? new Color(0.2f, 0.95f, 0.9f, 1f) : new Color(1f, 0.35f, 0.5f, 1f);
            transform.Find("ResultPanel").gameObject.SetActive(true);
        }

        public void Hide()
        {
            var panel = transform.Find("ResultPanel");
            if (panel != null) panel.gameObject.SetActive(false);
        }
    }
}

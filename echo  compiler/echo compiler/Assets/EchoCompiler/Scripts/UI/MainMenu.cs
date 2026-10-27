using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EchoCompiler
{
    public sealed class MainMenu : MonoBehaviour
    {
        private TMP_Text title;

        public void Build(Transform parent)
        {
            var go = new GameObject("MainMenuHint", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            title = go.GetComponent<TextMeshProUGUI>();
            title.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            title.fontSize = 14;
            title.color = new Color(0.48f, 0.78f, 0.9f, 1f);
            title.text = "WASD MOVE   |   SPACE COMPILE ECHO   |   R RESET   |   ESC PAUSE";
            title.alignment = TextAlignmentOptions.Center;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -18f);
            rect.sizeDelta = new Vector2(-60f, 24f);
        }

        public void SetVisible(bool visible)
        {
            if (title != null) title.enabled = visible;
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMButtonFactory
    {
        public static float RowHeight = 48f;
        public static float ConfigBtnPadX = 40f;  // left+right padding total
        public static float ConfigBtnPadY = 18f;  // top+bottom padding total
        public static float ConfigBtnMinH = 33f;  // keep a sensible minimum button height

        public static Button AddButtonSimple(
            Transform parent,
            string label,
            UnityAction onClick,
            bool interactable = true,
            Color? normalColor = null,
            Color? highlightedColor = null)
        {
            var template = ResolveTemplate();
            if (!template)
            {
                return null;
            }

            var buttonObject = InstantiateButton(template, parent, label);
            RemoveContentSizeFitter(buttonObject);

            var button = ConfigureButtonComponent(buttonObject, onClick, interactable);
            ApplyOptionalColors(button, normalColor, highlightedColor);

            var labelComponent = ConfigureLabel(buttonObject, label);

            var rectTransform = button.GetComponent<RectTransform>();
            NormalizeRectTransform(rectTransform);
            ApplySizing(rectTransform, labelComponent);

            FinalizeButton(button);

            return button;
        }

        private static GameObject ResolveTemplate()
        {
            var template = MCMWidgetLocator.GetOrResolvePopupButtonTemplate();
            if (!template)
            {
                Plugin.Log?.LogWarning("[MCM] Popup button template unavailable. Open any GenericPopUp once before building the list.");
            }

            return template;
        }

        private static GameObject InstantiateButton(GameObject template, Transform parent, string label)
        {
            var clone = UnityEngine.Object.Instantiate(template, parent, false);
            clone.name = $"MCM Button - {label}";
            clone.SetActive(true);

            return clone;
        }

        private static void RemoveContentSizeFitter(GameObject buttonObject)
        {
            var contentSizeFitter = buttonObject.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter)
            {
                UnityEngine.Object.Destroy(contentSizeFitter);
            }
        }

        private static Button ConfigureButtonComponent(GameObject buttonObject, UnityAction onClick, bool interactable)
        {
            var button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            if (onClick != null) button.onClick.AddListener(onClick);
            button.interactable = interactable;

            return button;
        }

        private static void ApplyOptionalColors(Button button, Color? normalColor, Color? highlightedColor)
        {
            if (!normalColor.HasValue && !highlightedColor.HasValue)
            {
                return;
            }

            var colors = button.colors;
            if (normalColor.HasValue) colors.normalColor = normalColor.Value;
            if (normalColor.HasValue) colors.pressedColor = normalColor.Value;
            if (highlightedColor.HasValue) colors.highlightedColor = highlightedColor.Value;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = colors;
        }

        private static TextMeshProUGUI ConfigureLabel(GameObject buttonObject, string label)
        {
            var labelComponent = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
            if (labelComponent)
            {
                labelComponent.text = label;
            }

            return labelComponent;
        }

        private static void NormalizeRectTransform(RectTransform rectTransform)
        {
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private static void ApplySizing(RectTransform rectTransform, TextMeshProUGUI labelComponent)
        {
            var preferredWidth = Mathf.Ceil(labelComponent.preferredWidth);
            var preferredHeight = Mathf.Ceil(labelComponent.preferredHeight);

            var width = preferredWidth + ConfigBtnPadX;
            var height = Mathf.Max(ConfigBtnMinH, preferredHeight + ConfigBtnPadY);

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private static void FinalizeButton(Button button)
        {
            button.transform.SetAsLastSibling();
        }
    }
}
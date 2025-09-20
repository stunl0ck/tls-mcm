using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TheLastStand.View.Generic;
using TheLastStand.View.Modding;
using Stunl0ck.ModConfigManager.DTO;
using Stunl0ck.TLS.Shared;
using TPLib;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMPopup
    {
        private const float SpacerHeight = 16f;
        private const float VerticalLayoutSpacing = 10f;
        private const float HorizontalLayoutSpacing = 20f;
        private const float ButtonRowMinHeight = 55f;
        private const float ButtonRowPrefHeight = 33f;

        public static void Show(ModsView modsView, ModConfig mod)
        {
            GenericPopUp.Open($"{mod.modName} {Localization.LocalizeOrDefault("MCM_Settings", "Settings")}", "OK", null, "OK", null);

            var popup = GetActivePopup();
            AttachCloseOnEsc(popup);

            var panel = PreparePanel(popup);
            var buttonsArea = CreateButtonsArea(panel);
            var content = CreateContentContainer(panel, buttonsArea);

            AddReloadWarningLabel(content, mod);

            var staged = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            BuildOptionsUI(content, mod, modsView, staged);

            ConfigureButtonsArea(buttonsArea);

            BuildButtonsRow(
                buttonsArea,
                onApply: () => ApplyStagedValues(mod, staged),
                onReset: () => ResetAndRebuild(content, mod, modsView, staged, panel),
                onClose: () => CloseOnlyThisPopup(popup));

            RefreshLayout(panel);
        }

        private static GenericPopUp GetActivePopup()
        {
            return UnityEngine.Object
                .FindObjectsOfType<GenericPopUp>(true)
                .First(g => g.Canvas && g.Canvas.enabled);
        }

        private static void AttachCloseOnEsc(GenericPopUp popup)
        {
            var root = popup.Canvas.transform;
            var esc = root.gameObject.AddComponent<MCMCloseOnEsc>();
            esc.popup = popup;
        }

        private static RectTransform PreparePanel(GenericPopUp popup)
        {
            var root = popup.Canvas.transform;
            var panel = root.GetComponentsInChildren<RectTransform>(true)
                .First(rt => rt.name.IndexOf("Panel", StringComparison.OrdinalIgnoreCase) >= 0
                          && rt.GetComponent<VerticalLayoutGroup>()
                          && rt.GetComponent<ContentSizeFitter>());

            HideStockDescription(panel);
            HideStockOkButton(panel);

            DestroyIfExists(panel, "MCM Content");
            DestroyIfExists(panel, "MCM Buttons Area");

            return panel;
        }

        private static void HideStockDescription(RectTransform panel)
        {
            var stockDesc = panel.Cast<Transform>()
                                 .FirstOrDefault(t => t.name.Equals("Description", StringComparison.OrdinalIgnoreCase));
            if (stockDesc)
            {
                stockDesc.gameObject.SetActive(false);
            }
        }

        private static void HideStockOkButton(RectTransform panel)
        {
            var stockOk = panel.GetComponentsInChildren<Button>(true).FirstOrDefault();
            if (stockOk)
            {
                stockOk.gameObject.SetActive(false);
            }
        }

        private static RectTransform CreateButtonsArea(RectTransform panel)
        {
            return New("MCM Buttons Area", panel, typeof(RectTransform), typeof(VerticalLayoutGroup))
                .GetComponent<RectTransform>();
        }

        private static void ConfigureButtonsArea(RectTransform buttonsArea)
        {
            var vlg = buttonsArea.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 0f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = false;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
        }

        private static void ApplyStagedValues(ModConfig mod, IDictionary<string, object> staged)
        {
            foreach (var kv in staged)
            {
                MCM.SetValue(mod.modId, kv.Key, kv.Value);
            }
        }

        private static void ResetAndRebuild(RectTransform content, ModConfig mod, ModsView modsView, Dictionary<string, object> staged, RectTransform panel)
        {
            MCM.ResetToDefaults(mod.modId);
            RebuildContent(content, mod, modsView, staged, panel);
        }

        private static void RefreshLayout(RectTransform panel)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            Canvas.ForceUpdateCanvases();
        }

        private static void CloseOnlyThisPopup(GenericPopUp popup)
        {
            if (!popup) return;

            IEnumerator CloseAndRestore()
            {
                popup.StartCoroutine("CloseAtEndOfFrame");
                yield return null;

                var modsView = TPSingleton<ModsView>.Instance;
                if (modsView != null)
                {
                    TheLastStand.View.Camera.CameraView.AttenuateWorldForPopupFocus(modsView);
                }
            }

            popup.StartCoroutine(CloseAndRestore());
        }

        private static void BuildButtonsRow(RectTransform buttonsArea, Action onApply, Action onReset, Action onClose)
        {
            var spacer = New("MCM Button Spacer", buttonsArea, typeof(RectTransform), typeof(LayoutElement))
                .GetComponent<RectTransform>();
            var spacerLayout = spacer.GetComponent<LayoutElement>();
            spacerLayout.minHeight = SpacerHeight;
            spacerLayout.preferredHeight = SpacerHeight;

            var row = EnsureHorizontalButtonRow(buttonsArea, "MCM Button Row");

            var resetButton = MCMButtonFactory.AddButtonSimple(
                row,
                Localization.LocalizeOrDefault("MCM_Reset", "Reset"),
                () => onReset?.Invoke(),
                interactable: true,
                normalColor: new Color(0.75f, 0.75f, 0.75f),
                highlightedColor: Color.white);

            var filler = New("MCM Button Filler", row, typeof(RectTransform), typeof(LayoutElement))
                .GetComponent<LayoutElement>();
            filler.flexibleWidth = 1f;

            var cancelButton = MCMButtonFactory.AddButtonSimple(
                row,
                Localization.LocalizeOrDefault("MCM_Cancel", "Cancel"),
                () => onClose?.Invoke(),
                interactable: true,
                normalColor:  new Color(0.85f, 0.40f, 0.40f),
                highlightedColor: new Color(0.925f, 0.70f, 0.70f));

            var applyButton = MCMButtonFactory.AddButtonSimple(
                row,
                Localization.LocalizeOrDefault("MCM_Save", "Save"),
                () =>
                {
                    onApply?.Invoke();
                    onClose?.Invoke();
                },
                interactable: true,
                normalColor:  new Color(0.40f, 0.75f, 0.40f),
                highlightedColor: new Color(0.70f, 0.875f, 0.70f));

            MakeUniformWidths(row, resetButton, cancelButton, applyButton);
        }

        private static void RebuildContent(RectTransform content, ModConfig mod, ModsView modsView, Dictionary<string, object> staged, RectTransform rootPanel)
        {
            staged.Clear();

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(content.GetChild(i).gameObject);
            }

            AddReloadWarningLabel(content, mod);
            BuildOptionsUI(content, mod, modsView, staged);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootPanel);
            Canvas.ForceUpdateCanvases();
        }

        private static void MakeUniformWidths(RectTransform row, params Button[] buttons)
        {
            if (buttons == null || buttons.Length == 0) return;

            var preferredWidths = new List<float>(buttons.Length);
            var maxWidth = 0f;
            var totalPreferred = 0f;

            foreach (var button in buttons)
            {
                if (!button) continue;

                var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
                var width = label ? Mathf.Ceil(label.preferredWidth) + MCMButtonFactory.ConfigBtnPadX : 0f;
                preferredWidths.Add(width);
                totalPreferred += width;
                if (width > maxWidth) maxWidth = width;
            }

            if (preferredWidths.Count == 0) return;

            var targetWidth = maxWidth;
            var containerWidth = 0f;

            containerWidth = row.rect.width;
            if (containerWidth <= 0f)
            {
                var parentRect = row.parent as RectTransform;
                if (parentRect) containerWidth = parentRect.rect.width;
            }
            
            var buttonCount = preferredWidths.Count;
            if (containerWidth > 0f && buttonCount > 0)
            {
                var spacingWidth = HorizontalLayoutSpacing * Mathf.Max(0, buttonCount - 1);
                var usableWidth = Mathf.Max(0f, containerWidth - spacingWidth);
                if (totalPreferred > usableWidth && usableWidth > 0f)
                {
                    targetWidth = Mathf.Min(maxWidth, usableWidth / buttonCount);
                }
            }

            if (targetWidth <= 0f)
            {
                targetWidth = maxWidth > 0f ? maxWidth : MCMButtonFactory.ConfigBtnPadX * 2f;
            }

            foreach (var button in buttons)
            {
                if (!button) continue;

                var rect = (RectTransform)button.transform;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

                var layout = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
                layout.minWidth = Mathf.Min(targetWidth, maxWidth);
                layout.preferredWidth = targetWidth;
                layout.flexibleWidth = 1f;
            }
        }

        private static void DestroyIfExists(Transform parent, string childName)
        {
            var existing = parent.Cast<Transform>().FirstOrDefault(child => child.name == childName);
            if (existing)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }
        }

        private static RectTransform CreateContentContainer(RectTransform panel, RectTransform buttonsArea)
        {
            var content = New(
                "MCM Content",
                panel,
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter),
                typeof(LayoutElement))
                .GetComponent<RectTransform>();

            if (buttonsArea)
            {
                content.SetSiblingIndex(buttonsArea.GetSiblingIndex());
            }
            else
            {
                content.SetAsLastSibling();
            }

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = VerticalLayoutSpacing;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.GetComponent<LayoutElement>().flexibleWidth = 1f;

            return content;
        }

        private static void BuildOptionsUI(RectTransform content, ModConfig mod, ModsView modsView, Dictionary<string, object> staged)
        {
            if (mod?.config == null) return;

            foreach (var entry in mod.config)
            {
                var key = entry.Key;
                var option = entry.Value;
                var kind = (option?.type ?? string.Empty).Trim().ToLowerInvariant();

                var label = Localization.LocalizeOrDefault(mod.modId, key, "displayName", key);
                var description = Localization.LocalizeOrDefault(mod.modId, key, "description", key);
                var tooltipText = string.IsNullOrWhiteSpace(description) ? null : description;

                switch (kind)
                {
                    case "toggle":
                    case "bool":
                    case "checkbox":
                    {
                        var defaultValue = option?.value is bool b && b;
                        var current = MCM.GetValue<bool>(mod.modId, key, defaultValue);
                        var toggle = MCMToggleFactory.AddToggle(content, label, current, value => staged[key] = value);
                        if (toggle)
                        {
                            var labelTMP = toggle.GetComponentInChildren<TextMeshProUGUI>(true);
                            MCMTooltipBinder.Apply(modsView, tooltipText, toggle, labelTMP ? labelTMP.gameObject : null);
                        }
                        break;
                    }

                    case "dropdown":
                    {
                        var options = GetOptionsList(option);
                        var defaultValue = option?.value?.ToString() ?? (options.Count > 0 ? options[0] : string.Empty);
                        var selected = MCM.GetValue<string>(mod.modId, key, defaultValue);
                        var selectedIndex = Mathf.Max(0, options.FindIndex(o => string.Equals(o, selected, StringComparison.OrdinalIgnoreCase)));

                        var dropdown = MCMDropdownFactory.AddDropdownSimple(content, label, options, selectedIndex, (index, text) => staged[key] = text);
                        if (dropdown)
                        {
                            var labelGO = dropdown.transform.parent?.Find("MCM Label")?.gameObject;
                            MCMTooltipBinder.Apply(modsView, tooltipText, dropdown, labelGO);
                        }
                        break;
                    }

                    default:
                        break;
                }
            }
        }

        private static List<string> GetOptionsList(ConfigOption option)
        {
            if (option == null) return new List<string>();

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

            var field = option.GetType().GetField("options", flags);
            if (field != null)
            {
                return CoerceToStrings(field.GetValue(option));
            }

            var property = option.GetType().GetProperty("options", flags);
            if (property != null)
            {
                return CoerceToStrings(property.GetValue(option));
            }

            return new List<string>();

            static List<string> CoerceToStrings(object value)
            {
                if (value == null) return new List<string>();
                if (value is IEnumerable<string> strings) return strings.ToList();
                if (value is IEnumerable<object> objects) return objects.Select(o => o?.ToString() ?? string.Empty).ToList();
                if (value is string single) return new List<string> { single };
                return new List<string>();
            }
        }

        private static void AddReloadWarningLabel(RectTransform parent, ModConfig mod)
        {
            if (mod == null || !mod.requireReload) return;

            var banner = new GameObject("MCM Reload Banner", typeof(RectTransform), typeof(LayoutElement), typeof(Image))
                .GetComponent<RectTransform>();
            banner.SetParent(parent, false);

            var background = banner.GetComponent<Image>();
            background.color = new Color(0.22f, 0.08f, 0.08f, 0.85f);

            banner.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40f);

            var outline = banner.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            outline.effectDistance = new Vector2(1f, -1f);

            var titleText = MCMLabelFactory.AddLabel(banner.transform, Localization.LocalizeOrDefault("MCM_RequireRestart", "Requires reload"));
            titleText.transform.SetParent(banner, false);

            var titleTMP = titleText.GetComponent<TextMeshProUGUI>();
            MCMLabelStyler.Apply(titleTMP, 18f, bold: true, warning: true);

            var rect = titleTMP.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(16f, 8f);
            rect.offsetMax = new Vector2(-16f, -8f);
        }

        private static RectTransform EnsureHorizontalButtonRow(RectTransform buttonsArea, string name)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement))
                .GetComponent<RectTransform>();
            row.SetParent(buttonsArea, false);

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = HorizontalLayoutSpacing;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.minHeight = ButtonRowMinHeight;
            rowLayout.preferredHeight = ButtonRowPrefHeight;

            return row;
        }

        private static GameObject New(string name, RectTransform parent, params Type[] components)
        {
            var go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }

        public interface IHasOptions
        {
            IEnumerable<string> options { get; }
        }
    }
}

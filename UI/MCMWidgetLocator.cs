using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TheLastStand.Framework.UI;
using TheLastStand.View.Settings;
using TheLastStand.View.Generic;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMWidgetLocator
    {
        public static MonoBehaviour SettingsPanel { get; private set; }

        public static GameObject ToggleTemplate { get; private set; }
        public static GameObject DropdownTemplate { get; private set; }
        public static GameObject PopupButtonTemplate { get; private set; }
        private static TMP_Text _labelRef;

        private const string ToggleCloneTarget = "EndTurnWarningToggle";
        private const string DropdownPanelCloneTarget = "LanguageDropdownPanel";

        public static GameObject GetOrResolveToggleTemplate()
        {
            if (ToggleTemplate) return ToggleTemplate;

            var root = GetSettingsRootOrWarn();
            if (!root)
            {
                return null;
            }

            var row = TryFindByTypeName(root, ToggleCloneTarget);
            if (!row)
            {
                Plugin.Log?.LogWarning("[MCM] Could not find a suitable row-style toggle template.");
                return null;
            }

            var toggle = row.GetComponentInChildren<BetterToggle>(true);
            ToggleTemplate = CacheClone(toggle.gameObject, "MCM Toggle Template");
            Plugin.Log?.LogInfo($"[MCM] Cached toggle row template: {MCMUIUtil.PathOf(ToggleTemplate.transform)}");
            return ToggleTemplate;
        }

        public static GameObject GetOrResolveDropdownTemplate()
        {
            if (DropdownTemplate) return DropdownTemplate;

            var root = GetSettingsRootOrWarn();
            if (!root)
            {
                return null;
            }

            var row = TryFindByTypeName(root, DropdownPanelCloneTarget);
            if (!row)
            {
                Plugin.Log?.LogWarning("[MCM] Could not find a suitable dropdown template.");
                return null;
            }

            var tpl = CacheClone(row, "MCM Dropdown Template", deactivate: false);

            var langPanel = tpl.GetComponentsInChildren<MonoBehaviour>(true)
                            .FirstOrDefault(mb => mb && mb.GetType().Name == DropdownPanelCloneTarget);
            if (langPanel) UnityEngine.Object.DestroyImmediate(langPanel, false);

            DropdownTemplate = tpl;
            Plugin.Log?.LogInfo($"[MCM] Cached dropdown row template: {MCMUIUtil.PathOf(DropdownTemplate.transform)}");
            return DropdownTemplate;
        }

        public static GameObject GetOrResolvePopupButtonTemplate()
        {
            if (PopupButtonTemplate) return PopupButtonTemplate;

            var popup = UnityEngine.Object.FindObjectsOfType<GenericPopUp>(true)
                                        .FirstOrDefault(g => g && g.Canvas);
            if (!popup)
            {
                Plugin.Log?.LogWarning("[MCM] Popup button template not found; ensure a GenericPopUp has been shown before using MCM UI.");
                return null;
            }

            var btns = popup.Canvas.GetComponentsInChildren<Button>(true);
            var btn = btns.FirstOrDefault(b =>
            {
                var t = b.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
                var s = t ? t.text?.Trim() : null;
                return !string.IsNullOrEmpty(s) &&
                    (string.Equals(s, "OK", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(s, "Close", StringComparison.OrdinalIgnoreCase));
            }) ?? btns.FirstOrDefault();

            if (!btn)
            {
                Plugin.Log?.LogWarning("[MCM] Could not locate a Button inside GenericPopUp.");
                return null;
            }

            PopupButtonTemplate = CacheClone(btn.gameObject, "MCM PopupButton Template");

            Plugin.Log?.LogInfo($"[MCM] Cached popup button template: {MCMUIUtil.PathOf(PopupButtonTemplate.transform)}");
            return PopupButtonTemplate;
        }

        public static TMP_Text GetOrResolveLabelReference()
        {
            if (_labelRef) return _labelRef;

            var tmpl = GetOrResolveToggleTemplate();
            _labelRef = tmpl.GetComponentsInChildren<TMP_Text>(true).First();
            Plugin.Log?.LogInfo("[MCM] Cached reference label from toggle template.");
            return _labelRef;
        }

        private static Transform GetSettingsRootOrWarn()
        {
            var settingsPanel = GetOrResolveSettingsPanel();
            if (!settingsPanel)
            {
                Plugin.Log?.LogWarning("[MCM] Settings panel not found; UI templates cannot be resolved.");
                return null;
            }

            return settingsPanel.transform;
        }

        private static GameObject CacheClone(GameObject original, string cloneName, bool deactivate = true)
        {
            var clone = UnityEngine.Object.Instantiate(original);
            clone.name = cloneName;
            if (deactivate)
            {
                clone.SetActive(false);
            }

            UnityEngine.Object.DontDestroyOnLoad(clone);
            return clone;
        }

        private static GameObject TryFindByTypeName(Transform settingsRoot, string typeName)
        {
            return settingsRoot
                .GetComponentsInChildren<MonoBehaviour>(true)
                .FirstOrDefault(mb => mb && mb.GetType().Name == typeName)
                ?.gameObject;
        }

        private static MonoBehaviour GetOrResolveSettingsPanel()
        {
            if (!SettingsPanel)
                SettingsPanel = UnityEngine.Object
                    .FindObjectsOfType<MonoBehaviour>(true)
                    .FirstOrDefault(mb => mb && mb.GetType().Name == "SettingsPanel");

            return SettingsPanel;
        }
    }
}

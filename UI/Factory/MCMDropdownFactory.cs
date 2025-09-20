using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TheLastStand.View.Settings;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMDropdownFactory
    {
        public static float RowHeight = 48f;
        public static float LabelFontSize = 19f;
        public static float HorizontalSpacing = 14f;

        public static GameObject AddDropdownSimple(Transform parent, string label, IList<string> options, int selectedIndex, Action<int, string> onChanged)
            => AddDropdown(parent, label, options, selectedIndex, onChanged);

        public static GameObject AddDropdown(
            Transform parent,
            string label,
            IList<string> options,
            int selectedIndex,
            Action<int, string> onChanged)
        {
            var template = ResolveTemplate();
            if (!template)
            {
                return null;
            }

            var row = MCMUIUtil.CreateLabeledRow(parent, "MCM Dropdown Row", label, RowHeight, HorizontalSpacing, out _);
            var clone = CreateDropdownInstance(template, row, label);
            var dropdown = clone.GetComponentInChildren<TMP_Dropdown>(true);

            ConfigureDropdown(dropdown, options, selectedIndex, onChanged);

            return clone;
        }

        private static GameObject ResolveTemplate()
        {
            var template = MCMWidgetLocator.GetOrResolveDropdownTemplate();
            Debug.Log($"AddDropdown: Template={(template ? "OK" : "NULL")}");
            if (!template)
            {
                Plugin.Log?.LogError("[MCM] Dropdown template missing; Settings UI is not available yet, so dropdown creation is skipped.");
            }

            return template;
        }

        private static GameObject CreateDropdownInstance(GameObject template, Transform parent, string label)
        {
            var clone = UnityEngine.Object.Instantiate(template, parent);
            clone.name = $"MCM Dropdown {label}";
            clone.SetActive(true);

            return clone;
        }

        private static void ConfigureDropdown(TMP_Dropdown dropdown, IList<string> options, int selectedIndex, Action<int, string> onChanged)
        {
            dropdown.ClearOptions();

            var resolvedOptions = (options ?? Array.Empty<string>())
                .Select(s => new TMP_Dropdown.OptionData(s))
                .ToList();

            dropdown.AddOptions(resolvedOptions);

            var clampedIndex = Mathf.Clamp(selectedIndex, 0, dropdown.options.Count - 1);
            dropdown.SetValueWithoutNotify(clampedIndex);

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(i =>
            {
                var text = (i >= 0 && i < dropdown.options.Count) ? dropdown.options[i].text : string.Empty;
                onChanged?.Invoke(i, text);
            });
        }
    }
}

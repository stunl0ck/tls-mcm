using System;
using TMPro;
using UnityEngine;
using TheLastStand.Framework.UI;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMToggleFactory
    {
        public static GameObject AddToggle(Transform parent, string label, bool value, Action<bool> onChanged)
        {
            var template = ResolveTemplate();
            if (!template)
            {
                return null;
            }

            var clone = InstantiateToggle(template, parent, label);

            ConfigureLabel(clone, label);
            ConfigureToggle(clone, value, onChanged);

            return clone;
        }

        private static GameObject ResolveTemplate()
        {
            var template = MCMWidgetLocator.GetOrResolveToggleTemplate();
            if (!template)
            {
                Plugin.Log?.LogError("[MCM] Toggle template missing; Settings UI is not available yet, so toggle creation is skipped.");
            }

            return template;
        }

        private static GameObject InstantiateToggle(GameObject template, Transform parent, string label)
        {
            var clone = UnityEngine.Object.Instantiate(template, parent);
            clone.name = $"MCM Toggle {label}";
            clone.SetActive(true);

            return clone;
        }

        private static void ConfigureLabel(GameObject clone, string label)
        {
            var labelTMP = clone.GetComponentInChildren<TextMeshProUGUI>(true);
            labelTMP.text = label;
            MCMLabelStyler.Apply(labelTMP);
        }

        private static void ConfigureToggle(GameObject clone, bool value, Action<bool> onChanged)
        {
            var betterToggle = clone.GetComponentInChildren<BetterToggle>(true);
            betterToggle.isOn = value;
            betterToggle.onValueChanged.AddListener(v => onChanged(v));
        }
    }
}

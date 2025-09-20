// MCMLabelFactory.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMLabelFactory
    {
        // Clone the game's settings-row label and parent it under 'parent'.
        public static TMP_Text AddLabel(Transform parent, string text, string name = "MCM Label")
        {
            var refLabel = MCMWidgetLocator.GetOrResolveLabelReference(); // strict
            var clone = Object.Instantiate(refLabel, parent, false);      // instantiate the component
            clone.gameObject.name = name;
            clone.text = text;

            var le = clone.GetComponent<LayoutElement>() ?? clone.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f; // let label take remaining width so control sits to the right

            return clone;
        }
    }
}

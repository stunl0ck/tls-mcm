using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMUIUtil
    {
        public static GameObject New(string name, Transform parent, params System.Type[] comps)
        {
            var go = new GameObject(name, comps);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform CreateLabeledRow(
            Transform parent,
            string rowName,
            string label,
            float rowHeight,
            float horizontalSpacing,
            out TMP_Text labelComponent)
        {
            var row = New(rowName, parent, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));

            var layoutGroup = row.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = horizontalSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandHeight = false;

            var layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = rowHeight;
            layoutElement.preferredHeight = rowHeight;

            labelComponent = MCMLabelFactory.AddLabel(row.transform, label);
            MCMLabelStyler.Apply(labelComponent);

            var rectTransform = row.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);

            return rectTransform;
        }

        public static void StripSettingsBehaviours(GameObject root)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!mb) continue;
                var ns = mb.GetType().Namespace ?? "";
                var name = mb.GetType().Name;
                if (ns.StartsWith("TheLastStand.View.Settings", System.StringComparison.Ordinal) ||
                    name.EndsWith("DropdownPanel", System.StringComparison.Ordinal))
                {
                    UnityEngine.Object.Destroy(mb);
                }
            }
        }

        public static void StripAllContentSizeFitters(GameObject root)
        {
            foreach (var csf in root.GetComponentsInChildren<ContentSizeFitter>(true))
                UnityEngine.Object.Destroy(csf);
        }

        public static string PathOf(Transform t)
        {
            var stack = new System.Collections.Generic.List<string>();
            var cur = t;
            while (cur)
            {
                stack.Add(cur.name);
                cur = cur.parent;
            }
            stack.Reverse();
            return "/" + string.Join("/", stack);
        }
    }
}
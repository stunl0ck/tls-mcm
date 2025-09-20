using TMPro;
using UnityEngine;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMLabelStyler
    {
        public static void Apply(TMP_Text target, float fontSize = 22f, bool bold = false, bool warning = false)
        {
            if (!target) return;

            target.fontSize = fontSize;
            target.alignment = TextAlignmentOptions.Left;
            target.margin = Vector4.zero;
            target.enableKerning = true;
            target.enableAutoSizing = false;

            if (bold)
            {
                target.fontStyle = FontStyles.Bold;
            }

            if (warning)
            {
                target.color = new Color(0.94f, 0.70f, 0.66f, 1f);
            }
        }
    }
}
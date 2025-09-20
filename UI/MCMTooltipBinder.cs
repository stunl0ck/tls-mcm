using TheLastStand.View.Generic;
using TheLastStand.View.Modding;
using UnityEngine;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMTooltipBinder
    {
        public static void Apply(ModsView modsView, string tooltipText, params GameObject[] hosts)
        {
            if (string.IsNullOrWhiteSpace(tooltipText) || hosts == null) return;

            var targetTooltip = modsView?.RawTextTooltip;
            if (!targetTooltip) return;

            foreach (var host in hosts)
            {
                if (!host) continue;

                var displayer = host.GetComponent<RawTextTooltipDisplayer>()
                                ?? host.AddComponent<RawTextTooltipDisplayer>();

                displayer.Text = tooltipText;
                displayer.TargetTooltip = targetTooltip;
            }
        }
    }
}

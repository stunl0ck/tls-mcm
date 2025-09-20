using HarmonyLib;
using TheLastStand.Controller.Modding.Module;
using TPLib.Localization;
using BepInEx;
using Stunl0ck.TLS.Shared;

namespace Stunl0ck.ModConfigManager.Hooks
{
    internal static class LocalizationHooks
    {
        private static bool s_injected;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LocalizationModuleController), "LoadLanguages")]
        private static void InjectTranslations()
        {
            if (s_injected) return;
            s_injected = true;

            var log = new Localization.Logger(
                info: s => Plugin.Log?.LogInfo(s),
                warn: s => Plugin.Log?.LogWarning(s),
                error: s => Plugin.Log?.LogError(s));

            Localization.MergeCsvsUnder(Paths.PluginPath, "MCM/languages.csv", log);
        }
    }
}
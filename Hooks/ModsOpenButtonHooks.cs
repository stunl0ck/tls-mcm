using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using Stunl0ck.ModConfigManager.DTO;
using TheLastStand.View.Modding;

namespace Stunl0ck.ModConfigManager.Hooks
{
    internal static class ModsOpenButtonHooks
    {
        private static bool? s_cachedShouldEnable;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ModsView), "ShouldEnableOpenButton")]
        private static void ShouldEnableOpenButtonPostfix(ref bool __result)
        {
            if (__result) return;
            __result = ShouldEnableForMcm();
        }

        private static bool ShouldEnableForMcm()
        {
            if (s_cachedShouldEnable.HasValue)
                return s_cachedShouldEnable.Value;

            try
            {
                if (MCM.Registered != null && MCM.Registered.Count > 0)
                {
                    s_cachedShouldEnable = true;
                    return true;
                }

                var pluginsRoot = Paths.PluginPath;
                if (string.IsNullOrWhiteSpace(pluginsRoot) || !Directory.Exists(pluginsRoot))
                    return false;

                foreach (var dir in Directory.EnumerateDirectories(pluginsRoot))
                {
                    var configPath = Path.Combine(dir, "config.json");
                    if (!File.Exists(configPath))
                        continue;

                    ModConfig mod;
                    try
                    {
                        mod = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(configPath));
                    }
                    catch
                    {
                        continue;
                    }

                    if (mod != null && !string.IsNullOrWhiteSpace(mod.modId))
                    {
                        s_cachedShouldEnable = true;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[MCM] Failed to evaluate whether Mods button should be enabled: {ex.Message}");
            }

            return false;
        }
    }
}

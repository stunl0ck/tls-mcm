using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using TheLastStand.View.Modding;
using Stunl0ck.ModConfigManager.UI;
using Stunl0ck.ModConfigManager.DTO;
using Newtonsoft.Json;
using BepInEx;

namespace Stunl0ck.ModConfigManager.Hooks
{
    internal static class ModsViewHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ModsView), "PopulateWindow")]
        private static void InjectRegisteredMods(ModsView __instance, Transform ___modItemParent, ModItemView ___modItemViewPrefab)
        {
            try
            {
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var registered = MCM.Registered;
                if (registered != null)
                {
                    foreach (var kv in registered)
                    {
                        var mod = kv.Value;
                        if (mod?.modId == null) continue;
                        seen.Add(mod.modId);
                        MCMModItemEntry.Spawn(__instance, ___modItemParent, ___modItemViewPrefab, mod);
                        Plugin.Log?.LogInfo($"Spawned MCM entry for registered mod: {mod.modName}");
                    }
                }

                // Search for non-MCM mods (TLS.ModKit data-only mods) desciptive only/no config
                var pluginsRoot = Paths.PluginPath;
                if (!string.IsNullOrWhiteSpace(pluginsRoot) && Directory.Exists(pluginsRoot))
                {
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
                        catch (Exception ex)
                        {
                            Plugin.Log?.LogWarning($"[MCM] Skipping config.json in '{dir}' (failed to parse): {ex.Message}");
                            continue;
                        }

                        if (mod == null || string.IsNullOrWhiteSpace(mod.modId))
                            continue;

                        if (seen.Contains(mod.modId))
                            continue;

                        seen.Add(mod.modId);
                        MCMModItemEntry.Spawn(__instance, ___modItemParent, ___modItemViewPrefab, mod);
                        Plugin.Log?.LogInfo($"Spawned MCM entry for data-only mod: {mod.modName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"Failed to inject MCM entries: {ex}");
            }
        }
    }
}

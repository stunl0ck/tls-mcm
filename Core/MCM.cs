using System.Collections.Generic;
using BepInEx;
using Stunl0ck.ModConfigManager.Core;
using Stunl0ck.ModConfigManager.DTO;

namespace Stunl0ck.ModConfigManager
{
    public static class MCM
    {
        public static IModConfigService Service { get; internal set; } = new ModConfigService();

        public static IReadOnlyDictionary<string, ModConfig> Registered => Service.Registered;

        public static void Register(BaseUnityPlugin plugin) => Service.Register(plugin);

        public static T GetValue<T>(string modId, string key, T fallback = default) =>
            Service.GetValue(modId, key, fallback);

        public static void SetValue<T>(string modId, string key, T value) =>
            Service.SetValue(modId, key, value);

        public static bool IsEnabled(string modId) => Service.IsEnabled(modId);

        public static void SetEnabled(string modId, bool enabled) => Service.SetEnabled(modId, enabled);

        public static bool ResetToDefaults(string modId) => Service.ResetToDefaults(modId);
    }
}
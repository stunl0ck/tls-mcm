using System.Collections.Generic;
using BepInEx;
using Stunl0ck.ModConfigManager.DTO;

namespace Stunl0ck.ModConfigManager.Core
{
    public interface IModConfigService
    {
        IReadOnlyDictionary<string, ModConfig> Registered { get; }

        void Register(BaseUnityPlugin plugin);

        T GetValue<T>(string modId, string key, T fallback = default);

        void SetValue<T>(string modId, string key, T value);

        bool IsEnabled(string modId);

        void SetEnabled(string modId, bool enabled);

        bool ResetToDefaults(string modId);
    }
}
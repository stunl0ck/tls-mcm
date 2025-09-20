using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Stunl0ck.ModConfigManager.Hooks;

namespace Stunl0ck.ModConfigManager
{
    [BepInPlugin(ModId, ModName, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModId = "com.modconfigmanager.core";
        public const string ModName = "Mod Configuration Manager";
        public const string Version = "1.0.0";

        public static ManualLogSource Log { get; private set; }
        private static bool s_initialized;

        private void Awake()
        {
            Log = Logger;

            if (s_initialized)
            {
                Log.LogInfo("[MCM] Already initialized, skipping.");
                return;
            }
            s_initialized = true;

            Harmony.CreateAndPatchAll(typeof(LocalizationHooks));
            Harmony.CreateAndPatchAll(typeof(ModsViewHooks));

            Log.LogInfo("[MCM] Plugin initialized.");
        }
    }
}
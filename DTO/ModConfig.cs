using System;
using System.Collections.Generic;

namespace Stunl0ck.ModConfigManager.DTO
{
    [Serializable]
    public class ModConfig
    {
        public string modId;
        public string modName;
        public string version;
        public string author;
        public string description;
        public Dictionary<string, ConfigOption> config;
        public bool requireReload = false;
    }
}

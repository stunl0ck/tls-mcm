using System;
using System.Collections.Generic;

namespace Stunl0ck.ModConfigManager.DTO
{
    [Serializable]
    public class ConfigOption
    {
        public string type;          // e.g. "bool", "int", "float", "string"
        public object value;
        public string displayName;
        public string description;
        public List<string> options; // used by "dropdown"
    }
}

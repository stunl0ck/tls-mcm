using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using Stunl0ck.ModConfigManager.DTO;

namespace Stunl0ck.ModConfigManager.Core
{
    public class ModConfigService : IModConfigService
    {
        private readonly Dictionary<string, ModConfig> _registered =
            new Dictionary<string, ModConfig>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _configJsonPathById =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, bool> _masterEnabledById =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private string OverridesDir => Path.Combine(Paths.ConfigPath, "ModConfigManager");

        public IReadOnlyDictionary<string, ModConfig> Registered => _registered;

        public void Register(BaseUnityPlugin plugin)
        {
            if (plugin?.Info?.Location == null)
                throw new Exception("MCM.Register: plugin.Info.Location is null");

            var modFolder = Path.GetDirectoryName(plugin.Info.Location);
            var configPath = Path.Combine(modFolder ?? string.Empty, "config.json");
            if (!File.Exists(configPath))
                throw new Exception($"config.json not found in {modFolder}");

            var defaults = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(configPath));
            if (defaults == null || string.IsNullOrWhiteSpace(defaults.modId))
                throw new Exception($"Invalid config.json in {modFolder}");

            var modId = defaults.modId;
            _configJsonPathById[modId] = configPath;

            Directory.CreateDirectory(OverridesDir);
            var overridesPath = Path.Combine(OverridesDir, $"{modId}.json");

            var overrides = ReadOverridesFile(overridesPath)
                            ?? BuildOverridesFromDefaults(defaults, enabledDefault: true);

            if (defaults.config != null)
            {
                foreach (var kv in defaults.config)
                {
                    if (!overrides.config.ContainsKey(kv.Key))
                        overrides.config[kv.Key] = kv.Value?.value;
                }
            }

            if (defaults.config != null)
            {
                foreach (var kv in defaults.config)
                {
                    var key = kv.Key;
                    var opt = kv.Value;
                    if (opt == null) continue;

                    if (overrides.config.TryGetValue(key, out var ov))
                        opt.value = CoerceForOption(opt.type, ov, opt.value);
                }
            }

            _masterEnabledById[modId] = overrides.enabled ?? true;

            WriteOverridesFileAtomic(overridesPath, overrides);

            _registered[modId] = defaults;
        }

        public T GetValue<T>(string modId, string key, T fallback = default)
        {
            if (string.IsNullOrEmpty(modId) || string.IsNullOrEmpty(key))
                return fallback;

            if (!_registered.TryGetValue(modId, out var cfg) || cfg?.config == null)
                return fallback;

            if (!cfg.config.TryGetValue(key, out var opt) || opt == null)
                return fallback;

            try
            {
                var v = opt.value;
                if (v is T tv) return tv;
                if (v == null) return fallback;

                var t = typeof(T);
                if (t == typeof(bool))
                {
                    var b = ToBool(v);
                    return (T)(object)b;
                }

                if (t == typeof(string))
                {
                    return (T)(object)v.ToString();
                }

                return (T)Convert.ChangeType(v, t, CultureInfo.InvariantCulture);
            }
            catch
            {
                return fallback;
            }
        }

        public void SetValue<T>(string modId, string key, T value)
        {
            if (string.IsNullOrEmpty(modId) || string.IsNullOrEmpty(key))
                return;

            if (!_registered.TryGetValue(modId, out var cfg) || cfg?.config == null)
                return;

            if (!cfg.config.ContainsKey(key))
                return;

            cfg.config[key].value = value;

            Directory.CreateDirectory(OverridesDir);
            var overridesPath = Path.Combine(OverridesDir, $"{modId}.json");
            var overrides = ReadOverridesFile(overridesPath)
                            ?? BuildOverridesFromDefaults(cfg, IsEnabled(modId));

            overrides.config[key] = value;
            WriteOverridesFileAtomic(overridesPath, overrides);
        }

        public bool IsEnabled(string modId)
        {
            if (modId == null) return true;
            return _masterEnabledById.TryGetValue(modId, out var enabled) ? enabled : true;
        }

        public void SetEnabled(string modId, bool enabled)
        {
            if (string.IsNullOrEmpty(modId))
                return;

            _masterEnabledById[modId] = enabled;

            Directory.CreateDirectory(OverridesDir);
            var overridesPath = Path.Combine(OverridesDir, $"{modId}.json");
            var basis = _registered.TryGetValue(modId, out var cfg) ? cfg : null;
            var overrides = ReadOverridesFile(overridesPath)
                            ?? BuildOverridesFromDefaults(basis, enabled);

            overrides.enabled = enabled;

            if (overrides.config == null)
                overrides.config = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (basis?.config != null)
            {
                foreach (var kv in basis.config)
                {
                    if (!overrides.config.ContainsKey(kv.Key))
                        overrides.config[kv.Key] = kv.Value?.value;
                }
            }

            WriteOverridesFileAtomic(overridesPath, overrides);
        }

        public bool ResetToDefaults(string modId)
        {
            try
            {
                if (!_registered.TryGetValue(modId, out var cfg) || cfg?.config == null)
                    return false;

                if (_configJsonPathById.TryGetValue(modId, out var jsonPath) && File.Exists(jsonPath))
                {
                    var freshDefaults = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(jsonPath));
                    if (freshDefaults?.config != null)
                    {
                        foreach (var kv in freshDefaults.config)
                        {
                            if (cfg.config.ContainsKey(kv.Key))
                                cfg.config[kv.Key].value = kv.Value?.value;
                        }
                    }
                }

                _masterEnabledById[modId] = true;

                Directory.CreateDirectory(OverridesDir);
                var overridesPath = Path.Combine(OverridesDir, $"{modId}.json");
                var normalized = BuildOverridesFromDefaults(cfg, enabledDefault: true);
                WriteOverridesFileAtomic(overridesPath, normalized);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"MCM.ResetToDefaults error for '{modId}': {ex}");
                return false;
            }
        }

        private OverridesFile BuildOverridesFromDefaults(ModConfig defaults, bool enabledDefault)
        {
            var of = new OverridesFile
            {
                enabled = enabledDefault,
                config = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            };

            if (defaults?.config != null)
            {
                foreach (var kv in defaults.config)
                    of.config[kv.Key] = kv.Value?.value;
            }

            return of;
        }

        private void WriteOverridesFileAtomic(string path, OverridesFile data)
        {
            var tmp = path + ".tmp";
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(tmp, json);
            try
            {
                if (File.Exists(path))
                    File.Replace(tmp, path, null);
                else
                    File.Move(tmp, path);
            }
            catch
            {
                File.Copy(tmp, path, overwrite: true);
                File.Delete(tmp);
            }
        }

        private OverridesFile ReadOverridesFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var text = File.ReadAllText(path);
                var obj = JsonConvert.DeserializeObject<OverridesFile>(text);
                if (obj != null && obj.config == null)
                    obj.config = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        private object CoerceForOption(string optionKind, object incoming, object fallback)
        {
            var kind = (optionKind ?? string.Empty).Trim().ToLowerInvariant();

            if (kind == "checkbox" || kind == "toggle")
                return ToBoolOrFallback(incoming, fallback);

            if (kind == "dropdown")
                return incoming?.ToString() ?? (fallback?.ToString() ?? string.Empty);

            return incoming ?? fallback;
        }

        private bool ToBool(object value)
        {
            if (value is bool b) return b;
            if (value is string s)
            {
                s = s.Trim();
                if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)) return false;
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i != 0;
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return Math.Abs(d) > double.Epsilon;
            }
            if (value is int ii) return ii != 0;
            if (value is long ll) return ll != 0;
            if (value is double dd) return Math.Abs(dd) > double.Epsilon;
            if (value is float ff) return Math.Abs(ff) > float.Epsilon;
            return false;
        }

        private object ToBoolOrFallback(object incoming, object fallback)
        {
            try { return ToBool(incoming); }
            catch { return fallback is bool b ? b : false; }
        }

        private class OverridesFile
        {
            public bool? enabled { get; set; } = true;
            public Dictionary<string, object> config { get; set; } =
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
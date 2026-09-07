using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace ModTranslator
{
    public class TranslationManager
    {
        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>();
        private readonly HashSet<string> _injectedPrefixes = new HashSet<string>();

        public int TranslationCount => _translations.Count;

        public void LoadTranslations(string translationsRoot)
        {
            string zhDir = Path.Combine(translationsRoot, "zh");
            if (!Directory.Exists(zhDir))
            {
                Debug.Log($"[{ModMain.MOD_ID}] 翻译目录不存在: {zhDir}");
                return;
            }

            foreach (string file in Directory.GetFiles(zhDir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    LoadTranslationFile(file);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[{ModMain.MOD_ID}] 加载翻译文件失败 [{Path.GetFileName(file)}]: {e.Message}");
                }
            }
        }

        private void LoadTranslationFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            TranslationData data = JsonConvert.DeserializeObject<TranslationData>(json);
            if (data?.translations == null) return;

            foreach (var kvp in data.translations)
            {
                _translations[kvp.Key] = kvp.Value;
            }

            Debug.Log($"[{ModMain.MOD_ID}] 已加载 {data.translations.Count} 条翻译 <- {Path.GetFileName(filePath)}");
        }

        public void ApplyTranslations()
        {
            if (_translations.Count == 0) return;

            foreach (var kvp in _translations)
            {
                Strings.Add(kvp.Key, kvp.Value);
            }
            Debug.Log($"[{ModMain.MOD_ID}] 已通过 Strings.Add 注册 {_translations.Count} 条翻译。");
        }

        public void ReapplyTranslations()
        {
            if (_translations.Count == 0) return;
            Localization.OverloadStrings(new Dictionary<string, string>(_translations));
        }

        public void InjectByPrefix(string prefix)
        {
            if (!_injectedPrefixes.Add(prefix)) return;
            int count = 0;
            foreach (var kvp in _translations)
            {
                if (!kvp.Key.StartsWith(prefix)) continue;
                Strings.Add(kvp.Key, kvp.Value);
                count++;
            }
            if (count > 0)
                Debug.Log($"[{ModMain.MOD_ID}] 已注入 {count} 条 '{prefix}' 翻译。");
        }

        public void GenerateTemplateForType(Type type, string outputDir)
        {
            string ns = type.Namespace ?? "";
            if (string.IsNullOrEmpty(ns)) return;

            string filePath = Path.Combine(outputDir, ns + ".json");
            if (File.Exists(filePath)) return;

            var translations = new Dictionary<string, string>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType != typeof(LocString)) continue;
                object val = field.GetValue(null);
                if (val == null) continue;
                string text = ((LocString)val).text;
                if (string.IsNullOrEmpty(text)) continue;
                string key = field.Name;
                translations[key] = text;
            }

            if (translations.Count == 0) return;

            var template = new TranslationData
            {
                modId = ns,
                modName = ns,
                translations = translations
            };
            File.WriteAllText(filePath, JsonConvert.SerializeObject(template, Formatting.Indented));
            Debug.Log($"[{ModMain.MOD_ID}] 已生成模板: {ns} ({translations.Count} 条)");
        }

        public void GenerateTemplatesForMod(KMod.Mod mod, string outputDir)
        {
            foreach (Assembly assem in mod.loaded_mod_data.dlls)
            {
                var namespaces = new HashSet<string>();
                foreach (Type t in assem.GetTypes())
                {
                    string ns = t.Namespace ?? "";
                    if (string.IsNullOrEmpty(ns) || namespaces.Contains(ns)) continue;
                    namespaces.Add(ns);
                    try { GenerateTemplateForType(t, outputDir); }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[{ModMain.MOD_ID}] 生成模板失败 [{ns}]: {e.Message}");
                    }
                }
            }
            Debug.Log($"[{ModMain.MOD_ID}] 已为模组 '{mod.title}' 生成翻译模板。");
        }

        public string GetTranslation(string key)
        {
            return _translations.TryGetValue(key, out string value) ? value : null;
        }

        /// <summary>检测字符串是否包含中文字符（CJK 统一汉字）</summary>
        public static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
                if (c >= '\u4e00' && c <= '\u9fff') return true;
            return false;
        }
    }

    public class TranslationData
    {
        [JsonProperty("modId")]
        public string modId { get; set; }

        [JsonProperty("modName")]
        public string modName { get; set; }

        [JsonProperty("translations")]
        public Dictionary<string, string> translations { get; set; }
    }
}

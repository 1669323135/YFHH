using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;

namespace ModTranslator
{
    public class ModMain : UserMod2
    {
        public const string MOD_ID = "ModTranslator";
        public const string MOD_VERSION = "1.0.0-U59-744825";

        internal static string ModDirectory { get; private set; }
        internal static TranslationManager TranslationMgr { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Debug.Log($"[{MOD_ID}] v{MOD_VERSION} 正在加载...");

            try
            {
                TranslationMgr = new TranslationManager();
                TranslationMgr.LoadTranslations(Path.Combine(ModDirectory, "translations"));
                TranslationMgr.ApplyTranslations();

                harmony.PatchAll();

                Debug.Log($"[{MOD_ID}] 加载完成，已加载 {TranslationMgr.TranslationCount} 条翻译。");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{MOD_ID}] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<KMod.Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);
            if (TranslationMgr == null) return;

            string templatesDir = Path.Combine(ModDirectory, "templates");
            Directory.CreateDirectory(templatesDir);

            foreach (KMod.Mod mod in mods)
            {
                if (mod.title == MOD_ID || mod.staticID == MOD_ID) continue;
                if (!mod.IsActive()) continue;
                if (mod.status != KMod.Mod.Status.Installed) continue;

                try { TranslationMgr.GenerateTemplatesForMod(mod, templatesDir); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[{MOD_ID}] 模组 '{mod.title}' 模板生成失败: {e.Message}");
                }
            }
        }
    }
}

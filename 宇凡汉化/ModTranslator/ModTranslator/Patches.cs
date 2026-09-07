using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace ModTranslator
{
    // 拦截 Strings.Get(string) 调用，将未翻译的文本替换为中文
    [HarmonyPatch(typeof(Strings), nameof(Strings.Get), new[] { typeof(string) })]
    public static class Strings_Get_Patch
    {
        public static void Postfix(string key, ref StringEntry __result)
        {
            if (__result == null || string.IsNullOrEmpty(__result.String))
            {
                string translated = ModMain.TranslationMgr?.GetTranslation(key);
                if (translated != null)
                {
                    __result = new StringEntry(translated);
                }
            }
        }
    }

    // 拦截 OverloadStrings，在其他模组注册字符串时注入翻译
    [HarmonyPatch(typeof(Localization), nameof(Localization.OverloadStrings), new[] { typeof(Dictionary<string, string>) })]
    public static class Localization_OverloadStrings_Patch
    {
        public static void Prefix(Dictionary<string, string> translated_strings)
        {
            if (ModMain.TranslationMgr == null) return;

            var keys = new List<string>(translated_strings.Keys);
            foreach (string key in keys)
            {
                // 模组自带汉化：原值已含中文则保留
                if (translated_strings.TryGetValue(key, out string existing)
                    && TranslationManager.ContainsChinese(existing)) continue;

                string translated = ModMain.TranslationMgr.GetTranslation(key);
                if (translated != null)
                {
                    translated_strings[key] = translated;
                }
            }
        }
    }

    // 拦截 RegisterForTranslation，在其他模组注册字符串后补注翻译覆盖
    [HarmonyPatch(typeof(Localization), nameof(Localization.RegisterForTranslation))]
    public static class Localization_RegisterForTranslation_Patch
    {
        public static void Postfix()
        {
            if (ModMain.TranslationMgr == null) return;
            ModMain.TranslationMgr.ReapplyTranslations();
        }
    }

    // 建筑注册时注入 STRINGS.BUILDINGS.* 翻译
    [HarmonyPatch(typeof(ModUtil), nameof(ModUtil.AddBuildingToPlanScreen),
        new[] { typeof(HashedString), typeof(string), typeof(string), typeof(string), typeof(ModUtil.BuildingOrdering) })]
    public static class AddBuildingToPlanScreen_Patch
    {
        public static void Prefix() => ModMain.TranslationMgr?.InjectByPrefix("STRINGS.BUILDINGS.");
    }

    // 建筑加载时注入 STRINGS.BUILDINGS.* 翻译
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class LoadGeneratedBuildings_Patch
    {
        public static void Prefix() => ModMain.TranslationMgr?.InjectByPrefix("STRINGS.BUILDINGS.");
    }

    // 元素加载时注入 STRINGS.ELEMENTS.* 翻译
    [HarmonyPatch(typeof(Assets), nameof(Assets.SubstanceListHookup))]
    public static class SubstanceListHookup_Patch
    {
        public static void Prefix() => ModMain.TranslationMgr?.InjectByPrefix("STRINGS.ELEMENTS.");
    }

    // 直接替换物品 name/desc
    [HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.CreateLooseEntity))]
    public static class CreateLooseEntity_Patch
    {
        public static void Prefix(string id, ref string name, ref string desc)
        {
            var mgr = ModMain.TranslationMgr;
            if (mgr == null) return;
            string prefix = $"STRINGS.ITEMS.FOOD.{id.ToUpperInvariant()}.";
            string tName = mgr.GetTranslation(prefix + "NAME");
            string tDesc = mgr.GetTranslation(prefix + "DESC");
            if (tName != null && !TranslationManager.ContainsChinese(name)) name = tName;
            if (tDesc != null && !TranslationManager.ContainsChinese(desc)) desc = tDesc;
        }
    }

    // 直接替换生物 name/desc
    [HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.CreatePlacedEntity))]
    public static class CreatePlacedEntity_Patch
    {
        public static void Prefix(string id, ref string name, ref string desc)
        {
            var mgr = ModMain.TranslationMgr;
            if (mgr == null) return;
            string prefix = $"STRINGS.CREATURES.SPECIES.{id.ToUpperInvariant()}.";
            string tName = mgr.GetTranslation(prefix + "NAME");
            string tDesc = mgr.GetTranslation(prefix + "DESC");
            if (tName != null && !TranslationManager.ContainsChinese(name)) name = tName;
            if (tDesc != null && !TranslationManager.ContainsChinese(desc)) desc = tDesc;
        }
    }

    // 直接替换种子 name/desc
    [HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.CreateAndRegisterSeedForPlant))]
    public static class CreateAndRegisterSeedForPlant_Patch
    {
        public static void Prefix(string id, ref string name, ref string desc)
        {
            var mgr = ModMain.TranslationMgr;
            if (mgr == null) return;
            string prefix = $"STRINGS.CREATURES.SPECIES.SEEDS.{id.ToUpperInvariant()}.";
            string tName = mgr.GetTranslation(prefix + "NAME");
            string tDesc = mgr.GetTranslation(prefix + "DESC");
            if (tName != null && !TranslationManager.ContainsChinese(name)) name = tName;
            if (tDesc != null && !TranslationManager.ContainsChinese(desc)) desc = tDesc;
        }
    }
}

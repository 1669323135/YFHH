# 更新日志

## 1.2.0 - 2026-09-07

### 新增

- 自动发现已安装模组并生成 JSON 翻译模板到 `templates/` 目录（`OnAllModsLoaded` 钩子）
- 新增 `InjectByPrefix(prefix)` 方法，按前缀分批注入翻译，避免重复注入
- 新增 `GenerateTemplateForType` / `GenerateTemplatesForMod` 方法，扫描模组类型的 LocString 字段生成模板
- 新增 6 个 Harmony 补丁：
    - `ModUtil.AddBuildingToPlanScreen` → 注入 `STRINGS.BUILDINGS.*`
    - `GeneratedBuildings.LoadGeneratedBuildings` → 注入 `STRINGS.BUILDINGS.*`
    - `Assets.SubstanceListHookup` → 注入 `STRINGS.ELEMENTS.*`
    - `EntityTemplates.CreateLooseEntity` → 替换物品 name/desc
    - `EntityTemplates.CreatePlacedEntity` → 替换生物 name/desc
    - `EntityTemplates.CreateAndRegisterSeedForPlant` → 替换种子 name/desc

### 补丁清单（当前共 9 个）

| 补丁目标                                            | 作用                     |
|-------------------------------------------------|------------------------|
| `Strings.Get(string)`                           | 拦截游戏获取字符串的出口，兜底替换未翻译文本 |
| `Localization.OverloadStrings`                  | 在其他模组注册字符串时注入翻译        |
| `Localization.RegisterForTranslation`           | 在其他模组注册翻译类型后补注翻译覆盖     |
| `ModUtil.AddBuildingToPlanScreen`               | 建筑注册时按前缀注入翻译           |
| `GeneratedBuildings.LoadGeneratedBuildings`     | 建筑加载时按前缀注入翻译           |
| `Assets.SubstanceListHookup`                    | 元素加载时按前缀注入翻译           |
| `EntityTemplates.CreateLooseEntity`             | 直接替换物品 name/desc       |
| `EntityTemplates.CreatePlacedEntity`            | 直接替换生物 name/desc       |
| `EntityTemplates.CreateAndRegisterSeedForPlant` | 直接替换种子 name/desc       |

---

## 1.1.0 - 2026-09-07

### 新增

- 新增 `Localization.RegisterForTranslation` Harmony 补丁，在其他模组注册字符串后自动补注翻译覆盖，解决延迟注册问题
- 新增 `TranslationManager.ReapplyTranslations()` 方法，通过 `OverloadStrings` 重新注入翻译

### 变更

- `ApplyTranslations()` 改用 `Strings.Add(key, value)` 逐条注册翻译，替代原先的 `OverloadStrings` 调用
- 精简代码注释，移除冗余的 API 签名说明

### 补丁清单（当前共 3 个）

| 补丁目标                                  | 作用                     |
|---------------------------------------|------------------------|
| `Strings.Get(string)`                 | 拦截游戏获取字符串的出口，兜底替换未翻译文本 |
| `Localization.OverloadStrings`        | 在其他模组注册字符串时注入翻译        |
| `Localization.RegisterForTranslation` | 在其他模组注册翻译类型后补注翻译覆盖     |

---

## 1.0.0-U59-744825 - 初始版本

- 基于本地 JSON 翻译文件的模组汉化系统
- 支持按模组分文件组织翻译（`translations/zh/{ModId}.json`）
- 通过 `Strings.Get` 和 `OverloadStrings` 双机制注入翻译
- 适配游戏版本 U59-744825

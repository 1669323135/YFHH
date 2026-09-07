# 宇凡汉化

**缺氧 (Oxygen Not Included) Steam 创意工坊模组翻译工具**

将 Steam 创意工坊中任意模组的非中文文本翻译为简体中文。

---

## 功能特性

- **通用翻译框架** — 支持翻译任意 Steam 创意工坊模组的文本内容
- **双机制注入** — 同时使用 `OverloadStrings` 注册覆盖 + Harmony 补丁拦截 `Strings.Get` 调用，确保翻译全面生效
- **JSON 翻译文件** — 按模组分组的 JSON 格式，易于编辑和维护
- **热加载目录** — 自动扫描 `translations/zh/` 下所有 `.json` 文件，支持子目录分类管理
- **自动路径检测** — 构建时自动通过注册表检测 Steam 安装路径，无需手动配置

## 工作原理

```
游戏启动
  └─ ModMain.OnLoad()
       ├─ 加载 translations/zh/*.json 翻译文件
       ├─ 调用 Localization.OverloadStrings() 注册翻译覆盖
       └─ 注入 Harmony 补丁
            ├─ Strings.Get(string) Postfix → 拦截字符串查询，返回中文
            └─ Localization.OverloadStrings Prefix → 拦截其他模组的字符串注册，替换为中文
```

## 安装方法

### 从 Steam 创意工坊安装

1. 在 Steam 创意工坊中订阅本模组
2. 启动缺氧，进入 **模组** 界面
3. 启用「宇凡汉化」
4. 游戏会自动重新加载模组

### 本地安装

1. 将整个模组文件夹复制到：
   ```
   C:\Users\<用户名>\Documents\Klei\OxygenNotIncluded\mods\local\
   ```
2. 确保文件夹内包含以下文件：
    - `mod.yaml`
    - `mod_info.yaml`
    - `ModTranslator.dll`
    - `translations/` 文件夹

## 添加翻译

### 翻译文件格式

在 `translations/zh/` 目录下创建 JSON 文件，每个文件对应一个模组的翻译：

```json
{
  "modId": "目标模组的staticID",
  "modName": "目标模组的显示名称",
  "translations": {
    "目标模组.STRINGS.建筑.名称": "中文翻译",
    "目标模组.STRINGS.建筑.描述": "中文翻译",
    "目标模组.STRINGS.建筑.效果": "中文翻译"
  }
}
```

### 如何获取模组的字符串键

1. 启用目标模组并运行游戏
2. 在游戏日志或模组目录中查找字符串模板（`.pot` 文件）
3. 字符串键的格式通常为：`模组ID.STRINGS.分类.子分类.条目名`
4. 将英文原文对应的键填入 JSON 的 `translations` 字段，值填写中文翻译

### 翻译文件示例

参考 `translations/zh/_example.json`：

```json
{
  "modId": "ExampleMod",
  "modName": "示例模组",
  "translations": {
    "ExampleMod.STRINGS.BUILDINGS.EXAMPLE.NAME": "示例建筑",
    "ExampleMod.STRINGS.BUILDINGS.EXAMPLE.DESC": "这是一个示例建筑的描述。",
    "ExampleMod.STRINGS.BUILDINGS.EXAMPLE.EFFECT": "这是一个<link=\"EFFECT\">效果</link>描述。"
  }
}
```

> **提示**：翻译文本中可以使用 `<link="TAG">内容</link>` 等富文本标签，与游戏原生格式保持一致。

## 项目结构

```
宇凡汉化/
├── README.md                           ← 本文件
├── ModTranslator.sln                   ← Visual Studio 解决方案
└── ModTranslator/                      ← 模组根目录（发布内容）
    ├── mod.yaml                        ← 模组基本信息
    ├── mod_info.yaml                   ← 模组技术配置 (APIVersion: 2)
    ├── preview.png                     ← 创意工坊预览图
    ├── translations/                   ← 翻译数据目录
    │   └── zh/                         ← 简体中文翻译
    │       └── _example.json           ← 翻译文件示例
    └── ModTranslator/                  ← C# 源码
        ├── ModTranslator.csproj        ← 项目文件 (.NET Framework 4.7.1)
        ├── ModMain.cs                  ← 模组入口 (UserMod2)
        ├── TranslationManager.cs       ← 翻译管理器（加载/注册翻译）
        └── Patches.cs                  ← Harmony 补丁（拦截字符串查询）
```

## 开发指南

### 环境要求

- **Visual Studio 2019+**（或 JetBrains Rider）
- **.NET Framework 4.7.1**
- **缺氧游戏本体**（用于引用游戏 DLL）

### 编译

1. 用 Visual Studio 打开 `ModTranslator.sln`
2. 项目会自动通过注册表检测 Steam 路径（用于解析 DLL 引用）
3. 按 `Ctrl+Shift+B` 编译
4. 编译产物位于 `bin\Debug\` 或 `bin\Release\` 目录，需手动将 DLL 复制到模组目录

### 技术栈

| 组件                   | 说明             |
|----------------------|----------------|
| .NET Framework 4.7.1 | 目标框架（与缺氧运行时一致） |
| Harmony 2.0          | 运行时方法补丁库       |
| Newtonsoft.Json      | JSON 翻译文件解析    |
| KMod.UserMod2        | 缺氧模组 API 入口基类  |

### 核心 API（经反射确认）

| 类              | 方法签名                                                     | 所在程序集                     |
|----------------|----------------------------------------------------------|---------------------------|
| `Strings`      | `static StringEntry Get(string key)`                     | Assembly-CSharp-firstpass |
| `Strings`      | `static StringEntry Get(StringKey key0)`                 | Assembly-CSharp-firstpass |
| `Localization` | `static void OverloadStrings(Dictionary<string,string>)` | Assembly-CSharp           |
| `StringKey`    | 字段: `string String`, `int Hash`                          | Assembly-CSharp-firstpass |
| `StringEntry`  | 隐式转换 `op_Implicit → string`，字段: `string String`          | Assembly-CSharp-firstpass |

## 兼容性

- **适配版本**: U59-744825-SCRPAN
- **支持内容**: ALL（本体 + 全部 DLC）
- **最低游戏版本**: 744825
- **API 版本**: 2（Harmony 2.0）

## 许可

本项目仅供学习交流使用。游戏版权归 Klei Entertainment 所有。

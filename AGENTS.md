# SolidWorks 参数化设计插件

## 项目概述

这是一个 **SolidWorks CAD 软件的 COM 插件 (Add-in)**，使用 **C# / .NET 8.0 + WPF** 开发。核心功能是"参数化设计"——用户可以为 SolidWorks 模型定义可配置参数，然后通过输入新的参数值自动生成变体模型。

插件名称（中文）：**参数化设计工具**

---

## 技术栈

| 层级 | 技术 |
|------|------|
| 运行时 | .NET 8.0 (net8.0-windows) |
| UI 框架 | WPF (Windows Presentation Foundation) |
| 架构模式 | MVVM (Model-View-ViewModel) |
| 序列化 | Newtonsoft.Json 13.0.3 |
| Excel 支持 | EPPlus 7.1.3 |
| COM 互操作 | SolidWorks Interop (sldworks / swconst / swpublished) |
| 语言版本 | C# 12.0 |
| 构建工具 | PowerShell (build.ps1) + dotnet CLI |

---

## 项目结构

```
d:\code\solidworks\
├── solidworks.sln                              # Visual Studio 解决方案
├── register_addin.bat                          # 顶层注册脚本 (RegAsm 方式)
├── register_addin.ps1                          # 顶层注册脚本 (注册表方式)
├── addin_log.txt                               # 插件运行日志（桌面）
├── AGENTS.md                                   # 本文件
│
├── SolidWorks.ParametricAddin/                 # ★ 主项目
│   ├── SolidWorks.ParametricAddin.csproj       #   项目文件 (Library, net8.0-windows, UseWPF)
│   ├── build.ps1                               #   构建脚本
│   ├── register_addin.bat                      #   注册脚本 (COM Host 方式)
│   ├── unregister_addin.bat                    #   卸载脚本
│   │
│   ├── Models/                                 # 数据模型
│   │   ├── TemplateConfig.cs                   #   模板配置（顶层 JSON 结构）
│   │   ├── ParameterDefinition.cs              #   参数定义（映射到 SW 方程式/全局变量）
│   │   └── RuleDefinition.cs                   #   规则定义（IF-THEN-ELSE 条件逻辑）
│   │
│   ├── Services/                               # 业务逻辑服务
│   │   ├── ModelGeneratorService.cs            #   模型生成编排器（复制→替换→重建→保存）
│   │   ├── EquationService.cs                  #   SW 方程式读写服务
│   │   ├── ValidationService.cs                #   参数值校验（数值范围/必填/正则/选项）
│   │   ├── RuleEngineService.cs                #   规则引擎（评估条件并执行动作）
│   │   ├── NamingService.cs                    #   命名模板解析（{参数名} → 实际值）
│   │   └── FileService.cs                      #   文件系统操作工具
│   │
│   ├── Data/                                   # 数据持久化
│   │   ├── TemplateRepository.cs               #   模板 JSON 的 CRUD 操作
│   │   └── TemplateRegistry.cs                 #   模板索引注册表 (template_registry.json)
│   │
│   ├── TaskPane/                               # WPF UI 层
│   │   ├── MainTaskPaneControl.xaml(.cs)       #   主面板（模式切换容器）
│   │   ├── DesignModeView.xaml(.cs)            #   设计模式视图
│   │   ├── UsageModeView.xaml(.cs)             #   使用模式视图（含 ParameterTemplateSelector）
│   │   └── ViewModels/                         #   ViewModel 层
│   │       ├── BaseViewModel.cs                #     INotifyPropertyChanged 基类
│   │       ├── MainViewModel.cs                #     主 ViewModel（管理三个模式切换）
│   │       ├── DesignModeViewModel.cs          #     设计模式逻辑
│   │       ├── UsageModeViewModel.cs           #     使用模式逻辑（含 ParameterInputViewModel）
│   │       └── RelayCommand.cs                 #     ICommand 实现
│   │
│   ├── Helpers/                                # 工具类
│   │   ├── Logger.cs                           #   文件日志（桌面 sw_addin_log.txt）
│   │   ├── SolidWorksHelper.cs                 #   SW 通用工具（获取模板/输出目录）
│   │   └── Converters.cs                       #   WPF 值转换器（Bool ↔ Visibility）
│   │
│   ├── Properties/
│   │   └── AssemblyInfo.cs                     #   [assembly: ComVisible(true)]
│   │
│   ├── stubs/                                  # SolidWorks Interop 存根 DLL
│   │   ├── SolidWorks.Interop.sldworks/        #   ISldWorks, IModelDoc2, IEquationMgr 等
│   │   ├── SolidWorks.Interop.swconst/         #   枚举定义 (swDocumentTypes_e 等)
│   │   └── SolidWorks.Interop.swpublished/     #   ISwAddin 接口
│   │
│   └── lib/                                    # 编译输出：真实 SW DLL 或编译后的存根
│
└── SolidWorks.ParametricAddin.ComHost/         # COM 主机项目
    ├── SolidWorks.ParametricAddin.ComHost.csproj # EnableComHosting=true
    └── SwAddin.cs                              #   ★ 插件入口：ISwAddin 实现 + COM 注册
```

---

## 架构与设计

### MVVM 模式

- **Model**: `Models/` 中的类 — `TemplateConfig`, `ParameterDefinition`, `RuleDefinition`
- **View**: `TaskPane/` 中的 XAML 文件 — `MainTaskPaneControl`, `DesignModeView`, `UsageModeView`
- **ViewModel**: `TaskPane/ViewModels/` — 所有 ViewModel 继承自 `BaseViewModel`（实现了 `INotifyPropertyChanged`）

### 三个工作模式

| 模式 | 枚举值 | 说明 |
|------|--------|------|
| 模式选择 | `AppMode.ModeSelect` | 初始页面，让用户选择进入设计模式还是使用模式 |
| 设计模式 | `AppMode.Design` | 配置模板参数、约束和规则，保存为 JSON 文件 |
| 使用模式 | `AppMode.Usage` | 加载模板，填写参数值，生成新的 SolidWorks 模型 |

### 插件启动流程 (SwAddin.cs)

1. SolidWorks 启动时通过 COM 加载插件
2. 调用 `ConnectToSW()` → 添加菜单项 + 启动独立 WPF STA 线程
3. WPF 线程创建 `Application` + `Window`，加载 `MainTaskPaneControl`
4. 插件卸载时调用 `DisconnectFromSW()` → 关闭 WPF 窗口 + 移除菜单

**重要设计决策**：WPF 运行在独立的 STA 线程上（而非 SolidWorks 主线程），这是因为 SolidWorks 的 Win32 消息泵与 WPF 的嵌套 Dispatcher 框架不完全兼容（特别是模态对话框如 OpenFileDialog）。

### COM 注册

- **GUID**: `{B8E7F3D1-A2C4-4E5F-9A1B-3C6D8E0F4A2C}`
- **ProgId**: `SolidWorks.ParametricAddin.ComHost.SwAddin`
- **注册表路径**: `HKLM\SOFTWARE\SolidWorks\AddIns\{GUID}`
  - `(默认)` = 1（启用）
  - `Description` = "SolidWorks Parametric Design Add-in"
  - `Title` = "参数化设计工具"
- COM Host 方式需要额外注册 `HKLM\SOFTWARE\Classes\CLSID\{GUID}\InprocServer32`
- 代码中也有 `[ComRegisterFunction]` 和 `[ComUnregisterFunction]` 自动注册逻辑

---

## 核心业务流程

### 设计模式流程

1. 用户在 SolidWorks 中打开一个模板模型
2. 点击"从 SW 读取"→ `EquationService.ReadAllEquations()` 读取模型的所有方程式/全局变量
3. 每个方程自动生成一个 `ParameterDefinition`，可设置：
   - `DisplayName` — UI 显示名称
   - `ControlType` — 输入控件类型（TextBox / NumericTextBox / ComboBox / CheckBox / Slider）
   - `DefaultValue` — 默认值
   - `Unit` — 单位（如 mm, deg）
   - `MinValue`, `MaxValue`, `Step` — 数值约束
   - `RegexPattern` — 文本验证正则
   - `Options` — 下拉框选项列表
   - `Group` — 分组（默认"基本参数"）
   - `IsRequired` — 是否必填
   - `IsReadOnly` — 是否只读
4. 可选：添加 IF-THEN-ELSE 规则 (`RuleDefinition`) 实现条件逻辑
5. 配置命名模板（如 `传送带_L{Length}_W{Width}`）
6. 保存 → `TemplateRepository.Save()` 写入 JSON 文件 → 更新 `TemplateRegistry`

### 使用模式流程

1. 加载一个 JSON 模板配置文件
2. 渲染参数输入表单（根据 `ControlType` 使用 `ParameterTemplateSelector` 选择控件）
3. 用户填写参数值
4. 实时预览文件名 (`NamingService.Preview()`)
5. 点击"生成模型"：
   - `ValidationService.ValidateAll()` — 校验所有输入
   - `RuleEngineService.Evaluate()` — 评估规则，计算派生参数值
   - `ModelGeneratorService.Generate()` — 执行模型生成：
     1. 打开模板模型
     2. SaveAs 复制到输出路径
     3. 打开新模型
     4. 批量替换方程式值 (`EquationService.SetEquationsBatch()`)
     5. 强制重建 (`EditRebuild3()`)
     6. 更新自定义属性（元数据）
     7. 保存新模型

### 模板 JSON 结构

```json
{
  "TemplateName": "模板名称",
  "TemplateModelPath": "D:\\...\\模板.sldprt",
  "OutputDirectory": "D:\\...\\Generated",
  "NamingPattern": "传送带_L{Length}_W{Width}",
  "Description": "备注",
  "PreviewImagePath": "D:\\...\\preview.png",
  "Parameters": [ ... ],
  "Rules": [ ... ],
  "LastModified": "2026-...",
  "ConfigVersion": 1
}
```

模板 JSON 文件存储在：`%AppData%\SolidWorks\ParametricAddin\Templates\`

模板索引注册表：`template_registry.json`

---

## 构建系统

### 构建脚本: `build.ps1`

- **开发模式**（默认）：编译 stubs/ 下的存根 DLL，复制到 lib/，然后构建主项目
- **生产模式** (`-RealSw`)：从 SolidWorks 安装目录复制真实的 Interop DLL 到 lib/
- **清理模式** (`-Clean`)：删除 bin/, obj/, lib/*.dll

### Stub DLLs（存根）

位于 `stubs/` 目录下的三个项目，提供 SolidWorks COM 接口的最小定义。目的是在没有安装 SolidWorks 的机器上也能编译通过：

| Stub 项目 | 提供内容 |
|-----------|----------|
| `SolidWorks.Interop.sldworks` | `ISldWorks`, `IModelDoc2`, `IEquationMgr`, `ICustomPropertyManager`, `ModelDocExtension` |
| `SolidWorks.Interop.swconst` | `swDocumentTypes_e`, `swSaveAsOptions_e`, `swCustomInfoType_e` 等枚举 |
| `SolidWorks.Interop.swpublished` | `ISwAddin` 接口 |

### VSCode 配置

- `.vscode/tasks.json` — 两个构建任务（开发模式 / 真实 SW DLL）
- `.vscode/launch.json` — "Attach to SolidWorks" 调试配置（附加到 SLDWORKS.exe 进程）

---

## 依赖项

| 包名 | 版本 | 用途 |
|------|------|------|
| Newtonsoft.Json | 13.0.3 | JSON 序列化/反序列化（模板文件） |
| EPPlus | 7.1.3 | Excel 读写（尚未在代码中广泛使用） |

---

## 日志系统

`Logger` 类将日志写入桌面文件 `sw_addin_log.txt`，支持三个级别：
- **INFO** — 关键流程节点（连接、加载、生成）
- **ERROR** — 异常信息
- **TRACE** — 详细跟踪

WPF 未处理异常通过 `DispatcherUnhandledException` 和 `AppDomain.UnhandledException` 捕获，防止崩溃导致 SolidWorks 进程退出。

---

## 关键文件索引

| 文件 | 作用 |
|------|------|
| [SwAddin.cs](SolidWorks.ParametricAddin.ComHost/SwAddin.cs) | ★ 插件入口点，COM 注册，WPF 线程管理 |
| [MainViewModel.cs](SolidWorks.ParametricAddin/TaskPane/ViewModels/MainViewModel.cs) | 主 ViewModel，模式切换 |
| [DesignModeViewModel.cs](SolidWorks.ParametricAddin/TaskPane/ViewModels/DesignModeViewModel.cs) | 设计模式逻辑 |
| [UsageModeViewModel.cs](SolidWorks.ParametricAddin/TaskPane/ViewModels/UsageModeViewModel.cs) | 使用模式逻辑，参数输入，模型生成 |
| [ModelGeneratorService.cs](SolidWorks.ParametricAddin/Services/ModelGeneratorService.cs) | ★ 核心生成流程 |
| [EquationService.cs](SolidWorks.ParametricAddin/Services/EquationService.cs) | SW 方程式读写 |
| [TemplateConfig.cs](SolidWorks.ParametricAddin/Models/TemplateConfig.cs) | 模板配置顶层模型 |
| [ParameterDefinition.cs](SolidWorks.ParametricAddin/Models/ParameterDefinition.cs) | 参数定义模型 |
| [RuleDefinition.cs](SolidWorks.ParametricAddin/Models/RuleDefinition.cs) | 规则定义模型 |
| [build.ps1](SolidWorks.ParametricAddin/build.ps1) | 构建脚本 |
| [MainTaskPaneControl.xaml](SolidWorks.ParametricAddin/TaskPane/MainTaskPaneControl.xaml) | 主面板 UI |
| [DesignModeView.xaml](SolidWorks.ParametricAddin/TaskPane/DesignModeView.xaml) | 设计模式 UI |
| [UsageModeView.xaml](SolidWorks.ParametricAddin/TaskPane/UsageModeView.xaml) | 使用模式 UI |

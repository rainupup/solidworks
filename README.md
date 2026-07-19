# 参数化设计工具 · SolidWorks 参数化设计插件

> 一个面向 SolidWorks 的 COM 插件（Add-in）：定义一次模板，输入新参数即可自动产出变体模型。

## 项目简介

本插件使用 **C# / .NET 8.0 + WPF** 开发，通过 SolidWorks 的 COM 接口读取 / 写入模型方程式（全局变量），并结合规则引擎实现条件逻辑，最终一键重建并保存为新模型。适合需要批量派生相似零件 / 装配体的非标自动化场景。

## 核心功能

- **三种工作模式**
  - 模式选择：初始页面，选择进入设计或使用模式
  - 设计模式：从 SolidWorks 读取方程式，配置参数与规则，保存为模板
  - 使用模式：加载模板，填写参数，生成新模型
- **参数配置**：显示名、控件类型（文本框 / 数值框 / 下拉 / 勾选 / 滑块）、单位、数值范围、步长、正则校验、下拉选项、分组、必填 / 只读
- **条件逻辑**：支持 IF-THEN-ELSE 规则，根据参数值派生或约束其他参数
- **命名模板**：如 `传送带_L{Length}_W{Width}`，实时预览输出文件名
- **模板持久化**：以 JSON 保存（参数定义 + 规则 + 命名规则），可跨模型复用

## 技术栈

| 层级 | 技术 |
|------|------|
| 运行时 | .NET 8.0 (net8.0-windows) |
| UI 框架 | WPF (MVVM) |
| 序列化 | Newtonsoft.Json 13.0.3 |
| Excel 支持 | EPPlus 7.1.3 |
| COM 互操作 | SolidWorks Interop (sldworks / swconst / swpublished) |
| 语言版本 | C# 12.0 |

## 目录结构

```
solidworks/
├── solidworks.sln                 # Visual Studio 解决方案
├── register_addin.bat / .ps1      # 插件注册脚本（RegAsm / 注册表）
├── AGENTS.md                      # 项目说明（架构、流程、构建）
├── SolidWorks.ParametricAddin/    # ★ 主项目
│   ├── Models/                    # 数据模型（模板 / 参数 / 规则 定义）
│   ├── Services/                  # 业务逻辑（模型生成、方程式、校验、规则引擎、命名）
│   ├── Data/                      # 模板 JSON 的 CRUD 与索引注册表
│   ├── TaskPane/                  # WPF UI（主面板 / 设计模式 / 使用模式 / ViewModels）
│   ├── Helpers/                   # 日志、SW 工具、WPF 转换器
│   ├── stubs/                     # SolidWorks Interop 存根 DLL（无需安装 SW 即可编译）
│   ├── build.ps1                  # 构建脚本
│   └── register_addin.bat         # 注册脚本（COM Host 方式）
└── SolidWorks.ParametricAddin.ComHost/  # COM 主机项目（插件入口 ISwAddin）
    └── SwAddin.cs
```

## 构建与注册

```powershell
# 开发模式（使用存根 Interop，无需安装 SolidWorks 即可编译）
.\SolidWorks.ParametricAddin\build.ps1

# 生产模式（从 SolidWorks 安装目录复制真实 Interop DLL）
.\SolidWorks.ParametricAddin\build.ps1 -RealSw

# 注册插件（需以管理员身份运行）
register_addin.bat        # RegAsm 方式
register_addin.ps1        # 注册表方式
```

> 调试：用 Visual Studio 打开 `solidworks.sln`，通过 `.vscode/launch.json` 的 "Attach to SolidWorks" 配置附加到 `SLDWORKS.exe` 进程。

## 使用流程

1. 在 SolidWorks 中打开（或新建）一个模板模型。
2. **设计模式**：点击"从 SW 读取"导入方程式 → 配置参数与规则 → 设置命名模板 → 保存模板 JSON（存放于 `%AppData%\SolidWorks\ParametricAddin\Templates\`）。
3. **使用模式**：加载模板 → 填写参数值（实时预览文件名）→ 点击"生成模型"，插件会自动复制模板、批量替换方程式、强制重建并保存为新模型。

## 许可证

详见仓库根目录的 `LICENSE` 文件。

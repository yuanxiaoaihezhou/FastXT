# FastXT

FastXT 是一个快速、轻量级的 TXT 文本阅读器，专为 Windows x64 平台设计。

## 功能特性

- ⚡ **快速打开** - 采用高效的文件读取算法，支持大文件快速加载
- 📑 **智能章节分割** - 自动识别并分割章节，支持多种章节格式
- 🎯 **章节导航** - 提供章节目录，快速跳转到任意章节
- 🔤 **编码自动检测** - 自动识别文件编码（UTF-8, GBK, GB2312等）
- 🎨 **字体调节** - 支持字体大小调节，提供舒适的阅读体验
- ⌨️ **快捷键支持** - 丰富的键盘快捷键，提高使用效率

## 界面预览

FastXT 提供简洁直观的阅读界面：
- 左侧章节导航面板
- 右侧文本阅读区域
- 底部状态栏显示文件信息

## 系统要求

- Windows 10/11 (x64)
- .NET 8.0 Runtime（应用程序已包含，无需额外安装）

## 下载与安装

从 [Releases](https://github.com/yuanxiaoaihezhou/FastXT/releases) 页面下载最新版本的 `FastXT.exe`。

无需安装，直接运行即可使用。

## 使用说明

### 打开文件

- 点击菜单 **文件 → 打开** 或按 `Ctrl+O`
- 选择要阅读的 TXT 文件

### 章节导航

- 左侧面板显示检测到的章节目录
- 点击章节标题即可跳转
- 使用快捷键 `Ctrl+←` 和 `Ctrl+→` 切换上一章/下一章

### 字体调节

- `Ctrl++`：增大字体
- `Ctrl+-`：减小字体

### 支持的章节格式

FastXT 支持自动识别以下章节格式：

- `第X章 标题` 或 `第X回 标题`
- `Chapter X 标题`
- `1. 标题` 或 `1、标题`
- `【标题】`
- `==== 标题 ====`
- `---- 标题 ----`

## 开发

### 构建要求

- .NET 8.0 SDK
- Windows 开发环境（用于 WPF）

### 本地构建

```bash
# 克隆仓库
git clone https://github.com/yuanxiaoaihezhou/FastXT.git
cd FastXT

# 构建
dotnet build FastXT/FastXT.csproj

# 发布单文件可执行程序
dotnet publish FastXT/FastXT.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 自动构建

本项目使用 GitHub Actions 自动构建和发布：

- 每次推送到 main/master 分支时自动构建
- 创建版本标签（如 `v1.0.0`）时自动发布 Release

## 技术栈

- **语言**: C# 12
- **框架**: .NET 8.0 + WPF
- **平台**: Windows x64
- **CI/CD**: GitHub Actions

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！
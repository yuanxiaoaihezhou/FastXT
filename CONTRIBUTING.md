# 贡献指南

感谢您对 FastXT 项目的关注！

## 如何贡献

### 报告 Bug

如果您发现了 Bug，请通过 [GitHub Issues](https://github.com/yuanxiaoaihezhou/FastXT/issues) 报告，并包含以下信息：

- Bug 的详细描述
- 复现步骤
- 预期行为
- 实际行为
- 系统环境（Windows 版本、.NET 版本等）
- 相关截图（如果适用）

### 提交功能建议

如果您有好的功能想法，欢迎通过 Issues 提出。请描述：

- 功能的用途
- 为什么需要这个功能
- 您期望的实现方式

### 贡献代码

1. Fork 本仓库
2. 创建您的特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启一个 Pull Request

### 代码规范

- 遵循 C# 编码规范
- 添加必要的注释
- 确保代码能够正常构建
- 测试您的更改

## 开发环境设置

### 要求

- Visual Studio 2022 或 Visual Studio Code
- .NET 8.0 SDK
- Windows 10/11

### 构建项目

```bash
git clone https://github.com/yuanxiaoaihezhou/FastXT.git
cd FastXT
dotnet build FastXT/FastXT.csproj
```

### 运行项目

```bash
dotnet run --project FastXT/FastXT.csproj
```

## 测试

目前项目还没有自动化测试。我们欢迎添加测试用例的贡献！

## 文档

如果您改进了文档，这也是非常有价值的贡献！

## 许可证

通过贡献代码，您同意您的贡献将遵循 MIT 许可证。

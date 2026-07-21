---
sidebar_position: 2
---

# API 参考

完整的 API 参考文档由 `scripts/generate-api-docs.js` 从源代码 XML 注释自动生成。

## 生成方式

API 文档在以下时机自动生成：

- **本地开发**：运行 `npm run docs:api`
- **CI/CD**：推送到 `main` 分支时自动生成并部署

## 文档结构

生成的 API 文档按命名空间组织，每个类型包含：
- 类型声明和 XML `<summary>` 注释
- 方法签名和参数说明
- 属性说明

> 💡 如需修改 API 描述，请直接编辑源代码中的 XML 文档注释（`<summary>` 等标签），然后重新运行 `npm run docs:api`。本页面内容由 CI 自动更新，无需手动编辑。

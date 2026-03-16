# 后端（ASP.NET Core Web API）

目录：`backend/JumpRopeAss.Api`

## 依赖

- .NET SDK 8.x（本开发环境未预装 SDK，需在本机/服务器自行安装）
- MySQL 8.x

## 配置

使用环境变量或 `.env`（参考仓库根目录 `.env.example`），避免在代码中写死密码/密钥。

## 约定

- App 端接口前缀：`/api/v1/app`
- Admin 端接口前缀：`/api/v1/admin`
- 健康检查：`/api/v1/health`


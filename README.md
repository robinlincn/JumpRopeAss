# 湖南省学生跳绳协会（微信小程序 + 后台管理）

本仓库用于开发“湖南省学生跳绳协会”业务系统，包含 H5（设计稿验证）、后台管理端与 ASP.NET Core 后端接口，覆盖资讯、赛事（创建/报名/审核/缴费）、证书（首次/补证、电子证书展示/校验）、基础数据与留痕管理等能力。

## 目录结构

- `Design/`：设计稿/素材
- `docs/`：需求/接口/状态机等文档
- `backend/`：ASP.NET Core Web API（统一提供 H5/小程序/后台管理端接口）
- `admin-web/`：后台管理端前端（React + Ant Design）
- `h5/`：H5 设计稿验证项目（用于先对齐页面与交互）
- `database/`：MySQL 建表脚本（含中文注释）

## 技术栈

- H5：Vite + Vue 3（`h5/`）
- 后台管理端：Vite + React + Ant Design（`admin-web/`）
- 后端：ASP.NET Core (.NET 8) + EF Core + MySQL（`backend/`）

## 环境变量（禁止在代码中硬编码密码/密钥）

复制 `.env.example` 为 `.env` 并填入真实配置（本仓库已将 `.env` 忽略）。

关键变量：

- MySQL：`DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD`
- JWT：`JWT_ISSUER/JWT_AUDIENCE/JWT_SIGNING_KEY`
- 微信：`WECHAT_APPID/WECHAT_SECRET`
- 微信支付：`WECHAT_PAY_*`

## 数据库

建表脚本：`database/schema.sql`（字段/表均带中文注释）。

注意：生产环境请务必更换数据库 root 密码，并启用 IP 白名单/最小权限账号。

后端启动时会执行必要的建表/补列逻辑（`CREATE TABLE IF NOT EXISTS` / `AddColumnIfMissing`），用于开发环境快速启动；生产环境建议用 DBA 流程管理变更。

## 本地启动（前端）

后台管理端：

```bash
cd admin-web
npm install
npm run dev
```

H5 设计稿验证：

```bash
cd h5
npm install
npm run dev
```

## 本地启动（后端）

后端位于 `backend/JumpRopeAss.Api`，需要安装 .NET SDK 8.x。配置使用环境变量或 `.env`（参考 `.env.example`）。

```bash
cd backend/JumpRopeAss.Api
dotnet run --urls "http://localhost:5005"
```

## 后台账号与权限（开发环境）

- 开发环境启动后端会确保存在默认账号：
  - 账号：`admin`
  - 密码：`123456`
- 后台“账号与角色”页面支持：
  - 账号：新增/编辑/删除、启用/停用、重置密码、分配角色
  - 角色：新增/编辑/删除、配置权限点（用于后续做菜单/接口授权）

## 系统参数（后台可配置）

后台「系统设置 → 系统参数」支持集中配置系统级参数与支付/短信/存储等参数：

- 系统：系统名称/简称/客服电话/Logo URL
- 报名：默认报名须知（支持 HTML）
- 证书：证书查询链接前缀（用于生成二维码/复制查询链接）
- 支付：微信支付配置（敏感字段脱敏显示，留空不覆盖）
- 短信：短信服务商与密钥（敏感字段脱敏显示，留空不覆盖）
- 存储：本地/OSS 存储配置（敏感字段脱敏显示，留空不覆盖）

## 安全提示（务必阅读）

- 生产环境请务必：
  - 配置强随机 `JWT_SIGNING_KEY`
  - 修改默认账号密码并创建最小权限账号
  - 禁止把密钥/私钥写入仓库（仅使用环境变量/安全配置中心）

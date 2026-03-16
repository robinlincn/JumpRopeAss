# 湖南省学生跳绳协会（微信小程序 + 后台管理）

本仓库用于开发“湖南省学生跳绳协会”微信小程序与后台管理系统，覆盖资讯展示、赛事全流程（创建/报名/审核/缴费）、证书（首次/补证定价、电子证书展示）、基础数据与留痕管理等能力。

## 目录结构

- [Design](file:///e:/Project/JumpRopeAss/Design)：现有参考截图
- `docs/`：需求/接口/状态机等文档
- `backend/`：ASP.NET Core Web API（统一提供小程序与后台管理端接口）
- `admin-web/`：后台管理端前端（Ant Design 风格）
- `h5/`：H5 设计稿验证项目（用于先对齐页面与交互）
- `database/`：MySQL 建表脚本（含中文注释）

## 技术栈

- 小程序端：uni-app（仅微信小程序端）  
  - 开发顺序：设计稿 → H5 页面（本仓库 `h5/`）→ uni-app 落地（后续在 `miniapp/` 目录新增）
- 后端：ASP.NET Core Web API + MySQL
- 后台管理端：React + Ant Design（界面风格参考 Ant Design Pro）

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

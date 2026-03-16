# 关键规则（冻结版）

## 教练代报名（仅第一教练员）

当报名方式为“教练代报名”时，服务端必须校验：

- `person_athlete.first_coach_person_id == coach.person_id`
- 教练员角色有效（`person_coach.status=1`）
- 运动员状态有效（`person_athlete.status=1`）

不满足则返回错误码：`40310 ENTRY_COACH_NOT_FIRST_COACH`。

## 我的认证需后台审核

- 小程序端提交认证后状态为 `PENDING`
- 后台审核通过/驳回后更新状态为 `APPROVED/REJECTED`


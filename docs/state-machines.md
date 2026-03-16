# 状态机

## 报名审核与缴费（event_entry.status）

枚举：

- 0 SUBMITTED：已提交（待审核）
- 1 REJECTED：审核驳回
- 2 APPROVED_PENDING_PAY：审核通过（待缴费）
- 3 PAYING：支付中（已下单）
- 4 PAID：已缴费（支付成功）
- 5 CONFIRMED：报名成功/已确认
- 6 CANCELLED：已取消
- 7 PAY_FAILED：支付失败（可重试）
- 8 REFUNDING：退款中（可选）
- 9 REFUNDED：已退款（可选）

流转：

- SUBMITTED → APPROVED_PENDING_PAY / REJECTED（后台审核）
- APPROVED_PENDING_PAY → PAYING（小程序去支付创建订单）
- PAYING → PAID（微信回调验签成功）
- PAID → CONFIRMED（服务端自动确认；如需二次复核可改为后台确认）
- APPROVED_PENDING_PAY → CANCELLED（未支付可取消）
- PAY_FAILED → PAYING（重试支付）
- PAID/CONFIRMED → REFUNDING → REFUNDED（可选，后台发起退款）

活动配置开关（event.need_audit / event.need_pay）对状态机的影响：

- need_audit=0：提交后直接进入“待缴费”或“已确认”
- need_pay=0：审核通过后直接“已确认”

## 我的认证（user_identity_submit.status）

- 0 PENDING：待审核
- 1 APPROVED：通过
- 2 REJECTED：驳回


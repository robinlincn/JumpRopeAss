using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 支付订单表
/// </summary>
public partial class PayOrder
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 业务类型：1活动报名；2首次发证；3补证
    /// </summary>
    public sbyte BizType { get; set; }

    /// <summary>
    /// 业务ID（如entryId/certIssueId）
    /// </summary>
    public ulong BizId { get; set; }

    /// <summary>
    /// 下单账号ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 订单金额（分）
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 状态：0待支付；1已支付；2已关闭；3已退款
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 微信商户订单号
    /// </summary>
    public string? WxOutTradeNo { get; set; }

    /// <summary>
    /// 支付完成时间
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

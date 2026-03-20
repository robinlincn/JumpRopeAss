using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 微信支付回调记录（原文落库）
/// </summary>
public partial class PayWechatNotify
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 支付订单ID
    /// </summary>
    public ulong? PayOrderId { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    public string? OutTradeNo { get; set; }

    /// <summary>
    /// 微信支付单号
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// 回调原文（用于审计与排障）
    /// </summary>
    public string NotifyRaw { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

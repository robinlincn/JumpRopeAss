using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 活动报名表（状态机驱动）
/// </summary>
public partial class EventEntry
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 活动ID
    /// </summary>
    public ulong EventId { get; set; }

    /// <summary>
    /// 组别ID
    /// </summary>
    public ulong GroupId { get; set; }

    /// <summary>
    /// 运动员人员ID
    /// </summary>
    public ulong AthletePersonId { get; set; }

    /// <summary>
    /// 报名方式：1运动员；2家长；3教练（第一教练）
    /// </summary>
    public sbyte EnrollChannel { get; set; }

    /// <summary>
    /// 报名提交账号ID
    /// </summary>
    public ulong EnrollUserId { get; set; }

    /// <summary>
    /// 状态：0已提交待审核；1审核驳回；2审核通过待缴费；3支付中；4已缴费；5已确认；6已取消；7支付失败；8退款中；9已退款
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 审核备注/驳回原因
    /// </summary>
    public string? AuditRemark { get; set; }

    /// <summary>
    /// 支付订单ID
    /// </summary>
    public ulong? PayOrderId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

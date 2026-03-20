using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 用户认证提交记录（需后台审核）
/// </summary>
public partial class UserIdentitySubmit
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 账号ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 身份证号（敏感）
    /// </summary>
    public string IdCardNo { get; set; } = null!;

    /// <summary>
    /// 手机号
    /// </summary>
    public string Mobile { get; set; } = null!;

    /// <summary>
    /// 状态：0待审核；1通过；2驳回
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 驳回原因
    /// </summary>
    public string? RejectReason { get; set; }

    /// <summary>
    /// 审核后台账号ID
    /// </summary>
    public ulong? ReviewedByAdminId { get; set; }

    /// <summary>
    /// 审核时间
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

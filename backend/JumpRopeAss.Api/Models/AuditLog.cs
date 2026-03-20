using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 操作审计日志
/// </summary>
public partial class AuditLog
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 操作人类型：1后台；2小程序
    /// </summary>
    public sbyte ActorType { get; set; }

    /// <summary>
    /// 操作人ID（后台用户ID或账号ID）
    /// </summary>
    public ulong ActorId { get; set; }

    /// <summary>
    /// 动作编码（如identity_approve/first_coach_change）
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// 业务类型（如event_entry/user_identity）
    /// </summary>
    public string? BizType { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    public ulong? BizId { get; set; }

    /// <summary>
    /// 详情JSON（变更前后摘要）
    /// </summary>
    public string? DetailJson { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

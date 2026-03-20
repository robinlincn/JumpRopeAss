using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 运动员学校/培训机构变更历史（留痕可追溯）
/// </summary>
public partial class AthleteAffiliationHistory
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 运动员人员ID
    /// </summary>
    public ulong AthletePersonId { get; set; }

    /// <summary>
    /// 变更类型：1学校；2培训机构
    /// </summary>
    public sbyte ChangeType { get; set; }

    /// <summary>
    /// 变更前机构ID
    /// </summary>
    public ulong? BeforeOrgId { get; set; }

    /// <summary>
    /// 变更后机构ID
    /// </summary>
    public ulong? AfterOrgId { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 操作人类型：1后台管理员；2本人；3家长；4教练
    /// </summary>
    public sbyte OperatorType { get; set; }

    /// <summary>
    /// 操作人账号ID
    /// </summary>
    public ulong? OperatorUserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

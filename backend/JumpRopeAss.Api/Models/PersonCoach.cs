using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 教练员角色表（必须绑定机构）
/// </summary>
public partial class PersonCoach
{
    /// <summary>
    /// 人员ID
    /// </summary>
    public ulong PersonId { get; set; }

    /// <summary>
    /// 所属机构ID（培训机构或中小学）
    /// </summary>
    public ulong OrgId { get; set; }

    /// <summary>
    /// 教练等级（可选）
    /// </summary>
    public string? CoachLevel { get; set; }

    /// <summary>
    /// 状态：1有效；0无效
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 运动员角色表（中小学必填；培训机构选填；第一教练员终身绑定）
/// </summary>
public partial class PersonAthlete
{
    /// <summary>
    /// 人员ID
    /// </summary>
    public ulong PersonId { get; set; }

    /// <summary>
    /// 所属中小学ID（必填，指向school.id）
    /// </summary>
    public ulong SchoolId { get; set; }

    /// <summary>
    /// 年级（如一年级/七年级等）
    /// </summary>
    public string? GradeName { get; set; }

    /// <summary>
    /// 班级（如一班/七年级三班等）
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// 所属培训机构ID（选填）
    /// </summary>
    public ulong? TrainingOrgId { get; set; }

    /// <summary>
    /// 第一教练员人员ID（终身绑定，仅平台可改）
    /// </summary>
    public ulong? FirstCoachPersonId { get; set; }

    /// <summary>
    /// 状态：1有效；0无效
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

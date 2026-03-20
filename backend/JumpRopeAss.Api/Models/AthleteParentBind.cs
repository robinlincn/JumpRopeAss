using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 运动员-家长绑定（支持多个家长）
/// </summary>
public partial class AthleteParentBind
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
    /// 家长人员ID
    /// </summary>
    public ulong ParentPersonId { get; set; }

    /// <summary>
    /// 关系：父亲/母亲/监护人等
    /// </summary>
    public string? Relation { get; set; }

    /// <summary>
    /// 状态：1有效；0无效
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

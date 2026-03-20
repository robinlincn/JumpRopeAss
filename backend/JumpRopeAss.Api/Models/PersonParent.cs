using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 家长角色表
/// </summary>
public partial class PersonParent
{
    /// <summary>
    /// 人员ID
    /// </summary>
    public ulong PersonId { get; set; }

    /// <summary>
    /// 状态：1有效；0无效
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

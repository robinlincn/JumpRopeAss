using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 后台用户-角色关联
/// </summary>
public partial class AdminUserRole
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 后台用户ID
    /// </summary>
    public ulong AdminUserId { get; set; }

    /// <summary>
    /// 后台角色ID
    /// </summary>
    public ulong AdminRoleId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 后台用户
/// </summary>
public partial class AdminUser
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// 状态：1启用；0停用
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 小程序微信账号信息
/// </summary>
public partial class UserWechat
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 微信openid
    /// </summary>
    public string Openid { get; set; } = null!;

    /// <summary>
    /// 微信unionid
    /// </summary>
    public string? Unionid { get; set; }

    /// <summary>
    /// 微信昵称
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 微信头像
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 账号表
/// </summary>
public partial class UserAccount
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 账号类型：1小程序；2后台
    /// </summary>
    public sbyte UserType { get; set; }

    /// <summary>
    /// 状态：1正常；0禁用
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

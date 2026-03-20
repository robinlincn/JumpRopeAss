using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 人员主表
/// </summary>
public partial class Person
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// 性别：0未知；1男；2女
    /// </summary>
    public sbyte? Gender { get; set; }

    /// <summary>
    /// 身份证号（敏感）
    /// </summary>
    public string? IdCardNo { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 状态：1正常；0禁用
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 删除时间（软删）
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

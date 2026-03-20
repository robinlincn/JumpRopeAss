using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 会员单位（展示）
/// </summary>
public partial class MemberUnit
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 单位名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// LOGO
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    public string? Intro { get; set; }

    /// <summary>
    /// 详情HTML
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }

    /// <summary>
    /// 状态：1启用；0停用
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

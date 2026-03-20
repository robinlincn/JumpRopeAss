using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 机构表（培训机构/中小学）
/// </summary>
public partial class Org
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 机构类型：1培训机构；2中小学
    /// </summary>
    public sbyte OrgType { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 机构简称
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// 省
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 区县
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 联系人姓名
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系人电话
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 状态：1启用；0停用
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

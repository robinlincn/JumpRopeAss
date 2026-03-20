using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 证书表（支持电子版展示）
/// </summary>
public partial class Certificate
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 证书编号（唯一）
    /// </summary>
    public string CertNo { get; set; } = null!;

    /// <summary>
    /// 证书类型ID
    /// </summary>
    public ulong CertTypeId { get; set; }

    /// <summary>
    /// 持证人员ID
    /// </summary>
    public ulong HolderPersonId { get; set; }

    /// <summary>
    /// 发证场景：1首次；2补证
    /// </summary>
    public sbyte IssueScene { get; set; }

    /// <summary>
    /// 发证时间
    /// </summary>
    public DateTime IssueAt { get; set; }

    /// <summary>
    /// 状态：1有效；2作废
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 电子证书文件URL（PDF/图片）
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 证书定价（首次/补证分开定价）
/// </summary>
public partial class CertPricing
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 证书类型ID
    /// </summary>
    public ulong CertTypeId { get; set; }

    /// <summary>
    /// 发证场景：1首次；2补证
    /// </summary>
    public sbyte IssueScene { get; set; }

    /// <summary>
    /// 价格（分）
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 生效时间
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
}

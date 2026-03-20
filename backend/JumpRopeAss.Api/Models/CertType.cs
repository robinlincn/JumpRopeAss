using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 证书类型表
/// </summary>
public partial class CertType
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 类型编码：coach_cert/athlete_level等
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 证书类型名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 状态：1启用；0停用
    /// </summary>
    public sbyte Status { get; set; }
}

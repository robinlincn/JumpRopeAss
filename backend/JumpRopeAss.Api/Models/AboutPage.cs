using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 关于协会页面内容
/// </summary>
public partial class AboutPage
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 页面编码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 内容HTML
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// 协会名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 协会地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 协会Logo（URL或base64 data url）
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// 协会概述HTML
    /// </summary>
    public string? OverviewHtml { get; set; }

    /// <summary>
    /// 协会历史HTML
    /// </summary>
    public string? HistoryHtml { get; set; }

    /// <summary>
    /// 协会荣誉HTML
    /// </summary>
    public string? HonorsHtml { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

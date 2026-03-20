using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 新闻资讯
/// </summary>
public partial class NewsArticle
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 封面图
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 摘要
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 内容类型：text图文；video视频
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 正文HTML
    /// </summary>
    public string? ContentHtml { get; set; }

    /// <summary>
    /// 视频URL
    /// </summary>
    public string? VideoUrl { get; set; }

    /// <summary>
    /// 标签（逗号分隔）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 浏览量
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime? PublishAt { get; set; }

    /// <summary>
    /// 状态：1已发布；0草稿；2下线
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
}

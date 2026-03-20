﻿﻿﻿using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// Banner配置
/// </summary>
public partial class Banner
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 位置：home等
    /// </summary>
    public string Position { get; set; } = null!;

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 类型：image/video
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// 图片URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 视频URL
    /// </summary>
    public string? VideoUrl { get; set; }

    /// <summary>
    /// 跳转类型：none/news/event/url
    /// </summary>
    public string? LinkType { get; set; }

    /// <summary>
    /// 跳转值（ID或URL）
    /// </summary>
    public string? LinkValue { get; set; }

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

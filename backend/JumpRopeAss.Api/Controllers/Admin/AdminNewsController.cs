using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/news")]
[Authorize]
public class AdminNewsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminNewsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? contentType,
        [FromQuery] sbyte? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var query = _db.NewsArticles.AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(x => x.Title.Contains(keyword));

        if (!string.IsNullOrEmpty(contentType) && contentType != "all")
            query = query.Where(x => x.ContentType == contentType);

        if (status.HasValue && status.Value != -1) // Assuming -1 or missing means 'all'
            query = query.Where(x => x.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<NewsArticle>> GetDetail(long id)
    {
        var item = await _db.NewsArticles.FindAsync((ulong)id);
        if (item == null) return ApiResponse<NewsArticle>.Fail(ErrorCodes.NotFound, "资讯不存在");
        return ApiResponse<NewsArticle>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<NewsArticle>> Create([FromBody] NewsDto dto)
    {
        if (!DataUrlImage.IsValid(dto.CoverUrl)) return ApiResponse<NewsArticle>.Fail(ErrorCodes.InvalidParam, "封面图片数据无效，请重新上传");
        var news = new NewsArticle
        {
            Title = dto.Title,
            CoverUrl = dto.CoverUrl,
            Summary = dto.Summary,
            ContentType = dto.ContentType,
            ContentHtml = dto.ContentHtml,
            VideoUrl = dto.VideoUrl,
            Tags = dto.Tags,
            ViewCount = 0,
            Status = dto.Status,
            PublishAt = dto.Status == 1 ? DateTime.Now : null,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.NewsArticles.Add(news);
        await _db.SaveChangesAsync();
        return ApiResponse<NewsArticle>.Ok(news);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<NewsArticle>> Update(long id, [FromBody] NewsDto dto)
    {
        var news = await _db.NewsArticles.FindAsync((ulong)id);
        if (news == null) return ApiResponse<NewsArticle>.Fail(ErrorCodes.NotFound, "资讯不存在");
        if (!DataUrlImage.IsValid(dto.CoverUrl)) return ApiResponse<NewsArticle>.Fail(ErrorCodes.InvalidParam, "封面图片数据无效，请重新上传");

        news.Title = dto.Title;
        news.CoverUrl = dto.CoverUrl;
        news.Summary = dto.Summary;
        news.ContentType = dto.ContentType;
        news.ContentHtml = dto.ContentHtml;
        news.VideoUrl = dto.VideoUrl;
        news.Tags = dto.Tags;
        
        if (news.Status != 1 && dto.Status == 1 && news.PublishAt == null)
            news.PublishAt = DateTime.Now;
            
        news.Status = dto.Status;
        news.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return ApiResponse<NewsArticle>.Ok(news);
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var news = await _db.NewsArticles.FindAsync((ulong)id);
        if (news == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "资讯不存在");

        _db.NewsArticles.Remove(news);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class NewsDto
{
    public string Title { get; set; } = null!;
    public string? CoverUrl { get; set; }
    public string? Summary { get; set; }
    public string ContentType { get; set; } = null!;
    public string? ContentHtml { get; set; }
    public string? VideoUrl { get; set; }
    public string? Tags { get; set; }
    public sbyte Status { get; set; }
}

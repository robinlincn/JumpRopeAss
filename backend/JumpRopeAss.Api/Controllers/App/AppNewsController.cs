using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/news")]
public sealed class AppNewsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppNewsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.NewsArticles.AsNoTracking().OrderByDescending(x => x.PublishAt ?? x.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new
        {
            id = x.Id,
            title = x.Title,
            coverUrl = x.CoverUrl,
            summary = x.Summary,
            publishAt = x.PublishAt,
            viewCount = x.ViewCount,
            contentType = x.ContentType,
        }).ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<object>> Detail([FromRoute] long id)
    {
        var item = await _db.NewsArticles.AsNoTracking()
            .Where(x => x.Id == (ulong)id)
            .Select(x => new
            {
                id = x.Id,
                title = x.Title,
                coverUrl = x.CoverUrl,
                summary = x.Summary,
                contentHtml = x.ContentHtml,
                contentType = x.ContentType,
                videoUrl = x.VideoUrl,
                publishAt = x.PublishAt,
                viewCount = x.ViewCount,
            })
            .FirstOrDefaultAsync();

        if (item is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "资讯不存在");
        return ApiResponse<object>.Ok(item);
    }
}


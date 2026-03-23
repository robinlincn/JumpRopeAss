using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/home")]
public sealed class AppHomeController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppHomeController(AppDbContext db) => _db = db;

    [HttpGet("aggregate")]
    public async Task<ApiResponse<object>> GetAggregate()
    {
        var banners = await _db.Banners.AsNoTracking()
            .Where(x => x.Status == 1)
            .OrderByDescending(x => x.SortNo)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                id = x.Id,
                title = x.Title,
                mediaType = x.MediaType,
                imageUrl = x.ImageUrl,
                videoUrl = x.VideoUrl,
                linkType = x.LinkType,
                linkValue = x.LinkValue,
            })
            .ToListAsync();

        var news = await _db.NewsArticles.AsNoTracking()
            .OrderByDescending(x => x.PublishAt ?? x.CreatedAt)
            .Take(3)
            .Select(x => new
            {
                id = x.Id,
                title = x.Title,
                coverUrl = x.CoverUrl,
                summary = x.Summary,
                publishAt = x.PublishAt,
                viewCount = x.ViewCount,
                contentType = x.ContentType,
            })
            .ToListAsync();

        var athletesCount = await _db.PersonAthletes.CountAsync();
        var judgesCount = await _db.PersonJudges.CountAsync();
        var coachesCount = await _db.PersonCoaches.CountAsync();
        var eventsCount = await _db.Events.CountAsync(x => x.Status == 1);

        var stats = new
        {
            athletes = athletesCount,
            judges = judgesCount,
            coaches = coachesCount,
            events = eventsCount,
            updatedAt = DateTime.UtcNow,
        };

        return ApiResponse<object>.Ok(new
        {
            banners = banners,
            stats = stats,
            news = news
        });
    }
}

using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/banners")]
public sealed class AppBannersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppBannersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List()
    {
        var items = await _db.Banners.AsNoTracking()
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

        return ApiResponse<object>.Ok(new { items });
    }
}


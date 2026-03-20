using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/about")]
public sealed class AppAboutController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppAboutController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> Get()
    {
        var item = await _db.AboutPages.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                id = x.Id,
                code = x.Code,
                title = x.Title,
                name = x.Name,
                address = x.Address,
                logoUrl = x.LogoUrl,
                overviewHtml = x.OverviewHtml,
                historyHtml = x.HistoryHtml,
                honorsHtml = x.HonorsHtml,
                updatedAt = x.UpdatedAt,
            })
            .FirstOrDefaultAsync();

        if (item is null) return ApiResponse<object>.Ok(new { item = (object?)null });
        return ApiResponse<object>.Ok(new { item });
    }
}


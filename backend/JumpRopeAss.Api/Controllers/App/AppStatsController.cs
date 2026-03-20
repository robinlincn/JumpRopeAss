using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/stats")]
public sealed class AppStatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppStatsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> Get()
    {
        var now = DateTime.UtcNow;
        var stats = new
        {
            athletes = await _db.PersonAthletes.AsNoTracking().CountAsync(),
            judges = await _db.PersonJudges.AsNoTracking().CountAsync(),
            coaches = await _db.PersonCoaches.AsNoTracking().CountAsync(),
            events = await _db.Events.AsNoTracking().CountAsync(x => x.Status == 1),
            updatedAt = now,
        };

        return ApiResponse<object>.Ok(stats);
    }
}


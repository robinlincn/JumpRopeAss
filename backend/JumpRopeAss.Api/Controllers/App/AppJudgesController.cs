using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/judges")]
public sealed class AppJudgesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppJudgesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List([FromQuery] string? keyword = null, [FromQuery] string? level = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = from j in _db.PersonJudges.AsNoTracking()
                    join p in _db.People.AsNoTracking() on j.PersonId equals p.Id
                    where p.DeletedAt == null && j.Status == 1
                    select new { j, p };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x => EF.Functions.Like(x.p.FullName, $"%{kw}%"));
        }
        
        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.j.JudgeLevel == level);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.j.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                id = x.j.PersonId,
                name = x.p.FullName,
                avatar = x.p.AvatarUrl,
                level = x.j.JudgeLevel,
                role = "裁判员"
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }
}

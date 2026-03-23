using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/coaches")]
public sealed class AppCoachesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppCoachesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List([FromQuery] string? keyword = null, [FromQuery] string? level = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = from c in _db.PersonCoaches.AsNoTracking()
                    join p in _db.People.AsNoTracking() on c.PersonId equals p.Id
                    where p.DeletedAt == null && c.Status == 1
                    select new { c, p };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x => EF.Functions.Like(x.p.FullName, $"%{kw}%"));
        }
        
        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.c.CoachLevel == level);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                id = x.c.PersonId,
                name = x.p.FullName,
                avatar = x.p.AvatarUrl,
                level = x.c.CoachLevel,
                role = "教练员"
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }
}

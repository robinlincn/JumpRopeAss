using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/athletes")]
public sealed class AppAthletesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppAthletesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List([FromQuery] string? keyword = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = from a in _db.PersonAthletes.AsNoTracking()
                    join p in _db.People.AsNoTracking() on a.PersonId equals p.Id
                    where p.DeletedAt == null && a.Status == 1
                    select new { a, p };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x => EF.Functions.Like(x.p.FullName, $"%{kw}%"));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                id = x.a.PersonId,
                name = x.p.FullName,
                avatar = x.p.AvatarUrl,
                level = "学员", // or maybe from some assessment record, but "学员" is fine
                role = "运动员"
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }
}

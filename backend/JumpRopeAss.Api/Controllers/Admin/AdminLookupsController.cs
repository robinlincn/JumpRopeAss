using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/lookups")]
[Authorize]
public sealed class AdminLookupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminLookupsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("orgs")]
    public async Task<ApiResponse<object>> Orgs([FromQuery] sbyte? type, [FromQuery] string? keyword, [FromQuery] int size = 200)
    {
        var q = _db.Orgs.AsQueryable();
        if (type.HasValue) q = q.Where(x => x.OrgType == type.Value);
        if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(x => x.Name.Contains(keyword));
        var items = await q.OrderBy(x => x.Name).Take(size).Select(x => new { x.Id, OrgName = x.Name, x.OrgType }).ToListAsync();
        return ApiResponse<object>.Ok(items);
    }

    [HttpGet("schools")]
    public async Task<ApiResponse<object>> Schools([FromQuery] string? keyword, [FromQuery] int size = 200)
    {
        var q = _db.Schools.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(x => x.Name.Contains(keyword));
        var items = await q.OrderBy(x => x.Name).Take(size).Select(x => new { x.Id, x.Name }).ToListAsync();
        return ApiResponse<object>.Ok(items);
    }

    [HttpGet("coaches")]
    public async Task<ApiResponse<object>> Coaches([FromQuery] string? keyword, [FromQuery] int size = 200)
    {
        var q =
            from c in _db.PersonCoaches
            join p in _db.People on c.PersonId equals p.Id
            select new { p.Id, p.FullName, p.Mobile };
        if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(x => x.FullName.Contains(keyword) || x.Mobile.Contains(keyword));
        var items = await q.OrderBy(x => x.FullName).Take(size).ToListAsync();
        return ApiResponse<object>.Ok(items);
    }
}

using System.Security.Claims;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/me")]
public sealed class AppMeController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppMeController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet]
    public ApiResponse<object> Me()
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");
        return ApiResponse<object>.Ok(new { userId });
    }

    [Authorize]
    [HttpGet("entries")]
    public async Task<ApiResponse<object>> MyEntries([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.EventEntries.AsNoTracking().Where(x => x.EnrollUserId == userId);
        var total = await query.CountAsync();
        var items = await (
                from e in query
                join ev0 in _db.Events.AsNoTracking() on e.EventId equals ev0.Id into ev1
                from ev in ev1.DefaultIfEmpty()
                join g0 in _db.EventGroups.AsNoTracking() on e.GroupId equals g0.Id into g1
                from g in g1.DefaultIfEmpty()
                orderby e.CreatedAt descending
                select new
                {
                    id = e.Id,
                    eventId = e.EventId,
                    eventTitle = ev != null ? ev.Title : null,
                    groupId = e.GroupId,
                    groupName = g != null ? g.Name : null,
                    status = e.Status,
                    createdAt = e.CreatedAt,
                })
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    private ulong GetUserIdOrZero()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return ulong.TryParse(sub, out var v) ? v : 0UL;
    }
}


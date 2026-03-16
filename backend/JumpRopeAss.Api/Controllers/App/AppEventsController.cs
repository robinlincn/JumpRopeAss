using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/events")]
public sealed class AppEventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppEventsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List([FromQuery] int? type = null, [FromQuery] int? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Events.AsNoTracking().AsQueryable();
        if (type is not null) query = query.Where(x => x.EventType == type);
        if (status is not null) query = query.Where(x => x.Status == status);

        query = query.OrderByDescending(x => x.SignupStartAt ?? x.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new
        {
            id = x.Id,
            eventType = x.EventType,
            title = x.Title,
            coverUrl = x.CoverUrl,
            signupStartAt = x.SignupStartAt,
            signupEndAt = x.SignupEndAt,
            eventDate = x.EventDate,
            location = x.Location,
            status = x.Status,
        }).ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    [HttpGet("{id:ulong}")]
    public async Task<ApiResponse<object>> Detail([FromRoute] ulong id)
    {
        var item = await _db.Events.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                id = x.Id,
                eventType = x.EventType,
                signupScope = x.SignupScope,
                title = x.Title,
                coverUrl = x.CoverUrl,
                signupStartAt = x.SignupStartAt,
                signupEndAt = x.SignupEndAt,
                eventDate = x.EventDate,
                location = x.Location,
                descriptionHtml = x.DescriptionHtml,
                status = x.Status,
            })
            .FirstOrDefaultAsync();

        if (item is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");
        return ApiResponse<object>.Ok(item);
    }
}


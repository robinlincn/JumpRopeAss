using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/events")]
public sealed class AppEventGroupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppEventGroupsController(AppDbContext db) => _db = db;

    [HttpGet("{id:long}/groups")]
    public async Task<ApiResponse<object>> Groups([FromRoute] long id)
    {
        var eventId = (ulong)id;
        var exists = await _db.Events.AsNoTracking().AnyAsync(x => x.Id == eventId);
        if (!exists) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");

        var items = await _db.EventGroups.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.SortNo)
            .Select(x => new
            {
                id = x.Id,
                code = x.Code,
                name = x.Name,
                feeAmount = x.FeeAmount,
                quota = x.Quota,
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { eventId, items });
    }
}


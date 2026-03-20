using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminDashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> Get()
    {
        var now = DateTime.UtcNow;
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var tomorrowStart = todayStart.AddDays(1);
        var soonEnd = now.AddDays(3);

        var pendingEntries = await _db.EventEntries.AsNoTracking().CountAsync(x => x.Status == 0);
        var pendingIdentities = await _db.UserIdentitySubmits.AsNoTracking().CountAsync(x => x.Status == 0);
        var todayPaidOrders = await _db.PayOrders.AsNoTracking().CountAsync(x => x.Status == 1 && x.PaidAt != null && x.PaidAt >= todayStart && x.PaidAt < tomorrowStart);
        var newsTotal = await _db.NewsArticles.AsNoTracking().CountAsync();

        var todayEntryTotal = await _db.EventEntries.AsNoTracking().CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart);
        var todayIdentityTotal = await _db.UserIdentitySubmits.AsNoTracking().CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart);
        var todayCertTotal = await _db.Certificates.AsNoTracking().CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart);

        var todayEntryDone = await _db.EventEntries.AsNoTracking().CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart && x.Status != 0);
        var todayIdentityDone = await _db.UserIdentitySubmits.AsNoTracking().CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart && x.Status != 0);
        var todayCertInvalid = await _db.Certificates.AsNoTracking().CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart && x.Status == 2);

        var remindersEvents = await _db.Events.AsNoTracking()
            .Where(x => x.Status == 1 && x.SignupEndAt != null && x.SignupEndAt >= now && x.SignupEndAt <= soonEnd)
            .OrderBy(x => x.SignupEndAt)
            .Take(6)
            .Select(x => new
            {
                type = "event_signup_end",
                title = x.Title,
                refId = x.Id,
                at = x.SignupEndAt!.Value,
            })
            .ToListAsync();

        var remindersEntriesPay = await _db.EventEntries.AsNoTracking()
            .Where(x => x.Status == 2)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .Select(x => new
            {
                type = "entry_wait_pay",
                title = "报名待缴费",
                refId = x.Id,
                at = x.CreatedAt,
            })
            .ToListAsync();

        var remindersPayFailed = await _db.PayOrders.AsNoTracking()
            .Where(x => x.Status == 2)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .Select(x => new
            {
                type = "pay_closed",
                title = "订单已关闭",
                refId = x.Id,
                at = x.CreatedAt,
            })
            .ToListAsync();

        var latestEntries = await (
                from e in _db.EventEntries.AsNoTracking()
                join ev0 in _db.Events.AsNoTracking() on e.EventId equals ev0.Id into ev1
                from ev in ev1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on e.AthletePersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                orderby e.CreatedAt descending
                select new
                {
                    id = e.Id,
                    eventTitle = ev != null ? ev.Title : null,
                    athleteName = p != null ? p.FullName : null,
                    status = e.Status,
                    createdAt = e.CreatedAt,
                })
            .Take(8)
            .ToListAsync();

        var latestOrders = await _db.PayOrders.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .Select(x => new
            {
                id = x.Id,
                bizType = x.BizType,
                amount = x.Amount,
                status = x.Status,
                createdAt = x.CreatedAt,
            })
            .ToListAsync();

        var reminders = remindersEvents
            .Concat(remindersEntriesPay)
            .Concat(remindersPayFailed)
            .OrderBy(x => x.at)
            .Take(10)
            .ToList();

        return ApiResponse<object>.Ok(new
        {
            kpis = new
            {
                pendingEntries,
                pendingIdentities,
                todayPaidOrders,
                newsTotal,
            },
            progress = new
            {
                entry = new { done = todayEntryDone, total = todayEntryTotal },
                identity = new { done = todayIdentityDone, total = todayIdentityTotal },
                cert = new { done = Math.Max(0, todayCertTotal - todayCertInvalid), total = todayCertTotal },
            },
            reminders,
            latest = new
            {
                entries = latestEntries,
                orders = latestOrders,
            }
        });
    }
}


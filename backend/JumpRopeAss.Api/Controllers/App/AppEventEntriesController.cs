using System.Security.Claims;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/events")]
public sealed class AppEventEntriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AppEventEntriesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public sealed record EnrollAthlete(string FullName, string? Mobile, sbyte? Gender, string? IdCardNo, string? Birthday);
    public sealed record EnrollRequest(ulong GroupId, sbyte EnrollChannel, EnrollAthlete Athlete);

    [Authorize]
    [HttpGet("{id:long}/my-entry")]
    public async Task<ApiResponse<object>> MyEntry([FromRoute] long id)
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var eventId = (ulong)id;
        var item = await (
                from e in _db.EventEntries.AsNoTracking()
                where e.EventId == eventId && e.EnrollUserId == userId
                join g0 in _db.EventGroups.AsNoTracking() on e.GroupId equals g0.Id into g1
                from g in g1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on e.AthletePersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                orderby e.CreatedAt descending
                select new
                {
                    id = e.Id,
                    eventId = e.EventId,
                    groupId = e.GroupId,
                    groupName = g != null ? g.Name : null,
                    feeAmount = g != null ? g.FeeAmount : 0,
                    athletePersonId = e.AthletePersonId,
                    athleteName = p != null ? p.FullName : null,
                    athleteMobile = p != null ? p.Mobile : null,
                    enrollChannel = e.EnrollChannel,
                    status = e.Status,
                    auditRemark = e.AuditRemark,
                    payOrderId = e.PayOrderId,
                    createdAt = e.CreatedAt,
                })
            .FirstOrDefaultAsync();

        return ApiResponse<object>.Ok(new { item });
    }

    [Authorize]
    [HttpPost("{id:long}/entries")]
    public async Task<ApiResponse<object>> Enroll([FromRoute] long id, [FromBody] EnrollRequest req)
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var eventId = (ulong)id;
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId);
        if (ev is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");

        var group = await _db.EventGroups.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.GroupId && x.EventId == eventId);
        if (group is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "组别不存在");

        var athleteName = (req.Athlete.FullName ?? string.Empty).Trim();
        if (athleteName.Length == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请填写运动员姓名");

        var enrollChannel = req.EnrollChannel;
        if (enrollChannel is < 1 or > 3) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "报名方式非法");

        var athleteMobile = string.IsNullOrWhiteSpace(req.Athlete.Mobile) ? null : req.Athlete.Mobile!.Trim();
        var athleteIdCard = string.IsNullOrWhiteSpace(req.Athlete.IdCardNo) ? null : req.Athlete.IdCardNo!.Trim();
        var athleteBirthday = string.IsNullOrWhiteSpace(req.Athlete.Birthday) ? null : req.Athlete.Birthday!.Trim();
        DateOnly? birthday = null;
        if (!string.IsNullOrWhiteSpace(athleteBirthday) && DateOnly.TryParse(athleteBirthday, out var b))
            birthday = b;

        if (string.IsNullOrWhiteSpace(athleteMobile))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请填写手机号");

        var exists = await _db.EventEntries.AsNoTracking().AnyAsync(x =>
            x.EventId == eventId &&
            x.GroupId == req.GroupId &&
            x.EnrollUserId == userId &&
            x.Status != 6 &&
            x.Status != 9);
        if (exists) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "已报名该组别，请勿重复提交");

        var now = DateTime.UtcNow;

        var identityOk = await _db.UserIdentitySubmits.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Status)
            .FirstOrDefaultAsync();
        if (!_env.IsDevelopment() && identityOk != 1)
            return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "请先完成实名认证");

        Person person;
        if (!string.IsNullOrWhiteSpace(athleteMobile))
        {
            person = await _db.People.FirstOrDefaultAsync(x => x.DeletedAt == null && x.Mobile == athleteMobile && x.FullName == athleteName)
                ?? new Person { FullName = athleteName, Mobile = athleteMobile, Gender = req.Athlete.Gender, IdCardNo = athleteIdCard, Birthday = birthday, Status = 1, CreatedAt = now, UpdatedAt = now };
        }
        else
        {
            person = new Person { FullName = athleteName, Mobile = null, Gender = req.Athlete.Gender, IdCardNo = athleteIdCard, Birthday = birthday, Status = 1, CreatedAt = now, UpdatedAt = now };
        }

        if (person.Id == 0)
        {
            _db.People.Add(person);
        }
        else
        {
            if (req.Athlete.Gender is not null) person.Gender = req.Athlete.Gender;
            if (!string.IsNullOrWhiteSpace(athleteIdCard)) person.IdCardNo = athleteIdCard;
            if (birthday is not null) person.Birthday = birthday;
            person.UpdatedAt = now;
        }
        await _db.SaveChangesAsync();

        var entry = new EventEntry
        {
            EventId = eventId,
            GroupId = req.GroupId,
            AthletePersonId = person.Id,
            EnrollChannel = enrollChannel,
            EnrollUserId = userId,
            Status = (sbyte)(
                (_env.IsDevelopment() || ev.NeedAudit == 0)
                    ? (ev.NeedPay == 1 ? 2 : 5)
                    : 0
            ),
            AuditRemark = (_env.IsDevelopment() || ev.NeedAudit == 0) ? "自动审核通过" : null,
            PayOrderId = null,
            CreatedAt = now,
        };

        _db.EventEntries.Add(entry);
        await _db.SaveChangesAsync();

        if (entry.Status == 2 && ev.NeedPay == 1)
        {
            var outTradeNo = await NewOutTradeNo();
            var order = new PayOrder
            {
                BizType = 1,
                BizId = entry.Id,
                UserId = userId,
                Amount = group.FeeAmount,
                Status = 0,
                WxOutTradeNo = outTradeNo,
                PaidAt = null,
                CreatedAt = now,
            };
            _db.PayOrders.Add(order);
            await _db.SaveChangesAsync();

            entry.PayOrderId = order.Id;
            entry.Status = 3;
            await _db.SaveChangesAsync();

            return ApiResponse<object>.Ok(new
            {
                entry = new { id = entry.Id, status = entry.Status, groupId = entry.GroupId, payOrderId = entry.PayOrderId },
                payOrder = new { id = order.Id, amount = order.Amount, status = order.Status, wxOutTradeNo = order.WxOutTradeNo },
            });
        }

        return ApiResponse<object>.Ok(new { entry = new { id = entry.Id, status = entry.Status, groupId = entry.GroupId } });
    }

    private async Task<string> NewOutTradeNo()
    {
        for (var i = 0; i < 5; i++)
        {
            var s = $"DEV{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100000, 999999)}";
            var exists = await _db.PayOrders.AsNoTracking().AnyAsync(x => x.WxOutTradeNo == s);
            if (!exists) return s;
        }
        return $"DEV{Guid.NewGuid():N}";
    }

    private ulong GetUserIdOrZero()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return ulong.TryParse(sub, out var v) ? v : 0UL;
    }
}

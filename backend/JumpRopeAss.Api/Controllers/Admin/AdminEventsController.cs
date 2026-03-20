using System.Text.Json;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/events")]
[Authorize]
public class AdminEventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminEventsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 50);

        var query = _db.Events.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.Title.Contains(keyword));

        query = query.OrderByDescending(x => x.Id);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size)
            .Take(size)
            .Select(x => new
            {
                x.Id,
                x.EventType,
                x.SignupScope,
                x.LimitOrgId,
                x.Title,
                x.CoverUrl,
                x.LogoUrl,
                x.BannerUrl,
                x.Slogan,
                x.HostOrg,
                x.CoOrgs,
                x.Contacts,
                x.Projects,
                x.NeedAudit,
                x.NeedPay,
                x.SignupStartAt,
                x.SignupEndAt,
                x.EventDate,
                x.EventStartAt,
                x.EventEndAt,
                x.Location,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<object>> GetDetail(long id)
    {
        var item = await _db.Events.AsNoTracking()
            .Where(x => x.Id == (ulong)id)
            .Select(x => new
            {
                x.Id,
                x.EventType,
                x.SignupScope,
                x.LimitOrgId,
                x.Title,
                x.CoverUrl,
                x.LogoUrl,
                x.BannerUrl,
                x.Slogan,
                x.HostOrg,
                x.CoOrgs,
                x.Contacts,
                x.Projects,
                x.NeedAudit,
                x.NeedPay,
                x.SignupStartAt,
                x.SignupEndAt,
                x.EventDate,
                x.EventStartAt,
                x.EventEndAt,
                x.Location,
                x.DescriptionHtml,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");
        return ApiResponse<object>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<object>> Create([FromBody] AdminEventDto dto)
    {
        if (!DataUrlImage.IsValid(dto.CoverUrl) || !DataUrlImage.IsValid(dto.LogoUrl) || !DataUrlImage.IsValid(dto.BannerUrl))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "图片数据无效，请重新上传");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请输入活动名称");

        var ev = new Event
        {
            EventType = dto.EventType,
            SignupScope = dto.SignupScope ?? 1,
            LimitOrgId = dto.LimitOrgId,
            Title = dto.Title.Trim(),
            CoverUrl = string.IsNullOrWhiteSpace(dto.CoverUrl) ? null : dto.CoverUrl,
            LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl,
            BannerUrl = string.IsNullOrWhiteSpace(dto.BannerUrl) ? null : dto.BannerUrl,
            Slogan = dto.Slogan,
            HostOrg = dto.HostOrg,
            CoOrgs = dto.CoOrgs != null ? JsonSerializer.Serialize(dto.CoOrgs) : null,
            Contacts = dto.Contacts != null ? JsonSerializer.Serialize(dto.Contacts) : null,
            Projects = dto.Projects != null ? JsonSerializer.Serialize(dto.Projects) : null,
            NeedAudit = dto.NeedAudit ?? 1,
            NeedPay = dto.NeedPay ?? 1,
            Location = dto.Location,
            SignupStartAt = dto.SignupStartAt,
            SignupEndAt = dto.SignupEndAt,
            EventDate = dto.EventDate ?? (dto.EventStartAt.HasValue ? DateOnly.FromDateTime(dto.EventStartAt.Value) : null),
            EventStartAt = dto.EventStartAt,
            EventEndAt = dto.EventEndAt,
            DescriptionHtml = dto.DescriptionHtml,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id = ev.Id });
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<object>> Update(long id, [FromBody] AdminEventDto dto)
    {
        var ev = await _db.Events.FindAsync((ulong)id);
        if (ev == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");
        if (!DataUrlImage.IsValid(dto.CoverUrl) || !DataUrlImage.IsValid(dto.LogoUrl) || !DataUrlImage.IsValid(dto.BannerUrl))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "图片数据无效，请重新上传");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请输入活动名称");

        ev.EventType = dto.EventType;
        ev.SignupScope = dto.SignupScope ?? ev.SignupScope;
        ev.LimitOrgId = dto.LimitOrgId;
        ev.Title = dto.Title.Trim();
        if (dto.CoverUrl != null) ev.CoverUrl = string.IsNullOrWhiteSpace(dto.CoverUrl) ? null : dto.CoverUrl;
        if (dto.LogoUrl != null) ev.LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl;
        if (dto.BannerUrl != null) ev.BannerUrl = string.IsNullOrWhiteSpace(dto.BannerUrl) ? null : dto.BannerUrl;
        ev.Slogan = dto.Slogan;
        ev.HostOrg = dto.HostOrg;
        ev.CoOrgs = dto.CoOrgs != null ? JsonSerializer.Serialize(dto.CoOrgs) : null;
        ev.Contacts = dto.Contacts != null ? JsonSerializer.Serialize(dto.Contacts) : null;
        ev.Projects = dto.Projects != null ? JsonSerializer.Serialize(dto.Projects) : null;
        ev.NeedAudit = dto.NeedAudit ?? ev.NeedAudit;
        ev.NeedPay = dto.NeedPay ?? ev.NeedPay;
        ev.Location = dto.Location;
        ev.SignupStartAt = dto.SignupStartAt;
        ev.SignupEndAt = dto.SignupEndAt;
        ev.EventDate = dto.EventDate ?? (dto.EventStartAt.HasValue ? DateOnly.FromDateTime(dto.EventStartAt.Value) : ev.EventDate);
        ev.EventStartAt = dto.EventStartAt;
        ev.EventEndAt = dto.EventEndAt;
        ev.DescriptionHtml = dto.DescriptionHtml;
        ev.Status = dto.Status ?? ev.Status;
        ev.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var ev = await _db.Events.FindAsync((ulong)id);
        if (ev == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class AdminEventContactDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
}

public class AdminEventDto
{
    public sbyte EventType { get; set; }
    public sbyte? SignupScope { get; set; }
    public ulong? LimitOrgId { get; set; }
    public string Title { get; set; } = null!;
    public string? CoverUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Slogan { get; set; }
    public string? HostOrg { get; set; }
    public List<string>? CoOrgs { get; set; }
    public List<AdminEventContactDto>? Contacts { get; set; }
    public List<string>? Projects { get; set; }
    public sbyte? NeedAudit { get; set; }
    public sbyte? NeedPay { get; set; }
    public string? Location { get; set; }
    public DateTime? SignupStartAt { get; set; }
    public DateTime? SignupEndAt { get; set; }
    public DateOnly? EventDate { get; set; }
    public DateTime? EventStartAt { get; set; }
    public DateTime? EventEndAt { get; set; }
    public string? DescriptionHtml { get; set; }
    public sbyte? Status { get; set; }
}

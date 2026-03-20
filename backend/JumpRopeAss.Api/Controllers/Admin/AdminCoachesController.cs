using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/coaches")]
[Authorize]
public class AdminCoachesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminCoachesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query =
            from c in _db.PersonCoaches
            join p in _db.People on c.PersonId equals p.Id
            join o in _db.Orgs on c.OrgId equals o.Id into og
            from o in og.DefaultIfEmpty()
            select new { c, p, o };

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(x => x.p.FullName.Contains(keyword) || x.p.Mobile.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.c.CreatedAt)
                               .Skip((page - 1) * size)
                               .Take(size)
                               .Select(x => new
                               {
                                   x.p.Id,
                                   x.p.FullName,
                                   x.p.Mobile,
                                   x.p.Gender,
                                   x.p.IdCardNo,
                                   x.p.AvatarUrl,
                                   x.c.OrgId,
                                   OrgName = x.o != null ? x.o.Name : null,
                                   OrgType = x.o != null ? (sbyte?)x.o.OrgType : null,
                                   x.c.CoachLevel,
                                   x.c.Status,
                                   x.c.CreatedAt
                               })
                               .ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<object>> GetDetail(long id)
    {
        var item = await (from c in _db.PersonCoaches
                          join p in _db.People on c.PersonId equals p.Id
                          join o in _db.Orgs on c.OrgId equals o.Id into og
                          from o in og.DefaultIfEmpty()
                          where c.PersonId == (ulong)id
                          select new
                          {
                              p.Id,
                              p.FullName,
                              p.Mobile,
                              p.Gender,
                              p.IdCardNo,
                              p.AvatarUrl,
                              p.Birthday,
                              c.OrgId,
                              OrgName = o != null ? o.Name : null,
                              OrgType = o != null ? (sbyte?)o.OrgType : null,
                              c.CoachLevel,
                              c.Status
                          }).FirstOrDefaultAsync();

        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "教练员不存在");
        return ApiResponse<object>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<object>> Create([FromBody] CoachDto dto)
    {
        var person = await _db.People.FirstOrDefaultAsync(x => x.IdCardNo == dto.IdCardNo);
        if (person == null)
        {
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl) && !DataUrlImage.IsValid(dto.AvatarUrl)) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "头像图片数据无效，请重新上传");
            person = new Person
            {
                FullName = dto.FullName,
                Mobile = dto.Mobile,
                Gender = dto.Gender ?? 0,
                IdCardNo = dto.IdCardNo,
                AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = 1
            };
            _db.People.Add(person);
            await _db.SaveChangesAsync();
        }
        else if (dto.AvatarUrl != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl) && !DataUrlImage.IsValid(dto.AvatarUrl)) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "头像图片数据无效，请重新上传");
            person.AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl;
            person.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        var coach = await _db.PersonCoaches.FindAsync(person.Id);
        if (coach != null)
        {
            return ApiResponse<object>.Fail(ErrorCodes.Conflict, "该人员已是教练");
        }

        coach = new PersonCoach
        {
            PersonId = person.Id,
            OrgId = dto.OrgId,
            CoachLevel = dto.CoachLevel,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now
        };
        _db.PersonCoaches.Add(coach);
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id = coach.PersonId });
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<object>> Update(long id, [FromBody] CoachDto dto)
    {
        var person = await _db.People.FindAsync((ulong)id);
        var coach = await _db.PersonCoaches.FindAsync((ulong)id);

        if (person == null || coach == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "教练员不存在");

        person.FullName = dto.FullName;
        person.Mobile = dto.Mobile;
        person.Gender = dto.Gender ?? person.Gender;
        if (!string.IsNullOrEmpty(dto.IdCardNo)) person.IdCardNo = dto.IdCardNo;
        if (dto.AvatarUrl != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl) && !DataUrlImage.IsValid(dto.AvatarUrl)) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "头像图片数据无效，请重新上传");
            person.AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl;
        }
        person.UpdatedAt = DateTime.Now;

        coach.OrgId = dto.OrgId;
        coach.CoachLevel = dto.CoachLevel;
        coach.Status = dto.Status ?? coach.Status;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var coach = await _db.PersonCoaches.FindAsync((ulong)id);
        if (coach == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "教练员不存在");

        _db.PersonCoaches.Remove(coach);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class CoachDto
{
    public string FullName { get; set; } = null!;
    public string? Mobile { get; set; }
    public sbyte? Gender { get; set; }
    public string? IdCardNo { get; set; }
    public string? AvatarUrl { get; set; }
    public ulong OrgId { get; set; }
    public string? CoachLevel { get; set; }
    public sbyte? Status { get; set; }
}

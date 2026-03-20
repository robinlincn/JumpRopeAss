using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/judges")]
[Authorize]
public class AdminJudgesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminJudgesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query =
            from j in _db.PersonJudges
            join p in _db.People on j.PersonId equals p.Id
            join o in _db.Orgs on j.OrgId equals o.Id into og
            from o in og.DefaultIfEmpty()
            select new { j, p, o };

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(x => x.p.FullName.Contains(keyword) || x.p.Mobile.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.j.CreatedAt)
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
                                   x.j.OrgId,
                                   OrgName = x.o != null ? x.o.Name : null,
                                   OrgType = x.o != null ? (sbyte?)x.o.OrgType : null,
                                   x.j.JudgeLevel,
                                   x.j.Status,
                                   x.j.CreatedAt
                               })
                               .ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<object>> GetDetail(long id)
    {
        var item = await (from j in _db.PersonJudges
                          join p in _db.People on j.PersonId equals p.Id
                          join o in _db.Orgs on j.OrgId equals o.Id into og
                          from o in og.DefaultIfEmpty()
                          where j.PersonId == (ulong)id
                          select new
                          {
                              p.Id,
                              p.FullName,
                              p.Mobile,
                              p.Gender,
                              p.IdCardNo,
                              p.AvatarUrl,
                              p.Birthday,
                              j.OrgId,
                              OrgName = o != null ? o.Name : null,
                              OrgType = o != null ? (sbyte?)o.OrgType : null,
                              j.JudgeLevel,
                              j.Status
                          }).FirstOrDefaultAsync();

        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "裁判员不存在");
        return ApiResponse<object>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<object>> Create([FromBody] JudgeDto dto)
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

        // Check if judge role exists
        var judge = await _db.PersonJudges.FindAsync(person.Id);
        if (judge != null)
        {
            return ApiResponse<object>.Fail(ErrorCodes.Conflict, "该人员已是裁判");
        }

        judge = new PersonJudge
        {
            PersonId = person.Id,
            OrgId = dto.OrgId,
            JudgeLevel = dto.JudgeLevel,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now
        };
        _db.PersonJudges.Add(judge);
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id = judge.PersonId });
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<object>> Update(long id, [FromBody] JudgeDto dto)
    {
        var person = await _db.People.FindAsync((ulong)id);
        var judge = await _db.PersonJudges.FindAsync((ulong)id);

        if (person == null || judge == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "裁判员不存在");

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

        judge.OrgId = dto.OrgId;
        judge.JudgeLevel = dto.JudgeLevel;
        judge.Status = dto.Status ?? judge.Status;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var judge = await _db.PersonJudges.FindAsync((ulong)id);
        if (judge == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "裁判员不存在");

        _db.PersonJudges.Remove(judge);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class JudgeDto
{
    public string FullName { get; set; } = null!;
    public string? Mobile { get; set; }
    public sbyte? Gender { get; set; }
    public string? IdCardNo { get; set; }
    public string? AvatarUrl { get; set; }
    public ulong OrgId { get; set; }
    public string? JudgeLevel { get; set; }
    public sbyte? Status { get; set; }
}

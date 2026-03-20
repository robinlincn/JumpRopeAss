using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/athletes")]
[Authorize]
public class AdminAthletesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminAthletesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query =
            from a in _db.PersonAthletes
            join p in _db.People on a.PersonId equals p.Id
            join s in _db.Schools on a.SchoolId equals s.Id into sg
            from s in sg.DefaultIfEmpty()
            join o in _db.Orgs on a.TrainingOrgId equals o.Id into og
            from o in og.DefaultIfEmpty()
            join c in _db.People on a.FirstCoachPersonId equals c.Id into cg
            from c in cg.DefaultIfEmpty()
            select new { a, p, s, o, c };

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(x => x.p.FullName.Contains(keyword) || x.p.Mobile.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.a.CreatedAt)
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
                                   x.a.SchoolId,
                                   SchoolName = x.s != null ? x.s.Name : null,
                                   x.a.TrainingOrgId,
                                   TrainingOrgName = x.o != null ? x.o.Name : null,
                                   TrainingOrgType = x.o != null ? (sbyte?)x.o.OrgType : null,
                                   x.a.FirstCoachPersonId,
                                   FirstCoachName = x.c != null ? x.c.FullName : null,
                                   x.a.GradeName,
                                   x.a.ClassName,
                                   x.a.Status,
                                   x.a.CreatedAt
                               })
                               .ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<object>> GetDetail(long id)
    {
        var item = await (from a in _db.PersonAthletes
                          join p in _db.People on a.PersonId equals p.Id
                          join s in _db.Schools on a.SchoolId equals s.Id into sg
                          from s in sg.DefaultIfEmpty()
                          join o in _db.Orgs on a.TrainingOrgId equals o.Id into og
                          from o in og.DefaultIfEmpty()
                          join c in _db.People on a.FirstCoachPersonId equals c.Id into cg
                          from c in cg.DefaultIfEmpty()
                          where a.PersonId == (ulong)id
                          select new
                          {
                              p.Id,
                              p.FullName,
                              p.Mobile,
                              p.Gender,
                              p.IdCardNo,
                              p.AvatarUrl,
                              p.Birthday,
                              a.SchoolId,
                              SchoolName = s != null ? s.Name : null,
                              a.TrainingOrgId,
                              TrainingOrgName = o != null ? o.Name : null,
                              TrainingOrgType = o != null ? (sbyte?)o.OrgType : null,
                              a.FirstCoachPersonId,
                              FirstCoachName = c != null ? c.FullName : null,
                              a.GradeName,
                              a.ClassName,
                              a.Status
                          }).FirstOrDefaultAsync();

        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "运动员不存在");
        return ApiResponse<object>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<object>> Create([FromBody] AthleteDto dto)
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

        var athlete = await _db.PersonAthletes.FindAsync(person.Id);
        if (athlete != null)
        {
            return ApiResponse<object>.Fail(ErrorCodes.Conflict, "该人员已是运动员");
        }

        athlete = new PersonAthlete
        {
            PersonId = person.Id,
            SchoolId = dto.SchoolId,
            TrainingOrgId = dto.TrainingOrgId,
            FirstCoachPersonId = dto.FirstCoachPersonId,
            GradeName = dto.GradeName,
            ClassName = dto.ClassName,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now
        };
        _db.PersonAthletes.Add(athlete);
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id = athlete.PersonId });
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<object>> Update(long id, [FromBody] AthleteDto dto)
    {
        var person = await _db.People.FindAsync((ulong)id);
        var athlete = await _db.PersonAthletes.FindAsync((ulong)id);

        if (person == null || athlete == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "运动员不存在");

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

        athlete.SchoolId = dto.SchoolId;
        athlete.TrainingOrgId = dto.TrainingOrgId;
        athlete.FirstCoachPersonId = dto.FirstCoachPersonId;
        athlete.GradeName = dto.GradeName;
        athlete.ClassName = dto.ClassName;
        athlete.Status = dto.Status ?? athlete.Status;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var athlete = await _db.PersonAthletes.FindAsync((ulong)id);
        if (athlete == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "运动员不存在");

        _db.PersonAthletes.Remove(athlete);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class AthleteDto
{
    public string FullName { get; set; } = null!;
    public string? Mobile { get; set; }
    public sbyte? Gender { get; set; }
    public string? IdCardNo { get; set; }
    public string? AvatarUrl { get; set; }
    public ulong SchoolId { get; set; }
    public ulong? TrainingOrgId { get; set; }
    public ulong? FirstCoachPersonId { get; set; }
    public string? GradeName { get; set; }
    public string? ClassName { get; set; }
    public sbyte? Status { get; set; }
}

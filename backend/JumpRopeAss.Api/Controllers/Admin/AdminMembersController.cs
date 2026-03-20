using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/members")]
[Authorize]
public class AdminMembersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminMembersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _db.MemberUnits.OrderBy(x => x.SortNo);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<MemberUnit>> GetDetail(long id)
    {
        var item = await _db.MemberUnits.FindAsync((ulong)id);
        if (item == null) return ApiResponse<MemberUnit>.Fail(ErrorCodes.NotFound, "会员单位不存在");
        return ApiResponse<MemberUnit>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<MemberUnit>> Create([FromBody] MemberUnitDto dto)
    {
        if (!DataUrlImage.IsValid(dto.LogoUrl)) return ApiResponse<MemberUnit>.Fail(ErrorCodes.InvalidParam, "Logo图片数据无效，请重新上传");
        var item = new MemberUnit
        {
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            Address = dto.Address,
            Intro = dto.Intro,
            ContentHtml = dto.ContentHtml,
            SortNo = dto.SortNo,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now
        };
        _db.MemberUnits.Add(item);
        await _db.SaveChangesAsync();
        return ApiResponse<MemberUnit>.Ok(item);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<MemberUnit>> Update(long id, [FromBody] MemberUnitDto dto)
    {
        var item = await _db.MemberUnits.FindAsync((ulong)id);
        if (item == null) return ApiResponse<MemberUnit>.Fail(ErrorCodes.NotFound, "会员单位不存在");
        if (!DataUrlImage.IsValid(dto.LogoUrl)) return ApiResponse<MemberUnit>.Fail(ErrorCodes.InvalidParam, "Logo图片数据无效，请重新上传");

        item.Name = dto.Name;
        item.LogoUrl = dto.LogoUrl;
        item.ContactName = dto.ContactName;
        item.ContactPhone = dto.ContactPhone;
        item.Address = dto.Address;
        item.Intro = dto.Intro;
        item.ContentHtml = dto.ContentHtml;
        item.SortNo = dto.SortNo;
        item.Status = dto.Status ?? 1;

        await _db.SaveChangesAsync();
        return ApiResponse<MemberUnit>.Ok(item);
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var item = await _db.MemberUnits.FindAsync((ulong)id);
        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "会员单位不存在");

        _db.MemberUnits.Remove(item);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class MemberUnitDto
{
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Intro { get; set; }
    public string? ContentHtml { get; set; }
    public int SortNo { get; set; }
    public sbyte? Status { get; set; }
}

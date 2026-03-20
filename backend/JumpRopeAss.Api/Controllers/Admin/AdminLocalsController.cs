using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/locals")]
[Authorize]
public class AdminLocalsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminLocalsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _db.LocalAssociations.OrderBy(x => x.SortNo);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<LocalAssociation>> GetDetail(long id)
    {
        var item = await _db.LocalAssociations.FindAsync((ulong)id);
        if (item == null) return ApiResponse<LocalAssociation>.Fail(ErrorCodes.NotFound, "地方协会不存在");
        return ApiResponse<LocalAssociation>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<LocalAssociation>> Create([FromBody] LocalAssocDto dto)
    {
        if (!DataUrlImage.IsValid(dto.LogoUrl)) return ApiResponse<LocalAssociation>.Fail(ErrorCodes.InvalidParam, "Logo图片数据无效，请重新上传");
        var item = new LocalAssociation
        {
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            Intro = dto.Intro,
            ContentHtml = dto.ContentHtml,
            SortNo = dto.SortNo,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now
        };
        _db.LocalAssociations.Add(item);
        await _db.SaveChangesAsync();
        return ApiResponse<LocalAssociation>.Ok(item);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<LocalAssociation>> Update(long id, [FromBody] LocalAssocDto dto)
    {
        var item = await _db.LocalAssociations.FindAsync((ulong)id);
        if (item == null) return ApiResponse<LocalAssociation>.Fail(ErrorCodes.NotFound, "地方协会不存在");
        if (!DataUrlImage.IsValid(dto.LogoUrl)) return ApiResponse<LocalAssociation>.Fail(ErrorCodes.InvalidParam, "Logo图片数据无效，请重新上传");

        item.Name = dto.Name;
        item.LogoUrl = dto.LogoUrl;
        item.ContactName = dto.ContactName;
        item.ContactPhone = dto.ContactPhone;
        item.Intro = dto.Intro;
        item.ContentHtml = dto.ContentHtml;
        item.SortNo = dto.SortNo;
        item.Status = dto.Status ?? 1;

        await _db.SaveChangesAsync();
        return ApiResponse<LocalAssociation>.Ok(item);
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var item = await _db.LocalAssociations.FindAsync((ulong)id);
        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "地方协会不存在");

        _db.LocalAssociations.Remove(item);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class LocalAssocDto
{
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Intro { get; set; }
    public string? ContentHtml { get; set; }
    public int SortNo { get; set; }
    public sbyte? Status { get; set; }
}

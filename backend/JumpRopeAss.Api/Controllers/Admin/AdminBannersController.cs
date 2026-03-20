using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/banners")]
[Authorize]
public class AdminBannersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminBannersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<List<Banner>>> GetList()
    {
        var items = await _db.Banners.OrderBy(x => x.SortNo).ToListAsync();
        return ApiResponse<List<Banner>>.Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<Banner>> GetDetail(long id)
    {
        var item = await _db.Banners.FindAsync((ulong)id);
        if (item == null) return ApiResponse<Banner>.Fail(ErrorCodes.NotFound, "Banner不存在");
        return ApiResponse<Banner>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<Banner>> Create([FromBody] BannerDto dto)
    {
        if (!DataUrlImage.IsValid(dto.ImageUrl)) return ApiResponse<Banner>.Fail(ErrorCodes.InvalidParam, "图片数据无效，请重新上传");
        var banner = new Banner
        {
            Title = dto.Title,
            Position = dto.Position,
            MediaType = dto.MediaType,
            ImageUrl = dto.ImageUrl,
            VideoUrl = dto.VideoUrl,
            SortNo = dto.SortNo,
            Status = dto.Status,
            CreatedAt = DateTime.Now
        };
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
        return ApiResponse<Banner>.Ok(banner);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<Banner>> Update(long id, [FromBody] BannerDto dto)
    {
        var banner = await _db.Banners.FindAsync((ulong)id);
        if (banner == null) return ApiResponse<Banner>.Fail(ErrorCodes.NotFound, "Banner不存在");
        if (!DataUrlImage.IsValid(dto.ImageUrl)) return ApiResponse<Banner>.Fail(ErrorCodes.InvalidParam, "图片数据无效，请重新上传");

        banner.Title = dto.Title;
        banner.Position = dto.Position;
        banner.MediaType = dto.MediaType;
        banner.ImageUrl = dto.ImageUrl;
        banner.VideoUrl = dto.VideoUrl;
        banner.SortNo = dto.SortNo;
        banner.Status = dto.Status;

        await _db.SaveChangesAsync();
        return ApiResponse<Banner>.Ok(banner);
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var banner = await _db.Banners.FindAsync((ulong)id);
        if (banner == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "Banner不存在");

        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class BannerDto
{
    public string? Title { get; set; }
    public string Position { get; set; } = null!;
    public string? MediaType { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public int SortNo { get; set; }
    public sbyte Status { get; set; }
}

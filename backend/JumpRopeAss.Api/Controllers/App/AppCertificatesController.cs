using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/certificates")]
public sealed class AppCertificatesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppCertificatesController(AppDbContext db) => _db = db;

    public sealed record SearchRequest(string? CertNo, string? Mobile, string? IdCardNo);

    private static string MaskMobile(string raw)
    {
        var s = (raw ?? string.Empty).Trim();
        if (s.Length <= 7) return s;
        return s[..3] + new string('*', s.Length - 7) + s[^4..];
    }

    private static string MaskIdCard(string raw)
    {
        var s = (raw ?? string.Empty).Trim();
        if (s.Length <= 8) return s;
        return s[..4] + new string('*', s.Length - 8) + s[^4..];
    }

    [HttpGet("by-no/{certNo}")]
    public async Task<ApiResponse<object>> GetByNo([FromRoute] string certNo)
    {
        var no = (certNo ?? string.Empty).Trim();
        if (no.Length == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "证书编号不能为空");

        var item = await (
                from c in _db.Certificates.AsNoTracking()
                join t0 in _db.CertTypes.AsNoTracking() on c.CertTypeId equals t0.Id into t1
                from t in t1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on c.HolderPersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                where c.CertNo == no
                select new
                {
                    certNo = c.CertNo,
                    certTypeName = t != null ? t.Name : null,
                    holderName = p != null ? p.FullName : null,
                    holderMobileMasked = p != null && p.Mobile != null ? MaskMobile(p.Mobile) : null,
                    holderIdCardNoMasked = p != null && p.IdCardNo != null ? MaskIdCard(p.IdCardNo) : null,
                    issueScene = c.IssueScene,
                    issueAt = c.IssueAt,
                    status = c.Status,
                    fileUrl = c.FileUrl,
                })
            .FirstOrDefaultAsync();

        if (item is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "证书不存在");
        return ApiResponse<object>.Ok(new { item });
    }

    [HttpPost("search")]
    public async Task<ApiResponse<object>> Search([FromBody] SearchRequest req)
    {
        var certNo = (req.CertNo ?? string.Empty).Trim();
        var mobile = (req.Mobile ?? string.Empty).Trim();
        var idCardNo = (req.IdCardNo ?? string.Empty).Trim();

        var filled = 0;
        if (certNo.Length > 0) filled++;
        if (mobile.Length > 0) filled++;
        if (idCardNo.Length > 0) filled++;
        if (filled != 1) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请只填写手机号/身份证号/证书编号中的任意一个");

        if (certNo.Length > 0)
        {
            var one = await (
                    from c in _db.Certificates.AsNoTracking()
                    join t0 in _db.CertTypes.AsNoTracking() on c.CertTypeId equals t0.Id into t1
                    from t in t1.DefaultIfEmpty()
                    join p0 in _db.People.AsNoTracking() on c.HolderPersonId equals p0.Id into p1
                    from p in p1.DefaultIfEmpty()
                    where c.CertNo == certNo
                    select new
                    {
                        certNo = c.CertNo,
                        certTypeName = t != null ? t.Name : null,
                        holderName = p != null ? p.FullName : null,
                        holderMobileMasked = p != null && p.Mobile != null ? MaskMobile(p.Mobile) : null,
                        holderIdCardNoMasked = p != null && p.IdCardNo != null ? MaskIdCard(p.IdCardNo) : null,
                        issueScene = c.IssueScene,
                        issueAt = c.IssueAt,
                        status = c.Status,
                        fileUrl = c.FileUrl,
                    })
                .FirstOrDefaultAsync();

            return ApiResponse<object>.Ok(new { items = one is null ? Array.Empty<object>() : new[] { one } });
        }

        var people = _db.People.AsNoTracking().Where(x => x.DeletedAt == null);
        if (idCardNo.Length > 0) people = people.Where(x => x.IdCardNo == idCardNo);
        if (mobile.Length > 0) people = people.Where(x => x.Mobile == mobile);

        var personIds = await people.Select(x => x.Id).Take(2).ToListAsync();
        if (personIds.Count == 0) return ApiResponse<object>.Ok(new { items = Array.Empty<object>() });
        if (personIds.Count > 1) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "该号码匹配到多个人员，请改用身份证号或证书编号查询");

        var pid = personIds[0];
        var items = await (
                from c in _db.Certificates.AsNoTracking()
                join t0 in _db.CertTypes.AsNoTracking() on c.CertTypeId equals t0.Id into t1
                from t in t1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on c.HolderPersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                where c.HolderPersonId == pid
                orderby c.IssueAt descending
                select new
                {
                    certNo = c.CertNo,
                    certTypeName = t != null ? t.Name : null,
                    holderName = p != null ? p.FullName : null,
                    holderMobileMasked = p != null && p.Mobile != null ? MaskMobile(p.Mobile) : null,
                    holderIdCardNoMasked = p != null && p.IdCardNo != null ? MaskIdCard(p.IdCardNo) : null,
                    issueScene = c.IssueScene,
                    issueAt = c.IssueAt,
                    status = c.Status,
                    fileUrl = c.FileUrl,
                })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { items });
    }
}

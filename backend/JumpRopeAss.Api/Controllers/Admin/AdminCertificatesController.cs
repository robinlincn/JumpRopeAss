using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public sealed class AdminCertificatesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminCertificatesController(AppDbContext db) => _db = db;

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
        var head = s[..4];
        var tail = s[^4..];
        return head + new string('*', s.Length - 8) + tail;
    }

    private static bool TryParseEntryCertNo(string certNo, out ulong eventId, out ulong entryId)
    {
        eventId = 0;
        entryId = 0;
        var s = (certNo ?? string.Empty).Trim();
        if (!s.StartsWith("CERT-E", StringComparison.Ordinal)) return false;
        var idx = s.IndexOf("-EN", StringComparison.Ordinal);
        if (idx <= 6) return false;
        var eventPart = s.Substring(6, idx - 6);
        var entryPart = s[(idx + 3)..];
        return ulong.TryParse(eventPart, out eventId) && ulong.TryParse(entryPart, out entryId);
    }

    [HttpGet("cert-types")]
    public async Task<ApiResponse<object>> ListCertTypes()
    {
        var items = await _db.CertTypes.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new { id = x.Id, code = x.Code, name = x.Name, status = x.Status })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { items });
    }

    public sealed record ImportRow(
        string CertTypeCode,
        string HolderName,
        string Mobile,
        string? IdCardNo,
        string? IssueAt,
        string? CertNo);

    public sealed record ImportCertificatesRequest(List<ImportRow> Items);

    [HttpPost("certificates/import")]
    public async Task<ApiResponse<object>> ImportCertificates([FromBody] ImportCertificatesRequest req)
    {
        if (req.Items is null || req.Items.Count == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "导入数据为空");
        if (req.Items.Count > 2000) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "单次导入最多2000条");

        var typeCodes = req.Items.Select(x => (x.CertTypeCode ?? string.Empty).Trim()).Where(x => x.Length > 0).Distinct().ToList();
        var typeMap = await _db.CertTypes.AsNoTracking()
            .Where(x => typeCodes.Contains(x.Code) && x.Status == 1)
            .Select(x => new { x.Id, x.Code })
            .ToDictionaryAsync(x => x.Code, x => x.Id);

        var now = DateTime.UtcNow;
        var created = 0;
        var skipped = 0;
        var errors = new List<string>();

        await using var tx = await _db.Database.BeginTransactionAsync();

        for (var i = 0; i < req.Items.Count; i++)
        {
            var r = req.Items[i];
            var typeCode = (r.CertTypeCode ?? string.Empty).Trim();
            if (!typeMap.TryGetValue(typeCode, out var certTypeId))
            {
                errors.Add($"第{i + 2}行：证书类型未启用或不存在（{typeCode}）");
                continue;
            }

            var holderName = (r.HolderName ?? string.Empty).Trim();
            var mobile = (r.Mobile ?? string.Empty).Trim();
            if (holderName.Length == 0 || mobile.Length == 0)
            {
                errors.Add($"第{i + 2}行：姓名/手机号必填");
                continue;
            }

            var idCard = string.IsNullOrWhiteSpace(r.IdCardNo) ? null : r.IdCardNo!.Trim();

            DateTime issueAt = now;
            if (!string.IsNullOrWhiteSpace(r.IssueAt) && DateTime.TryParse(r.IssueAt!.Trim(), out var dt))
                issueAt = dt;

            Person? person = null;
            if (!string.IsNullOrWhiteSpace(idCard))
            {
                person = await _db.People.FirstOrDefaultAsync(x => x.DeletedAt == null && x.IdCardNo == idCard);
            }
            if (person is null)
            {
                person = await _db.People.FirstOrDefaultAsync(x => x.DeletedAt == null && x.Mobile == mobile && x.FullName == holderName);
            }
            if (person is null)
            {
                person = new Person
                {
                    FullName = holderName,
                    Mobile = mobile,
                    Gender = 0,
                    IdCardNo = idCard,
                    Birthday = null,
                    Status = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    DeletedAt = null,
                };
                _db.People.Add(person);
                await _db.SaveChangesAsync();
            }
            else
            {
                person.FullName = holderName;
                person.Mobile = mobile;
                if (!string.IsNullOrWhiteSpace(idCard)) person.IdCardNo = idCard;
                person.UpdatedAt = now;
                await _db.SaveChangesAsync();
            }

            var certNo = string.IsNullOrWhiteSpace(r.CertNo) ? await NewCertNo(typeCode) : r.CertNo!.Trim();
            var existsNo = await _db.Certificates.AsNoTracking().AnyAsync(x => x.CertNo == certNo);
            if (existsNo)
            {
                skipped++;
                continue;
            }

            var cert = new Certificate
            {
                CertNo = certNo,
                CertTypeId = certTypeId,
                HolderPersonId = person.Id,
                IssueScene = 1,
                IssueAt = issueAt,
                Status = 1,
                FileUrl = null,
                CreatedAt = now,
            };
            _db.Certificates.Add(cert);
            created++;
        }

        if (created > 0) await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return ApiResponse<object>.Ok(new { created, skipped, errorCount = errors.Count, errors });
    }

    public sealed record ResetCertificatesRequest(List<ulong> Ids);

    [HttpPost("certificates/reset")]
    public async Task<ApiResponse<object>> ResetCertificates([FromBody] ResetCertificatesRequest req)
    {
        if (req.Ids is null || req.Ids.Count == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请选择证书");
        if (req.Ids.Count > 200) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "单次最多200条");

        var ids = req.Ids.Distinct().ToList();
        var certs = await _db.Certificates.Where(x => ids.Contains(x.Id)).ToListAsync();
        if (certs.Count == 0) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "证书不存在");

        var typeIds = certs.Select(x => x.CertTypeId).Distinct().ToList();
        var typeCodeMap = await _db.CertTypes.AsNoTracking()
            .Where(x => typeIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Code })
            .ToDictionaryAsync(x => x.Id, x => x.Code);

        var now = DateTime.UtcNow;
        var created = 0;
        var voided = 0;
        var mappings = new List<object>();

        await using var tx = await _db.Database.BeginTransactionAsync();

        foreach (var c in certs)
        {
            if (c.Status != 2)
            {
                c.Status = 2;
                c.FileUrl = null;
                voided++;
            }

            typeCodeMap.TryGetValue(c.CertTypeId, out var typeCode);
            typeCode ??= $"TYPE{c.CertTypeId}";

            var newNo = await NewCertNo(typeCode);
            var newCert = new Certificate
            {
                CertNo = newNo,
                CertTypeId = c.CertTypeId,
                HolderPersonId = c.HolderPersonId,
                IssueScene = 2,
                IssueAt = now,
                Status = 1,
                FileUrl = null,
                CreatedAt = now,
            };
            _db.Certificates.Add(newCert);
            created++;
            mappings.Add(new { oldId = c.Id, oldCertNo = c.CertNo, newCertNo = newNo });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return ApiResponse<object>.Ok(new { voided, created, mappings });
    }

    [HttpGet("certificates")]
    public async Task<ApiResponse<object>> ListCertificates(
        [FromQuery] ulong? certTypeId = null,
        [FromQuery] int? status = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "size")] int? size = null)
    {
        page = Math.Max(page, 1);
        if (size is not null) pageSize = size.Value;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Certificates.AsNoTracking().AsQueryable();
        if (certTypeId is not null) query = query.Where(x => x.CertTypeId == certTypeId);
        if (status is not null) query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            if (ulong.TryParse(kw, out var kwId))
            {
                query = query.Where(x => x.Id == kwId || x.HolderPersonId == kwId);
            }
            else
            {
                query = query.Where(x => EF.Functions.Like(x.CertNo, $"%{kw}%"));
            }
        }

        var total = await query.CountAsync();
        var raws = await (
                from c in query
                join t0 in _db.CertTypes.AsNoTracking() on c.CertTypeId equals t0.Id into t1
                from t in t1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on c.HolderPersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                select new
                {
                    id = c.Id,
                    certNo = c.CertNo,
                    certTypeId = c.CertTypeId,
                    certTypeCode = t != null ? t.Code : null,
                    certTypeName = t != null ? t.Name : null,
                    holderPersonId = c.HolderPersonId,
                    holderName = p != null ? p.FullName : null,
                    holderMobile = p != null ? p.Mobile : null,
                    holderMobileMasked = p != null && p.Mobile != null ? MaskMobile(p.Mobile) : null,
                    holderGender = p != null ? p.Gender : null,
                    holderIdCardNo = p != null ? p.IdCardNo : null,
                    holderIdCardNoMasked = p != null && p.IdCardNo != null ? MaskIdCard(p.IdCardNo) : null,
                    issueScene = c.IssueScene,
                    issueAt = c.IssueAt,
                    status = c.Status,
                    fileUrl = c.FileUrl,
                    createdAt = c.CreatedAt,
                })
            .OrderByDescending(x => x.createdAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var certNos = raws.Select(x => x.certNo).Distinct().ToList();
        var assessMap = await _db.AssessmentRecords.AsNoTracking()
            .Where(x => x.CertNo != null && certNos.Contains(x.CertNo))
            .Select(x => new
            {
                x.CertNo,
                x.GroupName,
                x.ProjectName,
                x.Level,
                x.ResultStatus,
                x.Score,
                x.AssociationName,
                x.Province,
                x.City,
                x.District,
            })
            .ToDictionaryAsync(x => x.CertNo!, x => x);

        var entryIds = new List<ulong>();
        foreach (var r in raws)
        {
            if (TryParseEntryCertNo(r.certNo, out _, out var entryId)) entryIds.Add(entryId);
        }
        entryIds = entryIds.Distinct().ToList();

        var entryMap = entryIds.Count == 0
            ? new Dictionary<ulong, object>()
            : (await (
                    from e in _db.EventEntries.AsNoTracking()
                    join ev0 in _db.Events.AsNoTracking() on e.EventId equals ev0.Id into ev1
                    from ev in ev1.DefaultIfEmpty()
                    join g0 in _db.EventGroups.AsNoTracking() on e.GroupId equals g0.Id into g1
                    from g in g1.DefaultIfEmpty()
                    join p0 in _db.People.AsNoTracking() on e.AthletePersonId equals p0.Id into p1
                    from p in p1.DefaultIfEmpty()
                    where entryIds.Contains(e.Id)
                    select new
                    {
                        entryId = e.Id,
                        eventId = e.EventId,
                        eventTitle = ev != null ? ev.Title : null,
                        groupName = g != null ? g.Name : null,
                        athleteName = p != null ? p.FullName : null,
                        athleteGender = p != null ? p.Gender : null,
                        athleteMobile = p != null ? p.Mobile : null,
                        athleteMobileMasked = p != null && p.Mobile != null ? MaskMobile(p.Mobile) : null,
                        athleteIdCardNo = p != null ? p.IdCardNo : null,
                        athleteIdCardNoMasked = p != null && p.IdCardNo != null ? MaskIdCard(p.IdCardNo) : null,
                    })
                .ToListAsync())
                .ToDictionary(x => x.entryId, x => (object)x);

        var items = raws.Select(r =>
        {
            assessMap.TryGetValue(r.certNo, out var ar);

            object? evr = null;
            if (TryParseEntryCertNo(r.certNo, out _, out var entryId))
                entryMap.TryGetValue(entryId, out evr);

            dynamic? evd = evr;

            return new
            {
                r.id,
                r.certNo,
                r.certTypeId,
                r.certTypeCode,
                r.certTypeName,

                raceNo = evd != null ? (ulong)evd.entryId : (ulong?)null,
                groupName = (string?)ar?.GroupName ?? (evd != null ? (string?)evd.groupName : null),

                holderPersonId = r.holderPersonId,
                holderName = r.holderName ?? (evd != null ? (string?)evd.athleteName : null),
                gender = r.holderGender ?? (evd != null ? (sbyte?)evd.athleteGender : null),
                holderMobile = r.holderMobile ?? (evd != null ? (string?)evd.athleteMobile : null),
                holderMobileMasked = r.holderMobileMasked ?? (evd != null ? (string?)evd.athleteMobileMasked : null),
                holderIdCardNo = r.holderIdCardNo ?? (evd != null ? (string?)evd.athleteIdCardNo : null),
                holderIdCardNoMasked = r.holderIdCardNoMasked ?? (evd != null ? (string?)evd.athleteIdCardNoMasked : null),

                assessStatus = ar?.ResultStatus,
                score = ar?.Score,
                points = ar?.Score,
                rank = (int?)null,

                associationName = ar?.AssociationName,
                certLevel = ar?.Level,
                projectName = ar?.ProjectName,
                province = ar?.Province,
                city = ar?.City,
                district = ar?.District,

                r.issueScene,
                r.issueAt,
                r.status,
                r.fileUrl,
                r.createdAt,
            };
        }).ToList();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    private async Task<string> NewCertNo(string typeCode)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"{typeCode}-{date}-";
        for (var i = 0; i < 8; i++)
        {
            var tail = Random.Shared.Next(100000, 999999).ToString();
            var no = prefix + tail;
            var exists = await _db.Certificates.AsNoTracking().AnyAsync(x => x.CertNo == no);
            if (!exists) return no;
        }
        return $"{typeCode}-{date}-{Guid.NewGuid():N}".Substring(0, Math.Min(64, $"{typeCode}-{date}-{Guid.NewGuid():N}".Length));
    }
}


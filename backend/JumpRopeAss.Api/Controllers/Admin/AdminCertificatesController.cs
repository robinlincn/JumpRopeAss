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
        string? CertNo,
        string? RaceNo,
        string? GroupName,
        string? Gender,
        string? Rank,
        string? ResultStatus,
        string? Score,
        string? AssociationName,
        string? RoleName,
        string? ProjectName,
        string? Level,
        string? Province,
        string? City,
        string? District,
        string? Points,
        string? SignPerson,
        string? ActivityDate,
        string? IssueDate,
        string? ValidPeriodText,
        string? CoachName,
        string? Location,
        string? Title,
        string? OrgName,
        string? ActivityName,
        string? IssuerOrg,
        string? Recommender,
        string? RecommenderPhone);

    public sealed record ImportCertificatesRequest(List<ImportRow> Items);

    [HttpPost("certificates/import")]
    public async Task<ApiResponse<object>> ImportCertificates([FromBody] ImportCertificatesRequest req)
    {
        if (req.Items is null || req.Items.Count == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "导入数据为空");
        if (req.Items.Count > 2000) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "单次导入最多2000条");

        var typeCodes = req.Items.Select(x => (x.CertTypeCode ?? string.Empty).Trim()).Where(x => x.Length > 0).Distinct().ToList();

        var ensureDefs = new[]
        {
            new { Code = "athlete_level", Name = "运动员等级证书" },
            new { Code = "coach_cert", Name = "教练员证书" },
            new { Code = "judge_cert", Name = "裁判员证书" },
        };
        var ensureNeed = ensureDefs.Where(x => typeCodes.Contains(x.Code)).ToList();
        if (ensureNeed.Count > 0)
        {
            var ensureCodes = ensureNeed.Select(x => x.Code).ToList();
            var existed = await _db.CertTypes.AsNoTracking()
                .Where(x => ensureCodes.Contains(x.Code))
                .Select(x => x.Code)
                .ToListAsync();
            var missing = ensureNeed.Where(x => !existed.Contains(x.Code)).ToList();
            if (missing.Count > 0)
            {
                _db.CertTypes.AddRange(missing.Select(x => new CertType { Code = x.Code, Name = x.Name, Status = 1 }));
                await _db.SaveChangesAsync();
            }
        }

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
            var idCard = string.IsNullOrWhiteSpace(r.IdCardNo) ? null : r.IdCardNo!.Trim();

            if (holderName.Length == 0)
            {
                errors.Add($"第{i + 2}行：姓名必填");
                continue;
            }

            DateTime issueAt = now;
            if (!string.IsNullOrWhiteSpace(r.IssueAt))
            {
                if (DateTime.TryParse(r.IssueAt!.Trim(), out var dt)) issueAt = dt;
                else if (double.TryParse(r.IssueAt!.Trim(), out var oaDate)) issueAt = DateTime.FromOADate(oaDate);
            }

            sbyte? gender = null;
            var genderRaw = (r.Gender ?? string.Empty).Trim();
            if (genderRaw == "男") gender = 1;
            else if (genderRaw == "女") gender = 2;
            else if (sbyte.TryParse(genderRaw, out var g) && g is >= 0 and <= 2) gender = g;

            Person? person = null;
            if (!string.IsNullOrWhiteSpace(idCard))
            {
                person = await _db.People.FirstOrDefaultAsync(x => x.DeletedAt == null && x.IdCardNo == idCard);
            }
            if (person is null && mobile.Length > 0)
            {
                person = await _db.People.FirstOrDefaultAsync(x => x.DeletedAt == null && x.Mobile == mobile && x.FullName == holderName);
            }
            if (person is null)
            {
                person = new Person
                {
                    FullName = holderName,
                    Mobile = mobile.Length > 0 ? mobile : null,
                    Gender = gender,
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
                if (mobile.Length > 0) person.Mobile = mobile;
                if (gender is not null) person.Gender = gender;
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

            var roleType = (sbyte)1;
            var roleName = (r.RoleName ?? string.Empty).Trim();
            if (roleName == "教练员") roleType = 2;
            else if (roleName == "裁判员") roleType = 3;
            else if (typeCode == "coach_cert") roleType = 2;
            else if (typeCode == "judge_cert") roleType = 3;

            var resultStatus = (r.ResultStatus ?? string.Empty).Trim();
            var scoreText = (r.Score ?? string.Empty).Trim();
            if (resultStatus.Length == 0 && (scoreText == "通过" || scoreText == "未通过" || scoreText == "合格" || scoreText == "不合格"))
            {
                resultStatus = scoreText;
                scoreText = string.Empty;
            }

            int? scoreValue = null;
            if (int.TryParse(scoreText, out var sv)) scoreValue = sv;
            else if (double.TryParse(scoreText, out var sd)) scoreValue = (int)Math.Round(sd);
            if (scoreValue is null)
            {
                var pointsText = (r.Points ?? string.Empty).Trim();
                if (int.TryParse(pointsText, out var pv)) scoreValue = pv;
                else if (double.TryParse(pointsText, out var pd)) scoreValue = (int)Math.Round(pd);
            }

            DateOnly? activityDate = null;
            var activityRaw = (r.ActivityDate ?? string.Empty).Trim();
            if (activityRaw.Length > 0)
            {
                if (DateOnly.TryParse(activityRaw, out var ad)) activityDate = ad;
                else if (double.TryParse(activityRaw, out var adOa)) activityDate = DateOnly.FromDateTime(DateTime.FromOADate(adOa));
            }

            DateOnly? issueDate = null;
            var issueDateRaw = (r.IssueDate ?? string.Empty).Trim();
            if (issueDateRaw.Length > 0)
            {
                if (DateOnly.TryParse(issueDateRaw, out var idt)) issueDate = idt;
                else if (double.TryParse(issueDateRaw, out var idOa)) issueDate = DateOnly.FromDateTime(DateTime.FromOADate(idOa));
            }

            var assess = await _db.AssessmentRecords.FirstOrDefaultAsync(x => x.CertNo == certNo);
            if (assess is null)
            {
                assess = new AssessmentRecord
                {
                    EventId = null,
                    PersonId = person.Id,
                    RoleType = roleType,
                    CreatedAt = now,
                };
                _db.AssessmentRecords.Add(assess);
            }
            else
            {
                assess.PersonId = person.Id;
                assess.RoleType = roleType;
            }

            assess.GroupName = string.IsNullOrWhiteSpace(r.GroupName) ? null : r.GroupName!.Trim();
            assess.ProjectName = string.IsNullOrWhiteSpace(r.ProjectName) ? null : r.ProjectName!.Trim();
            assess.Level = string.IsNullOrWhiteSpace(r.Level) ? null : r.Level!.Trim();
            assess.ResultStatus = resultStatus.Length == 0 ? null : resultStatus;
            assess.Score = scoreValue;
            assess.AssociationName = string.IsNullOrWhiteSpace(r.AssociationName) ? null : r.AssociationName!.Trim();
            assess.Province = string.IsNullOrWhiteSpace(r.Province) ? null : r.Province!.Trim();
            assess.City = string.IsNullOrWhiteSpace(r.City) ? null : r.City!.Trim();
            assess.District = string.IsNullOrWhiteSpace(r.District) ? null : r.District!.Trim();
            assess.SignPerson = string.IsNullOrWhiteSpace(r.SignPerson) ? null : r.SignPerson!.Trim();
            assess.ActivityDate = activityDate;
            assess.IssueDate = issueDate;
            assess.ValidPeriodText = string.IsNullOrWhiteSpace(r.ValidPeriodText) ? null : r.ValidPeriodText!.Trim();
            assess.CoachName = string.IsNullOrWhiteSpace(r.CoachName) ? null : r.CoachName!.Trim();
            assess.Location = string.IsNullOrWhiteSpace(r.Location) ? null : r.Location!.Trim();
            assess.CertNo = certNo;
            assess.Title = string.IsNullOrWhiteSpace(r.Title) ? null : r.Title!.Trim();
            assess.OrgName = string.IsNullOrWhiteSpace(r.OrgName) ? null : r.OrgName!.Trim();
            assess.ActivityName = string.IsNullOrWhiteSpace(r.ActivityName) ? null : r.ActivityName!.Trim();
            assess.IssuerOrg = string.IsNullOrWhiteSpace(r.IssuerOrg) ? null : r.IssuerOrg!.Trim();
            assess.Recommender = string.IsNullOrWhiteSpace(r.Recommender) ? null : r.Recommender!.Trim();
            assess.RecommenderPhone = string.IsNullOrWhiteSpace(r.RecommenderPhone) ? null : r.RecommenderPhone!.Trim();

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

    [HttpPost("certificates/clear")]
    public async Task<ApiResponse<object>> ClearCertificates()
    {
        var deleted = 0;
        try
        {
            deleted = await _db.Certificates.ExecuteDeleteAsync();
        }
        catch
        {
            var all = await _db.Certificates.ToListAsync();
            deleted = all.Count;
            _db.Certificates.RemoveRange(all);
            await _db.SaveChangesAsync();
        }
        return ApiResponse<object>.Ok(new { deleted });
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
                x.SignPerson,
                x.ActivityDate,
                x.IssueDate,
                x.ValidPeriodText,
                x.CoachName,
                x.Location,
                x.Title,
                x.OrgName,
                x.ActivityName,
                x.IssuerOrg,
                x.Recommender,
                x.RecommenderPhone,
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

                rank = (string?)null,
                assessStatus = ar?.ResultStatus,
                score = ar?.Score,
                associationName = ar?.AssociationName,
                projectName = ar?.ProjectName,
                certLevel = ar?.Level,
                province = ar?.Province,
                city = ar?.City,
                district = ar?.District,
                points = ar?.Score,
                issuerName = ar?.SignPerson,
                eventDate = ar?.ActivityDate != null ? ar.ActivityDate.Value.ToString("yyyy-MM-dd") : null,
                issueDate = ar?.IssueDate != null ? ar.IssueDate.Value.ToString("yyyy-MM-dd") : null,
                validPeriod = ar?.ValidPeriodText,
                coachName = ar?.CoachName,
                location = ar?.Location,
                titleName = ar?.Title,
                unitName = ar?.OrgName,
                eventName = ar?.ActivityName ?? (evd != null ? (string?)evd.eventTitle : null),
                issueOrg = ar?.IssuerOrg,
                referrerName = ar?.Recommender,
                referrerPhone = ar?.RecommenderPhone,

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

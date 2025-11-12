using System.Text.RegularExpressions;
using CVGeneratorAPI.Dtos;

namespace CVGeneratorAPI.Utils;

public static class Normalization
{
    private static readonly Regex MultiSpace = new(@"\s{2,}", RegexOptions.Compiled);

    private static string Norm(string s) =>
        MultiSpace.Replace((s ?? string.Empty).Trim(), " ");

    private static string? NormNullable(string? s)
    {
        if (s is null) return null;
        var x = Norm(s);
        return x.Length == 0 ? null : x;
    }

    private static List<string> NormSkills(IEnumerable<string> skills) =>
        skills
            .Select(Norm)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    // --- DTO-level normalization ---

    public static CreateCVRequest Normalize(this CreateCVRequest r) => r with
    {
        FirstName = Norm(r.FirstName),
        LastName  = Norm(r.LastName),
        City      = Norm(r.City),
        Country   = Norm(r.Country),
        Postcode  = Norm(r.Postcode),
        Phone     = Norm(r.Phone),
        Email     = Norm(r.Email),
        Photo     = NormNullable(r.Photo),
        JobTitle  = Norm(r.JobTitle),
        Summary   = Norm(r.Summary),

        Skills = NormSkills(r.Skills),

        WorkExperiences = r.WorkExperiences.Select(w => w with
        {
            Position = Norm(w.Position),
            Company  = Norm(w.Company),
            Description = Norm(w.Description)
        }).ToList(),

        Educations = r.Educations.Select(e => e with
        {
            Degree = Norm(e.Degree),
            School = Norm(e.School)
        }).ToList(),

        Links = r.Links.Select(l => l with
        {
            Type = Norm(l.Type),
            Url  = Norm(l.Url)
        }).ToList()
    };

    public static UpdateCVRequest NormalizePartial(this UpdateCVRequest r) => r with
    {
        FirstName = NormNullable(r.FirstName),
        LastName  = NormNullable(r.LastName),
        City      = NormNullable(r.City),
        Country   = NormNullable(r.Country),
        Postcode  = NormNullable(r.Postcode),
        Phone     = NormNullable(r.Phone),
        Email     = NormNullable(r.Email),
        Photo     = NormNullable(r.Photo),
        JobTitle  = NormNullable(r.JobTitle),
        Summary   = NormNullable(r.Summary),

        Skills = r.Skills is null ? null : NormSkills(r.Skills),

        WorkExperiences = r.WorkExperiences?.Select(w => w with
        {
            Position = Norm(w.Position),
            Company  = Norm(w.Company),
            Description = Norm(w.Description)
        }).ToList(),

        Educations = r.Educations?.Select(e => e with
        {
            Degree = Norm(e.Degree),
            School = Norm(e.School)
        }).ToList(),

        Links = r.Links?.Select(l => l with
        {
            Type = Norm(l.Type),
            Url  = Norm(l.Url)
        }).ToList()
    };
}

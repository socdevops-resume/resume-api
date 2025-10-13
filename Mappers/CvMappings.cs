using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Models;

namespace CVGeneratorAPI.Mappers;

public static class CvMappings
{
    public static CVModel ToModel(this CreateCVRequest dto, string userId) => new()
    {
        UserId = userId,
        FirstName = dto.FirstName,
        LastName  = dto.LastName,
        City      = dto.City,
        Country   = dto.Country,
        Postcode  = dto.Postcode,
        Phone     = dto.Phone,
        Email     = dto.Email,
        Photo     = dto.Photo,
        JobTitle  = dto.JobTitle,
        Summary   = dto.Summary,
        Skills    = dto.Skills.ToList(),
        WorkExperiences = dto.WorkExperiences.Select(w => new WorkExperience
        {
            Position = w.Position,
            Company  = w.Company,
            StartDate = w.StartDate,
            EndDate   = w.EndDate,
            Description = w.Description
        }).ToList(),
        Educations = dto.Educations.Select(e => new Education
        {
            Degree = e.Degree,
            School = e.School,
            StartDate = e.StartDate,
            EndDate   = e.EndDate
        }).ToList(),
        Links = dto.Links.Select(l => new Link { Type = l.Type, Url = l.Url }).ToList()
    };

    public static CVResponse ToResponse(this CVModel m) => new(
        m.Id!,
        m.FirstName,
        m.LastName,
        m.City,
        m.Country,
        m.Postcode,
        m.Phone,
        m.Email,
        m.Photo,
        m.JobTitle,
        m.Summary,
        m.Skills.ToList(),
        m.WorkExperiences.Select(w => new WorkExperienceDto(w.Position, w.Company, w.StartDate, w.EndDate, w.Description)).ToList(),
        m.Educations.Select(e => new EducationDto(e.Degree, e.School, e.StartDate, e.EndDate)).ToList(),
        m.Links.Select(l => new LinkDto(l.Type, l.Url)).ToList()
    );
}

using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Models;

namespace CVGeneratorAPI.Mappers;

public static class CvMappings
{
    // ------- DTO (Create) -> Model -------
    public static CVModel ToModel(this CreateCVRequest dto, string userId) => new()
    {
        UserId    = userId,
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
            Position   = w.Position,
            Company    = w.Company,
            StartDate  = w.StartDate,
            EndDate    = w.EndDate,
            Description= w.Description
        }).ToList(),
        Educations = dto.Educations.Select(e => new Education
        {
            Degree    = e.Degree,
            School    = e.School,
            StartDate = e.StartDate,
            EndDate   = e.EndDate
        }).ToList(),
        Links = dto.Links.Select(l => new Link
        {
            Type = l.Type,
            Url  = l.Url
        }).ToList()
    };

    // ------- Model -> DTO (Response) -------
    // CVResponse can stay a positional record; we just build its inner lists with object initializers.
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
        m.WorkExperiences.Select(w => new WorkExperienceDto
        {
            Position    = w.Position,
            Company     = w.Company,
            StartDate   = w.StartDate,
            EndDate     = w.EndDate,
            Description = w.Description
        }).ToList(),
        m.Educations.Select(e => new EducationDto
        {
            Degree    = e.Degree,
            School    = e.School,
            StartDate = e.StartDate,
            EndDate   = e.EndDate
        }).ToList(),
        m.Links.Select(l => new LinkDto
        {
            Type = l.Type,
            Url  = l.Url
        }).ToList()
    );

    // ------- Apply partial update (PATCH/PUT semantics you described) -------
    // Only non-null properties are applied.
    public static void ApplyUpdate(this CVModel m, UpdateCVRequest dto)
    {
        if (dto.FirstName is not null) m.FirstName = dto.FirstName;
        if (dto.LastName  is not null) m.LastName  = dto.LastName;
        if (dto.City      is not null) m.City      = dto.City;
        if (dto.Country   is not null) m.Country   = dto.Country;
        if (dto.Postcode  is not null) m.Postcode  = dto.Postcode;
        if (dto.Phone     is not null) m.Phone     = dto.Phone;
        if (dto.Email     is not null) m.Email     = dto.Email;

        if (dto.Photo    is not null) m.Photo    = dto.Photo;
        if (dto.JobTitle is not null) m.JobTitle = dto.JobTitle;
        if (dto.Summary  is not null) m.Summary  = dto.Summary;

        if (dto.Skills is not null)
            m.Skills = dto.Skills.ToList();

        if (dto.WorkExperiences is not null)
            m.WorkExperiences = dto.WorkExperiences.Select(w => new WorkExperience
            {
                Position    = w.Position,
                Company     = w.Company,
                StartDate   = w.StartDate,
                EndDate     = w.EndDate,
                Description = w.Description
            }).ToList();

        if (dto.Educations is not null)
            m.Educations = dto.Educations.Select(e => new Education
            {
                Degree    = e.Degree,
                School    = e.School,
                StartDate = e.StartDate,
                EndDate   = e.EndDate
            }).ToList();

        if (dto.Links is not null)
            m.Links = dto.Links.Select(l => new Link
            {
                Type = l.Type,
                Url  = l.Url
            }).ToList();
    }
}

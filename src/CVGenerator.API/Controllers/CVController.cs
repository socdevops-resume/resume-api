using System.Security.Claims;
using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Mappers;
using CVGeneratorAPI.Services;
using CVGeneratorAPI.Utils; 
using CVGeneratorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/cvs")]
[Tags("CVs")]
public class CVsController : ControllerBase
{
    private readonly CVService _cvService;
    private readonly ILogger<CVsController> _logger;

    public CVsController(CVService cvService, ILogger<CVsController> logger)
    {
        _cvService = cvService;
        _logger = logger;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<ActionResult<CVResponse>> Create([FromBody] CreateCVRequest newCv)
    {
        var dto = newCv.Normalize(); // normalize
        var model = dto.ToModel(UserId);
        await _cvService.CreateCvAsync(model);
        var response = model.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CVResponse>> Update(string id, [FromBody] UpdateCVRequest dtoRaw)
    {
        var dto = dtoRaw.NormalizePartial(); // normalize partial

        var existing = await _cvService.GetByIdForUserAsync(id, UserId);
        if (existing is null) return NotFound("CV not found.");

        var updated = await _cvService.UpdatePartialForUserAsync(id, UserId, u => u
            .SetIfNotNull(dto.FirstName, x => x.FirstName)
            .SetIfNotNull(dto.LastName, x => x.LastName)
            .SetIfNotNull(dto.City, x => x.City)
            .SetIfNotNull(dto.Country, x => x.Country)
            .SetIfNotNull(dto.Postcode, x => x.Postcode)
            .SetIfNotNull(dto.Phone, x => x.Phone)
            .SetIfNotNull(dto.Email, x => x.Email)
            .SetIfNotNull(dto.Photo, x => x.Photo)
            .SetIfNotNull(dto.JobTitle, x => x.JobTitle)
            .SetIfNotNull(dto.Summary, x => x.Summary)
            .ReplaceListIfProvided(dto.Skills, x => x.Skills)
            .ReplaceListIfProvided(dto.WorkExperiences?.Select(w => new WorkExperience
            {
                Position = w.Position,
                Company = w.Company,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Description = w.Description
            }).ToList(), x => x.WorkExperiences)
            .ReplaceListIfProvided(dto.Educations?.Select(e => new Education
            {
                Degree = e.Degree,
                School = e.School,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList(), x => x.Educations)
            .ReplaceListIfProvided(dto.Links?.Select(l => new Link
            {
                Type = l.Type,
                Url = l.Url
            }).ToList(), x => x.Links)
        );

        if (updated is null) return NotFound("CV not found.");
        return Ok(updated.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<List<CVResponse>>> GetAll()
    {
        _logger.LogInformation("User {UserId} requested all CVs", UserId);
        var cvs = await _cvService.GetAllByUserAsync(UserId);
        _logger.LogInformation("User {UserId} retrieved {Count} CVs", UserId, cvs.Count);
        return Ok(cvs.Select(c => c.ToResponse()).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CVResponse>> GetById(string id)
    {
        _logger.LogInformation("User {UserId} requested CV {CvId}", UserId, id);
        var cv = await _cvService.GetByIdForUserAsync(id, UserId);
        if (cv is null)
        {
            _logger.LogWarning("User {UserId} tried to access CV {CvId} but it was not found", UserId, id);
            return NotFound("CV not found.");
        }
        _logger.LogInformation("User {UserId} retrieved CV {CvId}", UserId, id);
        return Ok(cv.ToResponse());
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        _logger.LogInformation("User {UserId} is deleting CV {CvId}", UserId, id);

        var existing = await _cvService.GetByIdForUserAsync(id, UserId);
        if (existing is null)
        {
            _logger.LogWarning("User {UserId} tried to delete CV {CvId} but it was not found", UserId, id);
            return NotFound("CV not found.");
        }

        await _cvService.DeleteForUserAsync(id, UserId);
        _logger.LogInformation("User {UserId} deleted CV {CvId}", UserId, id);
        return NoContent();
    }
}

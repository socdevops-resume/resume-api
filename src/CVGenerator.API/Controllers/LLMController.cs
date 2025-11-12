using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Services.Llm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/llmconnection")]
[Tags("llmconnection")]
public class LLMController : ControllerBase
{
    private readonly LlmService _service;
    public LLMController(LlmService service) => _service = service;

    [HttpPost("extract-keywords")]
    public async Task<IActionResult> Keywords([FromBody] JDRequest req, CancellationToken ct)
        => Ok(await _service.ExtractKeywordsAsync(req, ct));

    [HttpPost("summary")]
    public async Task<IActionResult> Summary([FromBody] SummaryRequest req, CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(req, ct);
        return Ok(result);
    }

    [HttpPost("cover-letter")]
    public async Task<IActionResult> CoverLetter([FromBody] CoverLetterRequest req, CancellationToken ct)
    {
        var result = await _service.GetCoverLetterAsync(req, ct);
        return Ok(result);
    }

}
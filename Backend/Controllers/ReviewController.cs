using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly AiReviewService _aiService;

        public ReviewController(AiReviewService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Review([FromBody] CodeInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Code))
                return BadRequest("Code is required.");

            var result = await _aiService.AnalyzeAsync(input.Code);
            return Ok(result);
        }
    }

    public class CodeInput
    {
        public string Code { get; set; } = string.Empty;
    }
}

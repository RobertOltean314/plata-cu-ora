using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Services;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeclarationController : ControllerBase
    {
        private readonly IDeclarationService _declarationService;

        public DeclarationController(IDeclarationService declarationService)
        {
            _declarationService = declarationService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateDeclaration([FromQuery] string userId, [FromBody] List<DateTime> workedDays)
        {
            try
            {
                var pdfBytes = await _declarationService.GenerateDeclarationAsync(userId, workedDays);
                return File(pdfBytes, "application/pdf", "declaratie.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
    }
}
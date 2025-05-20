using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Services;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeclaratieController : ControllerBase
    {
        private readonly IDeclaratieService _declaratieService;

        public DeclaratieController(IDeclaratieService declaratieService)
        {
            _declaratieService = declaratieService;
        }

        [HttpPost("genereaza")]
        public async Task<IActionResult> GenereazaDeclaratie([FromQuery] string userId, [FromBody] List<DateTime> zileLucrate)
        {
            try
            {
                var pdfBytes = await _declaratieService.GenereazaDeclaratieAsync(userId, zileLucrate);
                return File(pdfBytes, "application/pdf", "declaratie.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
    }
}
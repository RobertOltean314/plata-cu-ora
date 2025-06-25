using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Services;
using System.Globalization;

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
        public async Task<IActionResult> GenereazaDeclaratie(
            [FromQuery] string userId,
            [FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromBody] List<string> zileLucrate)
        {
            try
            {
                var intervalStart = DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var intervalEnd = DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var dateList = zileLucrate
                    .Select(z => DateTime.ParseExact(z, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                    .ToList();

                var pdfBytes = await _declaratieService.GenereazaDeclaratieAsync(userId, dateList, intervalStart, intervalEnd);
                return File(pdfBytes, "application/pdf", "declaratie.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
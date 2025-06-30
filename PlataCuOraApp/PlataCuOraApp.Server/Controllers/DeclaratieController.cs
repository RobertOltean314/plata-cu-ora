using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services;
using System.Collections.Concurrent;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeclaratieController : ControllerBase
    {
        private readonly IDeclaratieService _declaratieService;
        private static ConcurrentDictionary<string, byte[]> _pdfCache = new();

        public DeclaratieController(IDeclaratieService declaratieService)
        {
            _declaratieService = declaratieService;
        }


        [HttpPost("genereaza")]
        public async Task<IActionResult> GenereazaDeclaratie(
            [FromQuery] string userId,
            [FromQuery] DateTime firstDay,
            [FromQuery] DateTime lastDay,
            [FromBody] List<DateTime> zileLucrate)
        {
            try
            {
                var pdfBytes = await _declaratieService.GenereazaDeclaratieAsync(userId, zileLucrate, firstDay, lastDay);
                return File(pdfBytes, "application/pdf", "declaratie.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("genereaza-excel")]
        public async Task<IActionResult> GenereazaDeclaratieExcel(
            [FromQuery] string userId,
            [FromQuery] DateTime firstDay,
            [FromQuery] DateTime lastDay,
            [FromBody] List<DateTime> zileLucrate)
        {
            try
            {
                var excelBytes = await _declaratieService.GenereazaDeclaratieEXCELAsync(userId, zileLucrate, firstDay, lastDay);

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "declaratie.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"Eroare la generarea Excel: {ex.Message}");
            }
        }
    }
}
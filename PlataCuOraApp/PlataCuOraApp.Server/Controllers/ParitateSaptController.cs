using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Services;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Services;

namespace PlataCuOra.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParitateSaptController : ControllerBase
    {
        private readonly IParitateSaptService _service;

        public ParitateSaptController(IParitateSaptService service)
        {
            _service = service;
        }

        [HttpPost("{parId}")]
        public async Task<IActionResult> AddOrUpdateParitateSapt(string parId, [FromBody] List<ParitateSaptamanaDTO> saptamani)
        {
            await _service.AddOrUpdateParitateSaptAsync(parId, saptamani);
            return Ok(new { Message = "Successfully added." });
        }

        [HttpGet("{parId}")]
        public async Task<ActionResult<List<ParitateSaptamanaDTO>>> GetParitateSapt(string parId)
        {
            var result = await _service.GetParitateSaptAsync(parId);
            return Ok(result);
        }
    }
}

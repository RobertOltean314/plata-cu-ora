using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Services;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Services;
using PlataCuOraApp.Server.Domain.DTOs;

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

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddOrUpdateParitateSapt(string userId, [FromBody] List<ParitateSaptamanaDTO> saptamani)
        {
            await _service.AddOrUpdateParitateSaptAsync(userId, saptamani);
            return Ok(new { Message = "Successfully added." });
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<ParitateSaptamanaDTO>>> GetParitateSapt(string userId)
        {
            var result = await _service.GetParitateSaptAsync(userId);
            return Ok(result);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateParitateSapt(string userId, [FromBody] UpdateParitateRequest request)
        {
            if (request.OldEntry == null || request.NewEntry == null)
                return BadRequest("Both old and new entries must be provided.");

            var result = await _service.UpdateParitateAsync(userId, request.OldEntry, request.NewEntry);
            return result ? Ok(new { Message = "Updated successfully." }) : NotFound("Old entry not found.");
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteParitateSapt(string userId, [FromBody] ParitateSaptamanaDTO entry)
        {
            var result = await _service.DeleteParitateAsync(userId, entry);
            return result ? Ok(new { Message = "Deleted successfully." }) : NotFound("Entry not found.");
        }

    }
}

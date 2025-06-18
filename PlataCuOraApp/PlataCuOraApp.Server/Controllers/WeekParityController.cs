using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Services;
using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOra.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeekParityController : ControllerBase
    {
        private readonly IWeekParityService _service;

        public WeekParityController(IWeekParityService service)
        {
            _service = service;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddOrUpdateWeekParity(string userId, [FromBody] List<WeekParityDTO> weeks)
        {
            await _service.AddOrUpdateWeekParityAsync(userId, weeks);
            return Ok(new { Message = "Successfully added." });
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<WeekParityDTO>>> GetWeekParity(string userId)
        {
            var result = await _service.GetWeekParityAsync(userId);
            return Ok(result);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateWeekParity(string userId, [FromBody] UpdateParityRequest request)
        {
            if (request.OldEntry == null || request.NewEntry == null)
                return BadRequest("Both old and new entries must be provided.");

            var result = await _service.UpdateParityAsync(userId, request.OldEntry, request.NewEntry);
            return result ? Ok(new { Message = "Updated successfully." }) : NotFound("Old entry not found.");
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteWeekParity(string userId, [FromBody] WeekParityDTO entry)
        {
            var result = await _service.DeleteParityAsync(userId, entry);
            return result ? Ok(new { Message = "Deleted successfully." }) : NotFound("Entry not found.");
        }

    }
}

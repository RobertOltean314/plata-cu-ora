using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserScheduleController : ControllerBase
    {
        private readonly IUserScheduleService _service;

        public UserScheduleController(IUserScheduleService service) => _service = service;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(string userId)
        {
            var result = await _service.GetAllAsync(userId);
            return result == null
                ? NotFound("No schedule found for the specified user.")
                : Ok(result);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> Add(string userId, [FromBody] UserScheduleDTO entry)
        {
            var msg = await _service.AddAsync(userId, entry);
            return msg.Contains("already exists")
                ? Conflict(msg)
                : Ok(msg);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateScheduleRequest request)
        {
            var msg = await _service.UpdateAsync(userId, request.OldEntry, request.NewEntry);
            return msg.Contains("not found")
                ? NotFound(msg)
                : Ok(msg);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId, [FromBody] UserScheduleDTO entry)
        {
            var msg = await _service.DeleteAsync(userId, entry);
            return msg.Contains("not found")
                ? NotFound(msg)
                : Ok(msg);
        }
    }
}

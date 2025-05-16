using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrarUserController : ControllerBase
    {
        private readonly IOrarUserService _service;

        public OrarUserController(IOrarUserService service) => _service = service;

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(string userId)
        {
            var result = await _service.GetAllAsync(userId);
            return result == null
                ? NotFound("No schedule found for the specified user.")
                : Ok(result);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> Add(string userId, [FromBody] OrarUserDTO entry)
        {
            var msg = await _service.AddAsync(userId, entry);
            return msg.Contains("already exists")
                ? Conflict(msg)
                : Ok(msg);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateOrarRequest request)
        {
            var msg = await _service.UpdateAsync(userId, request.OldEntry, request.NewEntry);
            return msg.Contains("not found")
                ? NotFound(msg)
                : Ok(msg);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId, [FromBody] OrarUserDTO entry)
        {
            var msg = await _service.DeleteAsync(userId, entry);
            return msg.Contains("not found")
                ? NotFound(msg)
                : Ok(msg);
        }
    }
}

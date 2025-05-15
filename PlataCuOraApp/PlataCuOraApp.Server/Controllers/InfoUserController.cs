using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain;
using PlataCuOraApp.Server.Services.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;
using System.Threading.Tasks;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOraApp.Server.Services.Implementation;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoUserController : ControllerBase
    {
        private readonly IInfoUserService _infoUserService;

        public InfoUserController(IInfoUserService infoUserService)
        {
            _infoUserService = infoUserService;
        }

        [HttpPost("extra-info/{userId}")]
        public async Task<IActionResult> AddExtraInfo(string userId, [FromBody] InfoUserDTO request)
        {
            var (success, error) = await _infoUserService.UpdateUserAsync(userId, request);

            if (success)
            {
                return Ok(new { message = "Added user info successfully" });
            }

            return BadRequest(new { message = error });
        }

        [HttpGet("extra-info/{userId}")]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            var result = await _infoUserService.GetUserByIdAsync(userId.Trim());

            if (result == null)
                return NotFound(new { message = $"User {userId} info not found" });

            return Ok(result);
        }

    }
}

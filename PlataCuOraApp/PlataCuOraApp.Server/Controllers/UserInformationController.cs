using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;
using System.Threading.Tasks;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserInformationController : ControllerBase
    {
        private readonly IUserInformationService _service;
        private readonly ILogger<UserInformationController> _logger;

        public UserInformationController(IUserInformationService service, ILogger<UserInformationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> GetAll(string userId)
        {
            _logger.LogInformation($"GET all info requested for user {userId}");
            var result = await _service.GetAllInfoAsync(userId);
            return Ok(result);
        }

        [HttpPost("add/{userId}")]
        public async Task<IActionResult> Add(string userId, [FromBody] UserInformationDTO info)
        {
            _logger.LogInformation($"POST add info requested for user {userId}");
            var (success, error) = await _service.AddInfoAsync(userId, info);
            if (success)
            {
                return Ok(new { message = "Info entry added successfully." });
            }
            else
            {
                _logger.LogWarning($"Add info failed for user {userId}: {error}");
                return BadRequest(new { error });
            }
        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateInfoRequestDTO request)
        {
            _logger.LogInformation($"PUT update info requested for user {userId}");
            var (success, error) = await _service.UpdateInfoAsync(userId, request.Old, request.New);
            if (success)
            {
                return Ok(new { message = "Info entry updated successfully." });
            }
            else
            {
                _logger.LogWarning($"Update info failed for user {userId}: {error}");
                return BadRequest(new { error });
            }
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> Delete(string userId, [FromBody] UserInformationDTO info)
        {
            _logger.LogInformation($"DELETE info requested for user {userId}");
            var (success, error) = await _service.DeleteInfoAsync(userId, info);
            if (success)
            {
                return Ok(new { message = "Info entry deleted successfully." });
            }
            else
            {
                _logger.LogWarning($"Delete info failed for user {userId}: {error}");
                return BadRequest(new { error });
            }
        }

        [HttpPost("set-active/{userId}")]
        public async Task<IActionResult> SetActive(string userId, [FromBody] UserInformationDTO info)
        {
            _logger.LogInformation($"POST set-active info requested for user {userId}");
            var (success, error) = await _service.SetActiveAsync(userId, info);
            if (success)
            {
                return Ok(new { message = "Info entry set as active successfully." });
            }
            else
            {
                _logger.LogWarning($"Set active info failed for user {userId}: {error}");
                return BadRequest(new { error });
            }
        }

        [HttpPost("unset-active/{userId}")]
        public async Task<IActionResult> UnsetActive(string userId, [FromBody] UserInformationDTO info)
        {
            _logger.LogInformation($"POST unset-active info requested for user {userId}");
            var (success, error) = await _service.UnsetActiveAsync(userId, info);
            if (success)
            {
                return Ok(new { message = "Info entry unset from active successfully." });
            }
            else
            {
                _logger.LogWarning($"Unset active info failed for user {userId}: {error}");
                return BadRequest(new { error });
            }
        }

        [HttpPost("{userId}/add-active-info")]
        public async Task<ActionResult<UserInformationDTO?>> AddActiveInfoToDb(string userId)
        {
            _logger.LogInformation($"POST add-active-info requested for user {userId}");
            var result = await _service.AddActiveInfoToDbAsync(userId);
            if (result == null)
            {
                _logger.LogWarning($"No active info found to add for user {userId}");
                return NotFound(new { error = $"No active info found for user {userId}." });
            }

            return Ok(result);
        }

        [HttpGet("{userId}/info-user")]
        public async Task<ActionResult<UserInformationDTO?>> GetInfoUserFromDb(string userId)
        {
            _logger.LogInformation($"GET info-user requested for user {userId}");
            var result = await _service.GetInfoUserFromDbAsync(userId);
            if (result == null)
            {
                _logger.LogWarning($"No info user document found for user {userId}");
                return NotFound(new { error = $"No info user document found for user {userId}." });
            }

            return Ok(result);
        }

        [HttpGet("{userId}/active-info")]
        public async Task<ActionResult<UserInformationDTO?>> GetActiveInfoAsync(string userId)
        {
            _logger.LogInformation($"GET active-info requested for user {userId}");
            var result = await _service.GetActiveInfoAsync(userId);
            if (result == null)
            {
                _logger.LogWarning($"No active info found for user {userId}");
                return NotFound(new { error = $"No active info found for user {userId}." });
            }

            return Ok(result);
        }
    }
}

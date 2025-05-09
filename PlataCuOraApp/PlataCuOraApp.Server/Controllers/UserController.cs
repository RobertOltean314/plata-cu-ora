using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;
using System.Threading.Tasks;
namespace PlataCuOra.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public UserController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDTO request)
        {
            var (success, error) = await _authService.RegisterUserAsync(request);

            if(success)
            {
                return Ok(new { message = "User registered succesfully" });
            }
            return BadRequest(new { message = error });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDTO request)
        {
            var (success, token, userData, error) = await _authService.LoginUserAsync(request);

            if(!success)
            {
                return Unauthorized(new { message = error });
            }

            return Ok(new
            {
                message = "Login successful.",
                token,
                user = userData
            });
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyTokenAsync([FromBody] TokenVerificationRequest request)
        {
            var isValid = await _authService.VerifyTokenAsync(request.Token);
            return Ok(new { valid = isValid });
        }
    }
}
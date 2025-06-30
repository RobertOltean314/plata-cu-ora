// ADAUGĂ ACEST USING PENTRU LOGARE
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PlataCuOra.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        // ADAUGĂ CÂMPUL PENTRU LOGGER
        private readonly ILogger<UserController> _logger;

        // MODIFICĂ CONSTRUCTORUL PENTRU A PRIMI LOGGER-UL
        public UserController(IAuthService authService, IUserService userService, ILogger<UserController> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDTO request)
        {
            var (success, error) = await _authService.RegisterUserAsync(request);

            if (success)
            {
                return Ok(new { message = "User registered succesfully" });
            }
            return BadRequest(new { message = error });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDTO request)
        {
            var (success, token, userData, error) = await _authService.LoginUserAsync(request);

            if (!success)
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

        //[HttpPost("verify-token")]
        //public async Task<IActionResult> VerifyTokenAsync([FromBody] TokenVerificationRequest request)
        //{
        //    var isValid = await _authService.VerifyTokenAsync(request.Token);
        //    return Ok(new { valid = isValid });
        //}

        [HttpPost("google-login")]
        public async Task<IActionResult> LoginWithGoogleAsync([FromBody] GoogleLoginRequestDTO request)
        {
            _logger.LogInformation("IdToken primit din body: {IdToken}", request.idToken);
            var token2 = Request.Headers["Authorization"];
            _logger.LogInformation($"Token primit: {token2}");

            if (string.IsNullOrWhiteSpace(request.idToken))
                return BadRequest(new { message = "IdToken is required." });

            try
            {
                var (success, token, user, error) = await _authService.LoginWithGoogleAsync(request);

                if (!success)
                    return Unauthorized(new { message = error });

                _logger.LogInformation("User dupa LoginWithGoogle: {@user}", user);

                return Ok(new
                {
                    message = "Google login successful.",
                    token,
                    user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la LoginWithGoogleAsync");
                return StatusCode(500, new { message = "A aparut o eroare interna." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            return Ok(user);
        }
    }
}
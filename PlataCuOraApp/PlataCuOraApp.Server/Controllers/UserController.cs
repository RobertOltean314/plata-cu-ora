// ADAUGĂ ACEST USING PENTRU LOGARE
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyTokenAsync([FromBody] TokenVerificationRequest request)
        {
            var isValid = await _authService.VerifyTokenAsync(request.Token);
            return Ok(new { valid = isValid });
        }


        //
        // AICI ESTE METODA MODIFICATĂ CU LOG-URI
        //
        [HttpPost("google-login")]
        public async Task<IActionResult> LoginWithGoogleAsync()
        {
            _logger.LogInformation("--- Endpoint /api/user/google-login a fost apelat. ---");

            // Încercăm să extragem header-ul
            string authorizationHeader = Request.Headers["Authorization"];

            // Logăm exact ce am primit, ca să vedem cu ochii noștri
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                _logger.LogWarning("Header-ul 'Authorization' este GOL sau NULL.");
            }
            else
            {
                _logger.LogInformation("Am primit header-ul 'Authorization' cu valoarea: '{AuthHeader}'", authorizationHeader);
            }

            // Aici este validarea pe care o suspectăm că eșuează
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("VALIDAREA A EȘUAT: Header-ul lipsește sau nu începe cu 'Bearer '.");
                return BadRequest(new { message = "Missing or invalid Authorization header." });
            }

            // Dacă ajungem aici, înseamnă că validarea a trecut
            _logger.LogInformation("Validarea header-ului a trecut. Se extrage token-ul și se apelează serviciul.");

            string idToken = authorizationHeader.Substring("Bearer ".Length);

            var (success, token, user, error) = await _authService.LoginWithGoogleAsync(idToken);

            if (!success)
            {
                // Logăm eroarea primită de la serviciu
                _logger.LogError("Serviciul de autentificare a eșuat. Eroare: {Error}", error);
                return Unauthorized(new { message = error });
            }

            _logger.LogInformation("Autentificarea prin serviciu a reușit. Se returnează răspunsul OK.");
            return Ok(new
            {
                message = "Google login successful.",
                token,
                user
            });
        }
    }
}
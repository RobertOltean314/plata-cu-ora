using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Domain.DTO;
using PlataCuOra.Server.Repository.Interface;
using System.Threading.Tasks;
namespace PlataCuOra.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDTO request)
        {
            var result = await _userRepository.RegisterUserAsync(request);
            if (result)
                return Ok("User registered successfully.");
            else
                return BadRequest("Error registering user.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var (success, token, userData, error) = await _userRepository.LoginUserAsync(request);
            if (!success)
                return Unauthorized(new { message = error });
            return Ok(new
            {
                message = "Login successful.",
                token,
                user = userData
            });
        }

        [HttpPost("test-firebase")]
        public IActionResult TestFirebase()
        {
            return Ok("Firebase working.");
        }
    }
}
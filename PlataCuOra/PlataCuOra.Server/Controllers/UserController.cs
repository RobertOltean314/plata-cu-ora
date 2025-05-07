<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Mvc;
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
=======
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Models.Domain;
using PlataCuOra.Server.Models.DTO;
using PlataCuOra.Server.Repo.Implementation;
using PlataCuOra.Server.Repo.Interface;
using PlataCuOra.Server.Utils;

namespace PlataCuOra.Server.Migrations
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserRepository userRepository;

		public UserController(IUserRepository userRepository)
		{
			this.userRepository = userRepository;
		}



		// POST : https://localhost:7176/api/user
		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
		{
			var user = await userRepository.GetByEmail(request.Email);
			if (user == null)
			{
				return NotFound();
			}
			if (PasswordHasher.Encrypt(request.Password) != user.Password)
			{
				return Unauthorized();
			}

			return Ok(new UserDTO
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				Password = user.Password,
				Role = user.Role,
			});
		}

		// POST : https://localhost:7176/api/user
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
		{
			var user = new User
			(
				request.Name,
				request.Email,
				PasswordHasher.Encrypt(request.Password),
				request.Role
			);

			user = await userRepository.CreateAsync(user);
			return Ok(
				new UserDTO
				{
					Id = user.Id,
					Name = user.Name,
					Email = user.Email,
					Password = user.Password,
					Role = user.Role
				}
			);
		}
	}
}
>>>>>>> SqlServerLogin

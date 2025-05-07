using Microsoft.AspNetCore.Http;
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

using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Domain.DTO;
using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Data;
using Google.Cloud.Firestore;

namespace PlataCuOra.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		//private IUserRepository userRepository;
		private readonly AppDbContext appDbContext;

		public UserController()
		{
			//this.userRepository = userRepository;
			this.appDbContext = new AppDbContext();
		}

		// POST : https://localhost:____/api/user/register
		/*[HttpPost]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
		{
			if (request == null)
			{
				return BadRequest("Invalid request.");
			}

			var user = new User
			{
				Name = request.Name,
				Email = request.Email,
				Password = request.Password,
				Role = request.Role
			};

			try
			{
				var userRecord = await this.userRepository.CreateAsync(user);

				//to do : return dto
				return Ok(userRecord);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
		*/


		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
		{
			try
			{
				var userRecord = await appDbContext._auth.CreateUserAsync(new UserRecordArgs
				{
					Email = request.Email,
					Password = request.Password,
					DisplayName = request.Name
				});

				var userDoc = new Dictionary<string, object>
			{
				{ "uid", userRecord.Uid },
				{ "name", request.Name },
				{ "email", request.Email },
				{ "created_at", Timestamp.GetCurrentTimestamp() }
			};

				await appDbContext._firestoreDb.Collection("users").Document(userRecord.Uid).SetAsync(userDoc);
				var token = await appDbContext._auth.CreateCustomTokenAsync(userRecord.Uid);

				return Ok(new { message = "User registered successfully.", token, user = userDoc });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}
	}
}

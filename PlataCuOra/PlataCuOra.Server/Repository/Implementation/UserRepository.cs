using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using PlataCuOra.Server.Data;
using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Repository.Interface;
using static Google.Rpc.Context.AttributeContext.Types;

namespace PlataCuOra.Server.Repository.Implementation
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext appDbContext;

		public UserRepository(AppDbContext appDbContext)
		{
			this.appDbContext = appDbContext;
		}

		public async Task<User> CreateAsync(User user)
		{
			var userRecord = new UserRecordArgs()
			{
				Uid = user.Id.ToString(),
				DisplayName = user.Name,
				Email = user.Email,
				Password = user.Password,
			};

			var createdUser = await this.appDbContext._auth.CreateUserAsync(
				userRecord
			);

			var userDoc = new Dictionary<string, object>
			{
				{ "uid", userRecord.Uid },
				{ "name", user.Name },
				{ "email", user.Email },
				{ "created_at", Timestamp.GetCurrentTimestamp() }
			};
			await this.appDbContext._firestoreDb.Collection("users").Document(userRecord.Uid).SetAsync(userDoc);

			user.Id = int.Parse(userRecord.Uid);
			return user;
		}

		public Task<User> DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<User>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<User?> GetById(int id)
		{
			throw new NotImplementedException();
		}

		public Task<User?> GetByUsername(string username)
		{
			throw new NotImplementedException();
		}

		public Task<User?> UpdateAsync(User user)
		{
			throw new NotImplementedException();
		}
	}
}

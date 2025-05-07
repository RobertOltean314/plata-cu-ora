using Microsoft.EntityFrameworkCore;
using PlataCuOra.Server.Data;
using PlataCuOra.Server.Models.Domain;
using PlataCuOra.Server.Repo.Interface;

namespace PlataCuOra.Server.Repo.Implementation
{
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext dbContext;

		public UserRepository(ApplicationDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public async Task<User> CreateAsync(User user)
		{
			await dbContext.Users.AddAsync(user);
			await dbContext.SaveChangesAsync();
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

		public async Task<User?> GetByEmail(string email) =>
			await dbContext.Users
				.FirstOrDefaultAsync(u => u.Email == email);

		public Task<User?> UpdateAsync(User user)
		{
			throw new NotImplementedException();
		}
	}
}

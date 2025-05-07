using PlataCuOra.Server.Domain;

namespace PlataCuOra.Server.Repository.Interface
{
	public interface IUserRepository
	{
		Task<User> CreateAsync(User user);
		Task<IEnumerable<User>> GetAllAsync();
		Task<User?> GetById(int id);
		Task<User?> UpdateAsync(User user);
		Task<User> DeleteAsync(int id);
		Task<User?> GetByUsername(string username);
	}
}

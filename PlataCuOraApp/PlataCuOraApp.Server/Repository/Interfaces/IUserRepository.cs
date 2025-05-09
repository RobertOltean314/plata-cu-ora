using PlataCuOra.Server.Domain.Entities;
using System.Threading.Tasks;

namespace PlataCuOra.Server.Repository.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string userId);
    }
}
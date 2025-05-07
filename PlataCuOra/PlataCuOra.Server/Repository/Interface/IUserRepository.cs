using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Domain.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<bool> RegisterUserAsync(RegisterRequestDTO request);
        Task<(bool success, string? token, Dictionary<string, object>? userData, string? error)> LoginUserAsync(LoginRequestDTO request);
    }
}
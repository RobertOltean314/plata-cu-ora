using PlataCuOraApp.Server.Domain.Entities;

namespace PlataCuOraApp.Server.Repository.Interfaces
{
    public interface IInfoUserRepository
    {
        Task<InfoUser?> GetUserByIdAsync(string userId);
        Task<bool> CreateUserAsync(InfoUser user);
        Task<bool> UpdateUserAsync(InfoUser user);
    }
}

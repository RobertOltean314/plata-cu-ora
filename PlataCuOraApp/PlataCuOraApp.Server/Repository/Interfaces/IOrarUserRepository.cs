using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Repository.Interfaces
{
    public interface IOrarUserRepository
    {
        Task<List<OrarUserDTO>?> GetAllAsync(string userId);
        Task<bool> AddAsync(string userId, OrarUserDTO entry);
        Task<bool> UpdateAsync(string userId, OrarUserDTO oldEntry, OrarUserDTO newEntry);
        Task<bool> DeleteAsync(string userId, OrarUserDTO entry);
    }
}

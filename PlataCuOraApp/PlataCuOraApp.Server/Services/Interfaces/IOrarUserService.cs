using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IOrarUserService
    {
        Task<List<OrarUserDTO>?> GetAllAsync(string userId);
        Task<string> AddAsync(string userId, OrarUserDTO entry);
        Task<string> UpdateAsync(string userId, OrarUserDTO oldEntry, OrarUserDTO newEntry);
        Task<string> DeleteAsync(string userId, OrarUserDTO entry);
    }
}
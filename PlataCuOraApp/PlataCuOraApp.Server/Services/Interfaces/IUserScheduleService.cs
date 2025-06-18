using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IUserScheduleService
    {
        Task<List<UserScheduleDTO>?> GetAllAsync(string userId);
        Task<string> AddAsync(string userId, UserScheduleDTO entry);
        Task<string> UpdateAsync(string userId, UserScheduleDTO oldEntry, UserScheduleDTO newEntry);
        Task<string> DeleteAsync(string userId, UserScheduleDTO entry);
    }
}
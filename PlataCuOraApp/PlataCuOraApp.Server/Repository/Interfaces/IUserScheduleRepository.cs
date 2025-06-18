using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Repository.Interfaces
{
    public interface IUserScheduleRepository
    {
        Task<List<UserScheduleDTO>?> GetAllAsync(string userId);
        Task<bool> AddAsync(string userId, UserScheduleDTO entry);
        Task<bool> UpdateAsync(string userId, UserScheduleDTO oldEntry, UserScheduleDTO newEntry);
        Task<bool> DeleteAsync(string userId, UserScheduleDTO entry);
    }
}

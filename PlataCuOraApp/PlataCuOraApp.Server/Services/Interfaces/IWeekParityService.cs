using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Services
{
    public interface IWeekParityService
    {
        Task AddOrUpdateWeekParityAsync(string userId, List<WeekParityDTO> weeks);
        Task<List<WeekParityDTO>> GetWeekParityAsync(string parId);
        Task<bool> UpdateParityAsync(string userId, WeekParityDTO oldEntry, WeekParityDTO newEntry);
        Task<bool> DeleteParityAsync(string userId, WeekParityDTO entry);
    }
}

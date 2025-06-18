using System.Collections.Generic;
using System.Threading.Tasks;
using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Repositories
{
    public interface IWeekParityRepository
    {
        Task AddOrUpdateParitateSaptAsync(string userId, List<WeekParityDTO> saptamani);
        Task<List<WeekParityDTO>> GetWeekParityAsync(string userId);
        Task<bool> UpdateParitateAsync(string userId, WeekParityDTO oldEntry, WeekParityDTO newEntry);
        Task<bool> DeleteParitateAsync(string userId, WeekParityDTO entry);
    }
}
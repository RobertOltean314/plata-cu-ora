using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Services;

namespace PlataCuOraApp.Server.Services
{
    public class WeekParityService : IWeekParityService
    {
        private readonly IWeekParityRepository _weekParityRepository;

        public WeekParityService(IWeekParityRepository weekParityRepository)
        {
            _weekParityRepository = weekParityRepository;
        }

        public async Task AddOrUpdateWeekParityAsync(string userId, List<WeekParityDTO> weeks)
        {
            await _weekParityRepository.AddOrUpdateParitateSaptAsync(userId, weeks);
        }

        public async Task<List<WeekParityDTO>> GetWeekParityAsync(string userId)
        {
            return await _weekParityRepository.GetWeekParityAsync(userId);
        }

        public async Task<bool> UpdateParityAsync(string userId, WeekParityDTO oldEntry, WeekParityDTO newEntry)
        {
            return await _weekParityRepository.UpdateParitateAsync(userId, oldEntry, newEntry);
        }

        public async Task<bool> DeleteParityAsync(string userId, WeekParityDTO entry)
        {
            return await _weekParityRepository.DeleteParitateAsync(userId, entry);
        }
    }
}

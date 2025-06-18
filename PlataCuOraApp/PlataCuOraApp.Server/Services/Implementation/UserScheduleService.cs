using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Implementation
{
    public class UserScheduleService : IUserScheduleService
    {
        private readonly IUserScheduleRepository _userScheduleRepository;
        private readonly ILogger<UserScheduleService> _logger;

        public UserScheduleService(IUserScheduleRepository userScheduleRepository, ILogger<UserScheduleService> logger)
        {
            _userScheduleRepository = userScheduleRepository;
            _logger = logger;
        }

        public async Task<List<UserScheduleDTO>?> GetAllAsync(string userId)
        {
            _logger.LogInformation($"Fetching schedule for user: {userId}");
            return await _userScheduleRepository.GetAllAsync(userId);
        }

        public async Task<string> AddAsync(string userId, UserScheduleDTO entry)
        {
            _logger.LogInformation($"Adding schedule entry for user: {userId}");
            var result = await _userScheduleRepository.AddAsync(userId, entry);
            return result ? "Successfully added." : "Entry already exists. No changes made.";
        }

        public async Task<string> UpdateAsync(string userId, UserScheduleDTO oldEntry, UserScheduleDTO newEntry)
        {
            _logger.LogInformation($"Updating schedule entry for user: {userId}");
            var result = await _userScheduleRepository.UpdateAsync(userId, oldEntry, newEntry);
            return result ? "Successfully updated." : "Entry not found for update.";
        }

        public async Task<string> DeleteAsync(string userId, UserScheduleDTO entry)
        {
            _logger.LogInformation($"Deleting schedule entry for user: {userId}");
            var result = await _userScheduleRepository.DeleteAsync(userId, entry);
            return result ? "Successfully deleted." : "Entry not found for deletion.";
        }
    }
}

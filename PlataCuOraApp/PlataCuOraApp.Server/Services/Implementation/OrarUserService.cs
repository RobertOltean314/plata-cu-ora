using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Implementation
{
    public class OrarUserService : IOrarUserService
    {
        private readonly IOrarUserRepository _repo;
        private readonly ILogger<OrarUserService> _logger;

        public OrarUserService(IOrarUserRepository repo, ILogger<OrarUserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<OrarUserDTO>?> GetAllAsync(string userId)
        {
            _logger.LogInformation($"Fetching schedule for user: {userId}");
            return await _repo.GetAllAsync(userId);
        }

        public async Task<string> AddAsync(string userId, OrarUserDTO entry)
        {
            _logger.LogInformation($"Adding schedule entry for user: {userId}");
            var result = await _repo.AddAsync(userId, entry);
            return result ? "Successfully added." : "Entry already exists. No changes made.";
        }

        public async Task<string> UpdateAsync(string userId, OrarUserDTO oldEntry, OrarUserDTO newEntry)
        {
            _logger.LogInformation($"Updating schedule entry for user: {userId}");
            var result = await _repo.UpdateAsync(userId, oldEntry, newEntry);
            return result ? "Successfully updated." : "Entry not found for update.";
        }

        public async Task<string> DeleteAsync(string userId, OrarUserDTO entry)
        {
            _logger.LogInformation($"Deleting schedule entry for user: {userId}");
            var result = await _repo.DeleteAsync(userId, entry);
            return result ? "Successfully deleted." : "Entry not found for deletion.";
        }
    }
}

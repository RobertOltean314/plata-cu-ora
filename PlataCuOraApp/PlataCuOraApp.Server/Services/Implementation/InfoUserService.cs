using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Implementation
{
    public class InfoUserService : IInfoUserService
    {
        private readonly IInfoUserRepository _repo;
        private readonly ILogger<InfoUserService> _logger;

        public InfoUserService(IInfoUserRepository repo, ILogger<InfoUserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<InfoUserDTO>> GetAllInfoAsync(string userId)
        {
            _logger.LogInformation($"Fetching all info for user {userId}");
            return await _repo.GetAllInfoAsync(userId);
        }

        public async Task<(bool success, string error)> AddInfoAsync(string userId, InfoUserDTO newInfo)
        {
            _logger.LogInformation($"Adding new info for user {userId}");
            var result = await _repo.AddInfoAsync(userId, newInfo);
            if (result)
                return (true, null);
            else
                return (false, "Duplicate info entry. No changes made.");
        }

        public async Task<(bool success, string error)> UpdateInfoAsync(string userId, InfoUserDTO oldInfo, InfoUserDTO newInfo)
        {
            _logger.LogInformation($"Updating info for user {userId}");
            var result = await _repo.UpdateInfoAsync(userId, oldInfo, newInfo);
            if (result)
                return (true, null);
            else
                return (false, "Info entry to update not found.");
        }

        public async Task<(bool success, string error)> DeleteInfoAsync(string userId, InfoUserDTO info)
        {
            _logger.LogInformation($"Deleting info for user {userId}");
            var result = await _repo.DeleteInfoAsync(userId, info);
            if (result)
                return (true, null);
            else
                return (false, "Info entry to delete not found.");
        }

        public async Task<(bool success, string error)> SetActiveAsync(string userId, InfoUserDTO activeInfo)
        {
            _logger.LogInformation($"Setting active info for user {userId}");
            var result = await _repo.SetActiveAsync(userId, activeInfo);
            if (result)
                return (true, null);
            else
                return (false, "Info entry to set active not found.");
        }

        public async Task<(bool success, string error)> UnsetActiveAsync(string userId, InfoUserDTO activeInfo)
        {
            _logger.LogInformation($"Unsetting active info for user {userId}");
            var result = await _repo.UnsetActiveAsync(userId, activeInfo);
            if (result)
                return (true, null);
            else
                return (false, "Failed to unset active info.");
        }

        public async Task<InfoUserDTO?> AddActiveInfoToDbAsync(string userId)
        {
            return await _repo.AddActiveInfoToDbAsync(userId);
        }

        public async Task<InfoUserDTO?> GetInfoUserFromDbAsync(string userId)
        {
            return await _repo.GetInfoUserFromDbAsync(userId);
        }

        public async Task<InfoUserDTO?> GetActiveInfoAsync(string userId)
        {
            return await _repo.GetActiveInfoAsync(userId);
        }


    }
}

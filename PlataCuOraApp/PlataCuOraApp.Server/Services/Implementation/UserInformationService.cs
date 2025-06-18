using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Implementation
{
    public class UserInformationService : IUserInformationService
    {
        private readonly IUserInformationRepository _userInformationRepository;
        private readonly ILogger<UserInformationService> _logger;

        public UserInformationService(IUserInformationRepository userInformationRepository, ILogger<UserInformationService> logger)
        {
            _userInformationRepository = userInformationRepository;
            _logger = logger;
        }

        public async Task<List<UserInformationDTO>> GetAllInfoAsync(string userId)
        {
            _logger.LogInformation($"Fetching all info for user {userId}");
            return await _userInformationRepository.GetAllInfoAsync(userId);
        }

        public async Task<(bool success, string error)> AddInfoAsync(string userId, UserInformationDTO newInfo)
        {
            _logger.LogInformation($"Adding new info for user {userId}");
            var result = await _userInformationRepository.AddInfoAsync(userId, newInfo);
            if (result)
                return (true, null);
            else
                return (false, "Duplicate info entry. No changes made.");
        }

        public async Task<(bool success, string error)> UpdateInfoAsync(string userId, UserInformationDTO oldInfo, UserInformationDTO newInfo)
        {
            _logger.LogInformation($"Updating info for user {userId}");
            var result = await _userInformationRepository.UpdateInfoAsync(userId, oldInfo, newInfo);
            if (result)
                return (true, null);
            else
                return (false, "Info entry to update not found.");
        }

        public async Task<(bool success, string error)> DeleteInfoAsync(string userId, UserInformationDTO info)
        {
            _logger.LogInformation($"Deleting info for user {userId}");
            var result = await _userInformationRepository.DeleteInfoAsync(userId, info);
            if (result)
                return (true, null);
            else
                return (false, "Info entry to delete not found.");
        }

        public async Task<(bool success, string error)> SetActiveAsync(string userId, UserInformationDTO activeInfo)
        {
            _logger.LogInformation($"Setting active info for user {userId}");
            var result = await _userInformationRepository.SetActiveAsync(userId, activeInfo);
            if (result)
                return (true, null);
            else
                return (false, "Info entry to set active not found.");
        }

        public async Task<(bool success, string error)> UnsetActiveAsync(string userId, UserInformationDTO activeInfo)
        {
            _logger.LogInformation($"Unsetting active info for user {userId}");
            var result = await _userInformationRepository.UnsetActiveAsync(userId, activeInfo);
            if (result)
                return (true, null);
            else
                return (false, "Failed to unset active info.");
        }

        public async Task<UserInformationDTO?> AddActiveInfoToDbAsync(string userId)
        {
            return await _userInformationRepository.AddActiveInfoToDbAsync(userId);
        }

        public async Task<UserInformationDTO?> GetInfoUserFromDbAsync(string userId)
        {
            return await _userInformationRepository.GetInfoUserFromDbAsync(userId);
        }

        public async Task<UserInformationDTO?> GetActiveInfoAsync(string userId)
        {
            return await _userInformationRepository.GetActiveInfoAsync(userId);
        }


    }
}

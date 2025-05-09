using PlataCuOra.Server.Domain.DTOs;
using PlataCuOra.Server.Domain.Entities;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;
using System.Threading.Tasks;

namespace PlataCuOra.Server.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserDTO?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName
            };
        }

        public async Task<bool> UpdateUserAsync(string userId, UserDTO userDto)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(userId);

                if (existingUser == null)
                    return false;

                existingUser.DisplayName = userDto.DisplayName;

                return await _userRepository.UpdateUserAsync(existingUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return false;
            }
        }
    }
}
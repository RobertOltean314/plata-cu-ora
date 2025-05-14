using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Domain.Entities;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Implementation;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Services.Implementation
{
    public class InfoUserService : IInfoUserService
    {
        private readonly IInfoUserRepository _userRepository;
        private readonly ILogger<InfoUserService> _logger;

        public InfoUserService(IInfoUserRepository userRepository, ILogger<InfoUserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<InfoUserDTO?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
                return null;

            return new InfoUserDTO
            {
                Declarant = user.Declarant,
                Tip = user.Tip,
                DirectorDepartament = user.DirectorDepartament,
                Decan = user.Decan,
                Universitate = user.Universitate,
                Facultate = user.Facultate,
                Departament = user.Departament
            };
        }

        public async Task<(bool success, string? error)> UpdateUserAsync(string userId, InfoUserDTO userDto)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(userId);

                if (existingUser == null)
                {
                    var newUser = new InfoUser
                    {
                        Id = userId,
                        Declarant = userDto.Declarant,
                        Tip = userDto.Tip,
                        DirectorDepartament = userDto.DirectorDepartament,
                        Decan = userDto.Decan,
                        Universitate = userDto.Universitate,
                        Facultate = userDto.Facultate,
                        Departament = userDto.Departament
                    };

                    await _userRepository.CreateUserAsync(newUser);
                }
                else
                {
                    existingUser.Declarant = userDto.Declarant;
                    existingUser.Tip = userDto.Tip;
                    existingUser.DirectorDepartament = userDto.DirectorDepartament;
                    existingUser.Decan = userDto.Decan;
                    existingUser.Universitate = userDto.Universitate;
                    existingUser.Facultate = userDto.Facultate;
                    existingUser.Departament = userDto.Departament;

                    await _userRepository.UpdateUserAsync(existingUser);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating info user {UserId}", userId);
                return (false, ex.Message);
            }
        }

    }
}

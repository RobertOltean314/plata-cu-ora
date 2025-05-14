using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IInfoUserService
    {
        Task<InfoUserDTO?> GetUserByIdAsync(string userId);
        Task<(bool success, string? error)> UpdateUserAsync(string userId, InfoUserDTO userDto);
    }
}

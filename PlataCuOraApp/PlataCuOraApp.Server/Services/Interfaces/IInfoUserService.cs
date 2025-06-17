using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IInfoUserService
    {
        Task<List<InfoUserDTO>> GetAllInfoAsync(string userId);
        Task<InfoUserDTO?> AddActiveInfoToDbAsync(string userId);
        Task<InfoUserDTO?> GetInfoUserFromDbAsync(string userId);
        Task<InfoUserDTO?> GetActiveInfoAsync(string userId);
        Task<(bool success, string error)> AddInfoAsync(string userId, InfoUserDTO info);
        Task<(bool success, string error)> UpdateInfoAsync(string userId, InfoUserDTO oldInfo, InfoUserDTO newInfo);
        Task<(bool success, string error)> DeleteInfoAsync(string userId, InfoUserDTO info);
        Task<(bool success, string error)> SetActiveAsync(string userId, InfoUserDTO info);
        Task<(bool success, string error)> UnsetActiveAsync(string userId, InfoUserDTO info);

    }
}

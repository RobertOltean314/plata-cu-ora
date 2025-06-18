using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IUserInformationService
    {
        Task<List<UserInformationDTO>> GetAllInfoAsync(string userId);
        Task<UserInformationDTO?> AddActiveInfoToDbAsync(string userId);
        Task<UserInformationDTO?> GetInfoUserFromDbAsync(string userId);
        Task<UserInformationDTO?> GetActiveInfoAsync(string userId);
        Task<(bool success, string error)> AddInfoAsync(string userId, UserInformationDTO info);
        Task<(bool success, string error)> UpdateInfoAsync(string userId, UserInformationDTO oldInfo, UserInformationDTO newInfo);
        Task<(bool success, string error)> DeleteInfoAsync(string userId, UserInformationDTO info);
        Task<(bool success, string error)> SetActiveAsync(string userId, UserInformationDTO info);
        Task<(bool success, string error)> UnsetActiveAsync(string userId, UserInformationDTO info);

    }
}

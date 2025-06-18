using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Repository.Interfaces
{
    public interface IUserInformationRepository
    {
        // returneaza toate info profile
        Task<List<UserInformationDTO>> GetAllInfoAsync(string userId);

        // face update la toata lista
        Task<bool> UpdateAllInfoAsync(string userId, List<UserInformationDTO> infoList);

        // adauga in infoUsers cand isActive==true
        Task<UserInformationDTO?> AddActiveInfoToDbAsync(string userId);
        // ea din baza de date infoUsers
        Task<UserInformationDTO?> GetInfoUserFromDbAsync(string userId);
        // ia din lista din baza de date infosUsers entitatea care are isActive== true
        Task<UserInformationDTO?> GetActiveInfoAsync(string userId);

        // adauga obiect in lista
        Task<bool> AddInfoAsync(string userId, UserInformationDTO newInfo);

        // Editează un obiect din lista info, cu două intrări: old și new
        Task<bool> UpdateInfoAsync(string userId, UserInformationDTO oldInfo, UserInformationDTO newInfo);

        // sterge un obiect din lista info
        Task<bool> DeleteInfoAsync(string userId, UserInformationDTO info);

        // seteaza un obiect ca fiind activ (isActive== true)
        Task<bool> SetActiveAsync(string userId, UserInformationDTO activeInfo);

        // seteaza un obiect ca fiind inactiv (isActive== false)
        Task<bool> UnsetActiveAsync(string userId, UserInformationDTO activeInfo);

    }
}

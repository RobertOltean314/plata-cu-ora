using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Repository.Interfaces
{
    public interface IInfoUserRepository
    {
        // returneaza toate info profile
        Task<List<InfoUserDTO>> GetAllInfoAsync(string userId);

        // face update la toata lista
        Task<bool> UpdateAllInfoAsync(string userId, List<InfoUserDTO> infoList);

        // adauga in infoUsers cand isActive==true
        Task<InfoUserDTO?> AddActiveInfoToDbAsync(string userId);
        // ea din baza de date infoUsers
        Task<InfoUserDTO?> GetInfoUserFromDbAsync(string userId);
        // ia din lista din baza de date infosUsers entitatea care are isActive== true
        Task<InfoUserDTO?> GetActiveInfoAsync(string userId);

        // adauga obiect in lista
        Task<bool> AddInfoAsync(string userId, InfoUserDTO newInfo);

        // Editează un obiect din lista info, cu două intrări: old și new
        Task<bool> UpdateInfoAsync(string userId, InfoUserDTO oldInfo, InfoUserDTO newInfo);

        // sterge un obiect din lista info
        Task<bool> DeleteInfoAsync(string userId, InfoUserDTO info);

        // seteaza un obiect ca fiind activ (isActive== true)
        Task<bool> SetActiveAsync(string userId, InfoUserDTO activeInfo);

        // seteaza un obiect ca fiind inactiv (isActive== false)
        Task<bool> UnsetActiveAsync(string userId, InfoUserDTO activeInfo);

    }
}

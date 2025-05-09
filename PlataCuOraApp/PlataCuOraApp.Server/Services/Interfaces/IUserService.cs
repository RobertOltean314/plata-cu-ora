using PlataCuOra.Server.Domain.DTOs;
using System.Threading.Tasks;

namespace PlataCuOra.Server.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(string userId, UserDTO userDto);
    }
}
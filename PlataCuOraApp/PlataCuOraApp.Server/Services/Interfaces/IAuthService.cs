using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOra.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string token, object userData, string error)> LoginUserAsync(LoginRequestDTO request);
        Task<(bool success, string error)> RegisterUserAsync(RegisterRequestDTO request);
        Task<bool> VerifyTokenAsync(string token); 
    }
}
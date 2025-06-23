using FirebaseAdmin.Auth;
using PlataCuOra.Server.Domain.DTOs;
using PlataCuOraApp.Server.Domain.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IFirebaseAuthWrapper
    {
        Task<DecodedTokenDTO> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
        Task<UserDTO> GetUserAsync(string uid, CancellationToken cancellationToken = default);
        Task<UserDTO> CreateUserAsync(UserRecordArgs args, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(string uid);
    }
}
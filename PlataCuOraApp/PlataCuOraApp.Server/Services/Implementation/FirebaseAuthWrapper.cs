using FirebaseAdmin.Auth;
using PlataCuOra.Server.Domain.DTOs;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PlataCuOraApp.Server.Services.Implementation
{
    public class FirebaseAuthWrapper : IFirebaseAuthWrapper
    {
        private UserDTO MapUserRecordToUserDTO(UserRecord userRecord)
        {
            if (userRecord == null) return null;
            return new UserDTO
            {
                Id = userRecord.Uid,
                Email = userRecord.Email,
                DisplayName = userRecord.DisplayName
                // Adaugă alte câmpuri necesare
            };
        }

        public async Task<DecodedTokenDTO> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
        {
            FirebaseToken firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken, cancellationToken);
            return new DecodedTokenDTO
            {
                Uid = firebaseToken.Uid
            };
        }

        public async Task<UserDTO> GetUserAsync(string uid, CancellationToken cancellationToken = default)
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid, cancellationToken);
            return MapUserRecordToUserDTO(userRecord);
        }

        public async Task<UserDTO> CreateUserAsync(UserRecordArgs args, CancellationToken cancellationToken = default)
        {
            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args, cancellationToken);
            return MapUserRecordToUserDTO(userRecord);
        }

        public Task DeleteUserAsync(string uid)
            => FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
    }
}
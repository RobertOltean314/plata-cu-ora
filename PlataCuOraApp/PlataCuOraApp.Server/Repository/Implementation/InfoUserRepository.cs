using Google.Cloud.Firestore;
using PlataCuOra.Server.Domain.Entities;
using PlataCuOra.Server.Repository.Implementation;
using PlataCuOraApp.Server.Domain.Entities;
using PlataCuOraApp.Server.Repository.Interfaces;

namespace PlataCuOraApp.Server.Repository.Implementation
{
    public class InfoUserRepository : IInfoUserRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<UserRepository> _logger;
        private const string USERS_COLLECTION = "infoUsers";

        public InfoUserRepository(FirestoreDb firestoreDb, ILogger<UserRepository> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task<InfoUser?> GetUserByIdAsync(string userId)
        {
            try
            {
                var docSnapshot = await _firestoreDb.Collection(USERS_COLLECTION).Document(userId).GetSnapshotAsync();

                if (!docSnapshot.Exists)
                {
                    _logger.LogInformation($"User with ID {userId} not found.");
                    return null;
                }

                var userData = docSnapshot.ConvertTo<InfoUser>();
                userData.Id = userId;

                return userData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving info user with ID {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(InfoUser user)
        {
            try
            {
                var userData = new Dictionary<string, object>
                {
                    { "declarant", user.Declarant },
                    { "tip", user.Tip },
                    { "directorDepartament", user.DirectorDepartament },
                    { "decan", user.Decan },
                    { "universitate", user.Universitate },
                    { "facultate", user.Facultate },
                    { "departament", user.Departament }
                };

                await _firestoreDb.Collection(USERS_COLLECTION).Document(user.Id).SetAsync(userData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating info user in Firestore");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(InfoUser user)
        {
            try
            {
                var userData = new Dictionary<string, object>
                {
                    { "declarant", user.Declarant },
                    { "tip", user.Tip },
                    { "directorDepartament", user.DirectorDepartament },
                    { "decan", user.Decan },
                    { "universitate", user.Universitate },
                    { "facultate", user.Facultate },
                    { "departament", user.Departament }
                };

                await _firestoreDb.Collection(USERS_COLLECTION).Document(user.Id).UpdateAsync(userData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating info user in Firestore");
                return false;
            }
        }
    }
}

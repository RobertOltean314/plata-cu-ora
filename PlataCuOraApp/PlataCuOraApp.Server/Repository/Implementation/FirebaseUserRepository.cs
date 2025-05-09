using Google.Cloud.Firestore;
using PlataCuOra.Server.Domain.Entities;
using PlataCuOra.Server.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlataCuOra.Server.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<UserRepository> _logger;
        private const string USERS_COLLECTION = "users";

        public UserRepository(FirestoreDb firestoreDb, ILogger<UserRepository> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            try
            {
                var docSnapshot = await _firestoreDb.Collection(USERS_COLLECTION).Document(userId).GetSnapshotAsync();

                if (!docSnapshot.Exists)
                    return null;

                var userData = docSnapshot.ConvertTo<User>();
                userData.Id = userId;

                return userData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", userId);
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var query = _firestoreDb.Collection(USERS_COLLECTION).WhereEqualTo("email", email);
                var querySnapshot = await query.GetSnapshotAsync();

                if (querySnapshot.Count == 0)
                    return null;

                var document = querySnapshot.Documents[0];
                var user = document.ConvertTo<User>();
                user.Id = document.Id;

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with email {Email}", email);
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                var userData = new Dictionary<string, object>
                {
                    { "email", user.Email },
                    { "displayName", user.DisplayName },
                    { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
                };

                await _firestoreDb.Collection(USERS_COLLECTION).Document(user.Id).SetAsync(userData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in Firestore");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var userData = new Dictionary<string, object>
                {
                    { "email", user.Email },
                    { "displayName", user.DisplayName }
                };

                await _firestoreDb.Collection(USERS_COLLECTION).Document(user.Id).UpdateAsync(userData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user in Firestore");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                await _firestoreDb.Collection(USERS_COLLECTION).Document(userId).DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user from Firestore");
                return false;
            }
        }
    }
}
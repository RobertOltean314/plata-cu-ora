using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PlataCuOra.Server.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly FirebaseAuth _auth;
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(FirebaseAuth auth, FirestoreDb firestoreDb, ILogger<UserRepository> logger)
        {
            _auth = auth;
            _firestoreDb = firestoreDb;
            _logger = logger;
        }
        public async Task<bool> RegisterUserAsync(RegisterRequestDTO request)
        {
            try
            {
                var userRecord = await _auth.CreateUserAsync(new UserRecordArgs
                {
                    Email = request.Email,
                    Password = request.Password,
                    DisplayName = request.Name
                });
                var userDoc = new Dictionary<string, object>
               {
                   { "uid", userRecord.Uid },
                   { "name", request.Name },
                   { "email", request.Email },
                   { "created_at", Timestamp.GetCurrentTimestamp() }
               };
                await _firestoreDb.Collection("users").Document(userRecord.Uid).SetAsync(userDoc);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la înregistrarea utilizatorului.");
                //return false;
                throw;
            }
        }
        public async Task<User> CreateAsync(User user)
        {
            var userRecord = new UserRecordArgs()
            {
                Uid = user.Id.ToString(),
                DisplayName = user.Name,
                Email = user.Email,
                Password = user.Password,
            };
            var createdUser = await _auth.CreateUserAsync(userRecord);
            var userDoc = new Dictionary<string, object>
           {
               { "uid", userRecord.Uid },
               { "name", user.Name },
               { "email", user.Email },
               { "created_at", Timestamp.GetCurrentTimestamp() }
           };
            await _firestoreDb.Collection("users").Document(userRecord.Uid).SetAsync(userDoc);
            user.Id = int.Parse(userRecord.Uid);
            return user;
        }
        public Task<User> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<User>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
        public Task<User?> GetById(int id)
        {
            throw new NotImplementedException();
        }
        public Task<User?> GetByUsername(string username)
        {
            throw new NotImplementedException();
        }
        public Task<User?> UpdateAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
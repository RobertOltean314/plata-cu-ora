using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using PlataCuOra.Server.Domain;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
namespace PlataCuOra.Server.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly FirebaseAuth _auth;
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<UserRepository> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseApiKey = "AIzaSyCI2nSWbRAhK8lZh69a2dO55G9-yphxkOI";
        public UserRepository(FirebaseAuth auth, FirestoreDb firestoreDb, ILogger<UserRepository> logger, HttpClient httpClient)
        {
            _auth = auth;
            _firestoreDb = firestoreDb;
            _logger = logger;
            _httpClient = httpClient;
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
                _logger.LogError(ex, "Error registering user.");
                //return false;
                throw;
            }
        }

        public async Task<(bool success, string? token, Dictionary<string, object>? userData, string? error)> LoginUserAsync(LoginRequestDTO request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}",
                    new
                    {
                        email = request.Email,
                        password = request.Password,
                        returnSecureToken = true
                    });
                if (!response.IsSuccessStatusCode)
                    return (false, null, null, "Invalid email or password.");
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                if (result == null || !result.ContainsKey("idToken") || !result.ContainsKey("localId"))
                    return (false, null, null, "Invalid response from Firebase.");
                var userId = result["localId"].ToString();
                var docSnapshot = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
                if (!docSnapshot.Exists)
                    return (false, null, null, "User not found in Firestore.");
                return (true, result["idToken"].ToString(), docSnapshot.ToDictionary(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                return (false, null, null, "Unexpected error occurred.");
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
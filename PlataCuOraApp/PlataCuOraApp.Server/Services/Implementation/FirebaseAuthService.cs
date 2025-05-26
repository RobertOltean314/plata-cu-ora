using FirebaseAdmin;
using FirebaseAdmin.Auth; 
using PlataCuOra.Server.Domain.DTOs;
using PlataCuOra.Server.Domain.Entities;
using PlataCuOra.Server.Infrastructure.Firebase;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace PlataCuOra.Server.Services.Implementation
{
    public class FirebaseAuthService : IAuthService
    {
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IUserRepository _userRepository;
        private readonly IFirebaseConfig _firebaseConfig;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FirebaseAuthService> _logger;

        public FirebaseAuthService(
            FirebaseAuth firebaseAuth,
            IUserRepository userRepository,
            IFirebaseConfig firebaseConfig,
            HttpClient httpClient,
            ILogger<FirebaseAuthService> logger)
        {
            _firebaseAuth = firebaseAuth;
            _userRepository = userRepository;
            _firebaseConfig = firebaseConfig;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(bool success, string token, object userData, string error)> LoginUserAsync(LoginRequestDTO request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseConfig.ApiKey}",
                    new
                    {
                        email = request.Email,
                        password = request.Password,
                        returnSecureToken = true
                    });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Firebase auth failed: {ErrorContent}", errorContent);
                    return (false, string.Empty, new object(), "Invalid email or password.");
                }

                var authResult = await response.Content.ReadFromJsonAsync<JsonElement>();
                var idToken = authResult.GetProperty("idToken").GetString();
                var localId = authResult.GetProperty("localId").GetString();

                if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(localId))
                    return (false, string.Empty, new object(), "Invalid response from Firebase.");

                // Get user from repository
                var user = await _userRepository.GetUserByIdAsync(localId);

                if (user == null)
                {
                    _logger.LogWarning("User authenticated with Firebase but not found in database. Creating user record.");

                    // Create the user in our database if they don't exist
                    user = new User
                    {
                        Id = localId,
                        Email = request.Email,
                        DisplayName = authResult.GetProperty("displayName").GetString() ?? request.Email
                    };

                    await _userRepository.CreateUserAsync(user);
                }

                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                };

                return (true, idToken, userDto, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return (false, string.Empty, new object(), "An unexpected error occurred.");
            }
        }

        public async Task<(bool success, string error)> RegisterUserAsync(RegisterRequestDTO request)
        {
            try
            {
                // Create user with Firebase Auth
                var userRecord = await _firebaseAuth.CreateUserAsync(new UserRecordArgs
                {
                    Email = request.Email,
                    Password = request.Password,
                    DisplayName = request.Name
                });

                // Create user in our database
                var user = new User
                {
                    Id = userRecord.Uid,
                    Email = request.Email,
                    DisplayName = request.Name,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _userRepository.CreateUserAsync(user);

                if (!success)
                {
                    _logger.LogWarning("User created in Firebase but failed to save in database. User ID: {UserId}", userRecord.Uid);
                    await _firebaseAuth.DeleteUserAsync(userRecord.Uid);
                    return (false, "Failed to create user in database.");
                }

                return (true, string.Empty);
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogWarning(ex, "Firebase auth error during registration");
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                return decodedToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token verification failed");
                return false;
            }
        }

        public async Task<(bool Success, string? Token, UserDTO? User, string? Error)> LoginWithGoogleAsync(string idToken)
        {
            try
            {
                // Verify the ID token sent from frontend
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                string uid = decodedToken.Uid;

                // Get user details from Firebase Authentication
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

                var user = await _userRepository.GetUserByIdAsync(uid);

                if (user == null)
                {
                    // Create user in Firestore if not exists
                    user = new User
                    {
                        Id = uid,
                        Email = firebaseUser.Email,
                        DisplayName = firebaseUser.DisplayName ?? "NoName"
                    };

                    await _userRepository.CreateUserAsync(user);
                }

                // Map to DTO
                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                };

                return (true, idToken, userDto, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Login failed: {Message}", ex.Message);
                return (false, null, null, $"Invalid token or error during Google login: {ex.Message}");
            }
        }

    }
}
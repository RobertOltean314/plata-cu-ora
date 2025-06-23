using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using PlataCuOra.Server.Domain.DTOs;
using PlataCuOra.Server.Domain.Entities;
using PlataCuOra.Server.Infrastructure.Firebase;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlataCuOra.Server.Services.Implementation
{
    public class FirebaseAuthService : IAuthService
    {
        private readonly IFirebaseAuthWrapper _firebaseAuth;
        private readonly IUserRepository _userRepository;
        private readonly IFirebaseConfig _firebaseConfig;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FirebaseAuthService> _logger;

        public FirebaseAuthService(
            IFirebaseAuthWrapper firebaseAuth,
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

                var user = await _userRepository.GetUserByIdAsync(localId);

                if (user == null)
                {
                    _logger.LogWarning("User authenticated with Firebase but not found in database. Creating user record.");

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
                var userDto = await _firebaseAuth.CreateUserAsync(new UserRecordArgs
                {
                    Email = request.Email,
                    Password = request.Password,
                    DisplayName = request.Name
                });

                var user = new User
                {
                    Id = userDto.Id,
                    Email = request.Email,
                    DisplayName = request.Name,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _userRepository.CreateUserAsync(user);

                if (!success)
                {
                    _logger.LogWarning("User created in Firebase but failed to save in database. User ID: {UserId}", userDto.Id);
                    await _firebaseAuth.DeleteUserAsync(userDto.Id);
                    return (false, "Failed to create user in database.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase auth error during registration");
                return (false, ex.Message);
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
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;

                var userDto = await _firebaseAuth.GetUserAsync(uid);
                var user = await _userRepository.GetUserByIdAsync(uid);

                if (user == null)
                {
                    user = new User
                    {
                        Id = uid,
                        Email = userDto.Email,
                        DisplayName = userDto.DisplayName ?? "NoName"
                    };

                    await _userRepository.CreateUserAsync(user);
                }

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

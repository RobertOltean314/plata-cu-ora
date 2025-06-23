using Xunit;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataCuOra.Server.Services.Implementation;
using PlataCuOra.Server.Domain.DTOs;
using PlataCuOra.Server.Domain.Entities;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Services.Interfaces;
using PlataCuOra.Server.Infrastructure.Firebase;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;
using FirebaseAdmin.Auth;

namespace PlataCuOraApp.Tests.Services
{
    public class FirebaseAuthServiceTests
    {
        private readonly Mock<IFirebaseAuthWrapper> _mockFirebaseAuth;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IFirebaseConfig> _mockFirebaseConfig;
        private readonly Mock<ILogger<FirebaseAuthService>> _mockLogger;
        private readonly HttpClient _httpClient;

        public FirebaseAuthServiceTests()
        {
            _mockFirebaseAuth = new Mock<IFirebaseAuthWrapper>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockFirebaseConfig = new Mock<IFirebaseConfig>();
            _mockLogger = new Mock<ILogger<FirebaseAuthService>>();

            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "{\"idToken\":\"fake-token\",\"localId\":\"localId123\"}");
            _httpClient = new HttpClient(handler);
        }

        [Fact]
        public async Task LoginUserAsync_ValidCredentials_ReturnsSuccess()
        {
            _mockUserRepository.Setup(r => r.GetUserByIdAsync("localId123"))
                .ReturnsAsync(new User
                {
                    Id = "localId123",
                    Email = "test@example.com",
                    DisplayName = "Test User"
                });

            var service = new FirebaseAuthService(null!, _mockUserRepository.Object, _mockFirebaseConfig.Object, _httpClient, _mockLogger.Object);

            var request = new LoginRequestDTO
            {
                Email = "test@example.com",
                Password = "correct-password"
            };

            var result = await service.LoginUserAsync(request);

            Assert.True(result.success);
            Assert.Equal("fake-token", result.token);
            Assert.NotNull(result.userData);
            var user = (UserDTO)result.userData;
            Assert.Equal("test@example.com", user.Email);
        }

        [Fact]
        public async Task LoginUserAsync_InvalidCredentials_ReturnsError()
        {
            var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, "{\"error\": {\"message\": \"INVALID_PASSWORD\"}}");
            var httpClient = new HttpClient(handler);

            var service = new FirebaseAuthService(null!, _mockUserRepository.Object, _mockFirebaseConfig.Object, httpClient, _mockLogger.Object);

            var request = new LoginRequestDTO
            {
                Email = "wrong@example.com",
                Password = "wrong-password"
            };

            var result = await service.LoginUserAsync(request);

            Assert.False(result.success);
            Assert.Contains("Invalid email or password", result.error);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidInput_ReturnsSuccess()
        {
            var mockUserDto = new UserDTO { Id = "new-uid" };

            _mockFirebaseAuth
                .Setup(f => f.CreateUserAsync(It.IsAny<UserRecordArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUserDto);

            _mockUserRepository
                .Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            var service = new FirebaseAuthService(
                _mockFirebaseAuth.Object,
                _mockUserRepository.Object,
                _mockFirebaseConfig.Object,
                _httpClient,
                _mockLogger.Object);

            var request = new RegisterRequestDTO
            {
                Email = "new@example.com",
                Password = "123456",
                Name = "Test Name"
            };

            var result = await service.RegisterUserAsync(request);

            Assert.True(result.success);
            Assert.Equal(string.Empty, result.error);
        }

        [Fact]
        public async Task RegisterUserAsync_EmailExists_ReturnsError()
        {
            _mockFirebaseAuth
                .Setup(f => f.CreateUserAsync(It.IsAny<UserRecordArgs>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Email already exists"));

            var service = new FirebaseAuthService(
                _mockFirebaseAuth.Object,
                _mockUserRepository.Object,
                _mockFirebaseConfig.Object,
                _httpClient,
                _mockLogger.Object);

            var request = new RegisterRequestDTO
            {
                Email = "existing@example.com",
                Password = "123456",
                Name = "Existing User"
            };

            var result = await service.RegisterUserAsync(request);

            Assert.False(result.success);
            Assert.Contains("Email already exists", result.error);
        }

        [Fact]
        public async Task RegisterUserAsync_DatabaseSaveFails_DeletesFirebaseUserAndReturnsError()
        {
            // Arrange
            var mockUserDto = new UserDTO { Id = "new-uid" };

            _mockFirebaseAuth
                .Setup(f => f.CreateUserAsync(It.IsAny<UserRecordArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUserDto);

            _mockUserRepository
                .Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(false);

            _mockFirebaseAuth
                .Setup(f => f.DeleteUserAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new FirebaseAuthService(
                _mockFirebaseAuth.Object,
                _mockUserRepository.Object,
                _mockFirebaseConfig.Object,
                _httpClient,
                _mockLogger.Object);

            var request = new RegisterRequestDTO
            {
                Email = "new@example.com",
                Password = "123456",
                Name = "Test Name"
            };

            // Act
            var result = await service.RegisterUserAsync(request);

            // Assert
            Assert.False(result.success);
            Assert.Equal("Failed to create user in database.", result.error);

            // Verificăm că DeleteUserAsync a fost apelat o dată cu ID-ul nou creat
            _mockFirebaseAuth.Verify(f => f.DeleteUserAsync("new-uid"), Times.Once);
        }

        [Fact]
        public async Task LoginWithGoogleAsync_ValidToken_ReturnsSuccess()
        {
            // Arrange
            var decodedTokenDto = new DecodedTokenDTO { Uid = "google-uid" };

            _mockFirebaseAuth.Setup(f => f.VerifyIdTokenAsync("valid-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(decodedTokenDto);

            // Folosim un obiect real UserDTO, nu un mock
            var userRecord = new UserDTO
            {
                Id = "google-uid",
                Email = "googleuser@example.com",
                DisplayName = "Google User"
            };

            _mockFirebaseAuth.Setup(f => f.GetUserAsync("google-uid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRecord);

            _mockUserRepository.Setup(r => r.GetUserByIdAsync("google-uid")).ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            var service = new FirebaseAuthService(
                _mockFirebaseAuth.Object,
                _mockUserRepository.Object,
                _mockFirebaseConfig.Object,
                _httpClient,
                _mockLogger.Object);

            // Act
            var result = await service.LoginWithGoogleAsync("valid-token");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("valid-token", result.Token);
            Assert.NotNull(result.User);
        }

        [Fact]
        public async Task LoginWithGoogleAsync_InvalidToken_ReturnsError()
        {
            _mockFirebaseAuth.Setup(f => f.VerifyIdTokenAsync("invalid-token", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("Invalid token"));

            var service = new FirebaseAuthService(_mockFirebaseAuth.Object, _mockUserRepository.Object, _mockFirebaseConfig.Object, _httpClient, _mockLogger.Object);

            var result = await service.LoginWithGoogleAsync("invalid-token");

            Assert.False(result.Success);
            Assert.Contains("Invalid token", result.Error);
        }
    }

    // Helper mock HttpMessageHandler for HttpClient in tests
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent)
            };
            return Task.FromResult(response);
        }
    }
}

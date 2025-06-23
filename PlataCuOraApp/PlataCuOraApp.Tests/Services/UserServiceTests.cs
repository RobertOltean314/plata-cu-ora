using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PlataCuOra.Server.Services.Implementation;
using PlataCuOra.Server.Repository.Interface;
using PlataCuOra.Server.Domain.DTOs;
using PlataCuOra.Server.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PlataCuOra.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository = new();
        private readonly Mock<ILogger<UserService>> _mockLogger = new();
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(_mockRepository.Object, _mockLogger.Object);
        }

        private User CreateSampleUser(string id = "user1") => new()
        {
            Id = id,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        private UserDTO CreateSampleUserDTO(string id = "user1") => new()
        {
            Id = id,
            Email = "test@example.com",
            DisplayName = "Updated Name",
            Role = "Admin"
        };

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = CreateSampleUser();
            _mockRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.DisplayName, result.DisplayName);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetUserByIdAsync("unknown")).ReturnsAsync((User?)null);

            // Act
            var result = await _service.GetUserByIdAsync("unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsTrue_WhenUserExistsAndUpdated()
        {
            // Arrange
            var user = CreateSampleUser();
            var dto = CreateSampleUserDTO(user.Id);

            _mockRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.UpdateUserAsync(It.Is<User>(u => u.DisplayName == dto.DisplayName)))
                     .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateUserAsync(user.Id, dto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var dto = CreateSampleUserDTO("missing");

            _mockRepository.Setup(r => r.GetUserByIdAsync("missing")).ReturnsAsync((User?)null);

            // Act
            var result = await _service.UpdateUserAsync("missing", dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsFalse_WhenExceptionIsThrown()
        {
            // Arrange
            var user = CreateSampleUser();
            var dto = CreateSampleUserDTO(user.Id);

            _mockRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                     .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UpdateUserAsync(user.Id, dto);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

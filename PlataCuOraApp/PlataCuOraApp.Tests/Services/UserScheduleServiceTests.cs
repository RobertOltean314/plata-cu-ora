using Moq;
using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Services.Implementation;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Tests.Services
{
    public class UserScheduleServiceTests
    {
        private readonly Mock<IUserScheduleRepository> _mockRepository = new();
        private readonly Mock<ILogger<UserScheduleService>> _mockLogger = new();
        private readonly UserScheduleService _service;

        public UserScheduleServiceTests()
        {
            _service = new UserScheduleService(_mockRepository.Object, _mockLogger.Object);
        }

        private UserScheduleDTO CreateSampleEntry()
        {
            return new UserScheduleDTO
            {
                NrPost = 1,
                DenPost = "Profesor",
                OreCurs = 10,
                OreSem = 5,
                OreLab = 3,
                OreProi = 2,
                Tip = "Curs",
                Formatia = "Grupa A",
                Ziua = "Luni",
                ImparPar = "Impar",
                Materia = "Matematica",
                SaptamanaInceput = "1",
                TotalOre = 20
            };
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSchedules_WhenUserHasSchedules()
        {
            // Arrange
            var userId = "user1";
            var schedules = new List<UserScheduleDTO>
            {
                CreateSampleEntry(),
                CreateSampleEntry()
            };

            _mockRepository.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(schedules);

            // Act
            var result = await _service.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockRepository.Verify(r => r.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ReturnsSuccessMessage_WhenAddSucceeds()
        {
            // Arrange
            var userId = "user1";
            var entry = CreateSampleEntry();

            _mockRepository.Setup(r => r.AddAsync(userId, entry)).ReturnsAsync(true);

            // Act
            var result = await _service.AddAsync(userId, entry);

            // Assert
            Assert.Equal("Successfully added.", result);
            _mockRepository.Verify(r => r.AddAsync(userId, entry), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ReturnsFailureMessage_WhenEntryExists()
        {
            // Arrange
            var userId = "user1";
            var entry = CreateSampleEntry();

            _mockRepository.Setup(r => r.AddAsync(userId, entry)).ReturnsAsync(false);

            // Act
            var result = await _service.AddAsync(userId, entry);

            // Assert
            Assert.Equal("Entry already exists. No changes made.", result);
            _mockRepository.Verify(r => r.AddAsync(userId, entry), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsSuccessMessage_WhenUpdateSucceeds()
        {
            // Arrange
            var userId = "user1";
            var oldEntry = CreateSampleEntry();
            var newEntry = CreateSampleEntry();
            newEntry.NrPost = 2;

            _mockRepository.Setup(r => r.UpdateAsync(userId, oldEntry, newEntry)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(userId, oldEntry, newEntry);

            // Assert
            Assert.Equal("Successfully updated.", result);
            _mockRepository.Verify(r => r.UpdateAsync(userId, oldEntry, newEntry), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFailureMessage_WhenEntryNotFound()
        {
            // Arrange
            var userId = "user1";
            var oldEntry = CreateSampleEntry();
            var newEntry = CreateSampleEntry();

            _mockRepository.Setup(r => r.UpdateAsync(userId, oldEntry, newEntry)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(userId, oldEntry, newEntry);

            // Assert
            Assert.Equal("Entry not found for update.", result);
            _mockRepository.Verify(r => r.UpdateAsync(userId, oldEntry, newEntry), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccessMessage_WhenDeleteSucceeds()
        {
            // Arrange
            var userId = "user1";
            var entry = CreateSampleEntry();

            _mockRepository.Setup(r => r.DeleteAsync(userId, entry)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(userId, entry);

            // Assert
            Assert.Equal("Successfully deleted.", result);
            _mockRepository.Verify(r => r.DeleteAsync(userId, entry), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFailureMessage_WhenEntryNotFound()
        {
            // Arrange
            var userId = "user1";
            var entry = CreateSampleEntry();

            _mockRepository.Setup(r => r.DeleteAsync(userId, entry)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(userId, entry);

            // Assert
            Assert.Equal("Entry not found for deletion.", result);
            _mockRepository.Verify(r => r.DeleteAsync(userId, entry), Times.Once);
        }
    }
}
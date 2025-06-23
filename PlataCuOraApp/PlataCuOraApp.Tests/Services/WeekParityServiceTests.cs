using Moq;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Services;

namespace PlataCuOraApp.Tests.Services
{
    public class WeekParityServiceTests
    {
        private readonly Mock<IWeekParityRepository> _mockRepository;
        private readonly WeekParityService _service;

        public WeekParityServiceTests()
        {
            _mockRepository = new Mock<IWeekParityRepository>();
            _service = new WeekParityService(_mockRepository.Object);
        }

        [Fact]
        public async Task AddOrUpdateWeekParityAsync_ShouldCallRepository()
        {
            // Arrange
            var userId = "user123";
            var weeks = new List<WeekParityDTO>
            {
                new WeekParityDTO { Sapt = "1", Data = "2024-01-01", Paritate = "impar" }
            };

            // Act
            await _service.AddOrUpdateWeekParityAsync(userId, weeks);

            // Assert
            _mockRepository.Verify(r => r.AddOrUpdateParitateSaptAsync(userId, weeks), Times.Once);
        }

        [Fact]
        public async Task GetWeekParityAsync_Should_Return_Data_From_Repository()
        {
            // Arrange
            var userId = "user123";
            var expected = new List<WeekParityDTO>
            {
                new WeekParityDTO { Sapt = "2", Data = "2024-01-08", Paritate = "par" }
            };

            _mockRepository.Setup(r => r.GetWeekParityAsync(userId)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetWeekParityAsync(userId);

            // Assert
            Assert.Equal(expected, result);
            _mockRepository.Verify(r => r.GetWeekParityAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateParityAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            // Arrange
            var userId = "user123";
            var oldEntry = new WeekParityDTO { Sapt = "3", Data = "2024-01-15", Paritate = "impar" };
            var newEntry = new WeekParityDTO { Sapt = "3", Data = "2024-01-15", Paritate = "par" };

            _mockRepository.Setup(r => r.UpdateParitateAsync(userId, oldEntry, newEntry)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateParityAsync(userId, oldEntry, newEntry);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.UpdateParitateAsync(userId, oldEntry, newEntry), Times.Once);
        }

        [Fact]
        public async Task DeleteParityAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            var userId = "user123";
            var entry = new WeekParityDTO { Sapt = "4", Data = "2024-01-22", Paritate = "par" };

            _mockRepository.Setup(r => r.DeleteParitateAsync(userId, entry)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteParityAsync(userId, entry);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteParitateAsync(userId, entry), Times.Once);
        }
    }
}
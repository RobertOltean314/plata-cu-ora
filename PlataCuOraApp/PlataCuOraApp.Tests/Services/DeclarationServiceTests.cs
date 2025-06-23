using Microsoft.Extensions.Logging;
using Moq;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Repository.Interfaces;
using Xunit;

namespace PlataCuOraApp.Tests.Services
{
    public class DeclarationServiceTests
    {
        private readonly Mock<IUserInformationRepository> _mockInfoRepo = new();
        private readonly Mock<IUserScheduleRepository> _mockScheduleRepo = new();
        private readonly Mock<IWeekParityRepository> _mockParityRepo = new();
        private readonly Mock<ILogger<DeclarationService>> _mockLogger = new();

        private readonly DeclarationService _service;

        public DeclarationServiceTests()
        {
            _service = new DeclarationService(
                _mockInfoRepo.Object,
                _mockScheduleRepo.Object,
                _mockParityRepo.Object,
                _mockLogger.Object
            );
        }

        private static UserInformationDTO GetSampleUserInfo() => new()
        {
            Declarant = "John Doe",
            Tip = "LR",
            Decan = "Prof. X",
            DirectorDepartament = "Dr. Y",
            Universitate = "Uni Test",
            Facultate = "Fac Test",
            Departament = "Dept Test"
        };

        private static List<UserScheduleDTO> GetSampleSchedule() => new()
        {
            new UserScheduleDTO
            {
                NrPost = 1,
                DenPost = "Lect. Univ.",
                OreCurs = 2,
                Ziua = "Luni",
                Formatia = "Grupa A",
                Tip = "LR",
                TotalOre = 4
            }
        };

        private static List<WeekParityDTO> GetSampleParities() => new()
        {
            new WeekParityDTO
            {
                Sapt = "S1",
                Data = DateTime.Now.ToString("dd.MM.yyyy"),
                Paritate = "Impar"
            }
        };

        [Fact]
        public async Task GenerateDeclarationAsync_WithValidData_ReturnsPdfBytes()
        {
            // Arrange
            var userId = "user123";
            var days = new List<DateTime> { DateTime.Today };

            _mockInfoRepo.Setup(r => r.GetActiveInfoAsync(userId)).ReturnsAsync(GetSampleUserInfo());
            _mockScheduleRepo.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(GetSampleSchedule());
            _mockParityRepo.Setup(r => r.GetWeekParityAsync(userId)).ReturnsAsync(GetSampleParities());

            // Act
            var result = await _service.GenerateDeclarationAsync(userId, days);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateDeclarationAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var userId = "user123";
            _mockInfoRepo.Setup(r => r.GetActiveInfoAsync(userId)).ReturnsAsync((UserInformationDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GenerateDeclarationAsync(userId, new List<DateTime> { DateTime.Today }));
        }

        [Fact]
        public async Task GenerateDeclarationAsync_ThrowsException_WhenNoSchedule()
        {
            // Arrange
            var userId = "user123";
            _mockInfoRepo.Setup(r => r.GetActiveInfoAsync(userId)).ReturnsAsync(GetSampleUserInfo());
            _mockScheduleRepo.Setup(r => r.GetAllAsync(userId)).ReturnsAsync((List<UserScheduleDTO>?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GenerateDeclarationAsync(userId, new List<DateTime> { DateTime.Today }));
        }

        [Fact]
        public async Task GenerateDeclarationAsync_ThrowsException_WhenNoParity()
        {
            // Arrange
            var userId = "user123";

            _mockInfoRepo.Setup(r => r.GetActiveInfoAsync(userId)).ReturnsAsync(GetSampleUserInfo());
            _mockScheduleRepo.Setup(r => r.GetAllAsync(userId)).ReturnsAsync(GetSampleSchedule());
            _mockParityRepo.Setup(r => r.GetWeekParityAsync(userId)).ReturnsAsync((List<WeekParityDTO>?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GenerateDeclarationAsync(userId, new List<DateTime> { DateTime.Today }));
        }
    }
}
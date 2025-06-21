using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Services.Implementation;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Tests.Services;

public class UserInformationServiceTests
{

    private readonly Mock<IUserInformationRepository> _mockRepository;
    private readonly Mock<ILogger<UserInformationService>> _mockLogger;
    private readonly UserInformationService _service;

    public UserInformationServiceTests()
    {
        _mockRepository = new Mock<IUserInformationRepository>();
        _mockLogger = new Mock<ILogger<UserInformationService>>();
        _service = new UserInformationService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllInfoAsync_ReturnsCorrectUserInfoList()
    {
        // Arrange
        var userId = "testUser123";

        var expectedList = new List<UserInformationDTO>
        {
            new UserInformationDTO
            {
                Declarant = "Maria Ionescu",
                Tip = "Student",
                DirectorDepartament = "Dr. Popescu",
                Decan = "Prof. Ionescu",
                Universitate = "UB",
                Facultate = "Matematică",
                Departament = "Informatică",
                IsActive = true
            },
            new UserInformationDTO
            {
                Declarant = "Ion Pop",
                Tip = "Cadru didactic",
                DirectorDepartament = "Dr. Andrei",
                Decan = "Prof. Vasilescu",
                Universitate = "UPB",
                Facultate = "Automatică",
                Departament = "Calculatoare",
                IsActive = false
            }
        };

        _mockRepository.Setup(r => r.GetAllInfoAsync(userId)).ReturnsAsync(expectedList);

        // Act
        var result = await _service.GetAllInfoAsync(userId);

        // Assert - the list has 2 elements
        Assert.Equal(2, result.Count);

        // Assert for first element
        var firstResult = result[0];
        Assert.Equal("Maria Ionescu", firstResult.Declarant);
        Assert.Equal("Student", firstResult.Tip);
        Assert.Equal("Dr. Popescu", firstResult.DirectorDepartament);
        Assert.Equal("Prof. Ionescu", firstResult.Decan);
        Assert.Equal("UB", firstResult.Universitate);
        Assert.Equal("Matematică", firstResult.Facultate);
        Assert.Equal("Informatică", firstResult.Departament);
        Assert.True(firstResult.IsActive);

        // Assert for second element
        var secondResult = result[1];
        Assert.Equal("Ion Pop", secondResult.Declarant);
        Assert.Equal("Cadru didactic", secondResult.Tip);
        Assert.Equal("Dr. Andrei", secondResult.DirectorDepartament);
        Assert.Equal("Prof. Vasilescu", secondResult.Decan);
        Assert.Equal("UPB", secondResult.Universitate);
        Assert.Equal("Automatică", secondResult.Facultate);
        Assert.Equal("Calculatoare", secondResult.Departament);
        Assert.False(secondResult.IsActive);
    }

    [Fact]
    public async Task AddInfoAsync_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        // Arrange
        string userId = "user1";
        var newInfo = new UserInformationDTO
        {
            Declarant = "George Enescu",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Marinescu",
            Decan = "Prof. Popa",
            Universitate = "Universitatea Cluj",
            Facultate = "Muzică",
            Departament = "Compoziție",
            IsActive = false
        };

        _mockRepository.Setup(r => r.AddInfoAsync(userId, newInfo)).ReturnsAsync(true);

        // Act
        var result = await _service.AddInfoAsync(userId, newInfo);

        // Assert
        Assert.True(result.success);
        Assert.Null(result.error);
        _mockRepository.Verify(r => r.AddInfoAsync(userId, newInfo), Times.Once);
    }

    [Fact]
    public async Task AddInfoAsync_ShouldReturnFailure_WhenRepositoryReturnsFalse()
    {
        // Arrange
        string userId = "user1";
        var newInfo = new UserInformationDTO
        {
            Declarant = "Ana Marinescu",
            Tip = "Student",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository.Setup(r => r.AddInfoAsync(userId, newInfo)).ReturnsAsync(false);

        // Act
        var result = await _service.AddInfoAsync(userId, newInfo);

        // Assert
        Assert.False(result.success);
        Assert.Equal("Duplicate info entry. No changes made.", result.error);
        _mockRepository.Verify(r => r.AddInfoAsync(userId, newInfo), Times.Once);
    }

    [Fact]
    public async Task UpdateInfoAsync_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        // Arrange
        var userId = "user1";
        var oldInfo = new UserInformationDTO
        {
            Declarant = "Old",
            Tip = "Student",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = false
        };
        var newInfo = new UserInformationDTO
        {
            Declarant = "New",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        // Setup mock ca UpdateInfoAsync sa returneze true cand e apelat cu exact parametrii asteptati
        _mockRepository.Setup(r => r.UpdateInfoAsync(
            userId,
            It.Is<UserInformationDTO>(dto => dto.Declarant == "Old" && dto.IsActive == false),
            It.Is<UserInformationDTO>(dto => dto.Declarant == "New" && dto.IsActive == true)))
            .ReturnsAsync(true)
            .Verifiable();

        // Act
        var result = await _service.UpdateInfoAsync(userId, oldInfo, newInfo);

        // Assert
        Assert.True(result.success);
        Assert.Null(result.error);

        // Verifica ca metoda repo a fost apelata o singura data cu exact parametrii asteptati
        _mockRepository.Verify(r => r.UpdateInfoAsync(userId, oldInfo, newInfo), Times.Once);
    }

    [Fact]
    public async Task UpdateInfoAsync_ShouldReturnFailure_WhenRepositoryReturnsFalse()
    {
        // Arrange
        var userId = "user1";
        var oldInfo = new UserInformationDTO
        {
            Declarant = "Old",
            Tip = "Student",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = false
        };
        var newInfo = new UserInformationDTO
        {
            Declarant = "New",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository.Setup(r => r.UpdateInfoAsync(userId, oldInfo, newInfo)).ReturnsAsync(false);

        // Act
        var result = await _service.UpdateInfoAsync(userId, oldInfo, newInfo);

        // Assert
        Assert.False(result.success);
        Assert.Equal("Info entry to update not found.", result.error);

        _mockRepository.Verify(r => r.UpdateInfoAsync(userId, oldInfo, newInfo), Times.Once);
    }

    [Fact]
    public async Task DeleteInfoAsync_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "ToDelete",
            Tip = "Student",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.DeleteInfoAsync(userId, info))
            .ReturnsAsync(true)
            .Verifiable();

        // Act
        var result = await _service.DeleteInfoAsync(userId, info);

        // Assert
        Assert.True(result.success);
        Assert.Null(result.error);

        _mockRepository.Verify(r => r.DeleteInfoAsync(userId, info), Times.Once);
    }

    [Fact]
    public async Task DeleteInfoAsync_ShouldReturnFailure_WhenRepositoryReturnsFalse()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "ToDelete",
            Tip = "Student",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.DeleteInfoAsync(userId, info))
            .ReturnsAsync(false)
            .Verifiable();

        // Act
        var result = await _service.DeleteInfoAsync(userId, info);

        // Assert
        Assert.False(result.success);
        Assert.Equal("Info entry to delete not found.", result.error);

        _mockRepository.Verify(r => r.DeleteInfoAsync(userId, info), Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "SetActive",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = false
        };

        _mockRepository
            .Setup(r => r.SetActiveAsync(userId, info))
            .ReturnsAsync(true)
            .Verifiable();

        // Act
        var result = await _service.SetActiveAsync(userId, info);

        // Assert
        Assert.True(result.success);
        Assert.Null(result.error);
        _mockRepository.Verify(r => r.SetActiveAsync(userId, info), Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_ShouldReturnFailure_WhenRepositoryReturnsFalse()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "SetActive",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = false
        };

        _mockRepository
            .Setup(r => r.SetActiveAsync(userId, info))
            .ReturnsAsync(false)
            .Verifiable();

        // Act
        var result = await _service.SetActiveAsync(userId, info);

        // Assert
        Assert.False(result.success);
        Assert.Equal("Info entry to set active not found.", result.error);
        _mockRepository.Verify(r => r.SetActiveAsync(userId, info), Times.Once);
    }

    [Fact]
    public async Task UnsetActiveAsync_ShouldReturnSuccess_WhenRepositoryReturnsTrue()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "UnsetActive",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.UnsetActiveAsync(userId, info))
            .ReturnsAsync(true)
            .Verifiable();

        // Act
        var result = await _service.UnsetActiveAsync(userId, info);

        // Assert
        Assert.True(result.success);
        Assert.Null(result.error);
        _mockRepository.Verify(r => r.UnsetActiveAsync(userId, info), Times.Once);
    }

    [Fact]
    public async Task UnsetActiveAsync_ShouldReturnFailure_WhenRepositoryReturnsFalse()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "UnsetActive",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.UnsetActiveAsync(userId, info))
            .ReturnsAsync(false)
            .Verifiable();

        // Act
        var result = await _service.UnsetActiveAsync(userId, info);

        // Assert
        Assert.False(result.success);
        Assert.Equal("Failed to unset active info.", result.error);
        _mockRepository.Verify(r => r.UnsetActiveAsync(userId, info), Times.Once);
    }

    [Fact]
    public async Task AddActiveInfoToDbAsync_ShouldReturnActiveInfo_WhenActiveExists()
    {
        // Arrange
        var userId = "user1";
        var activeInfo = new UserInformationDTO
        {
            Declarant = "AddedActive",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.AddActiveInfoToDbAsync(userId))
            .ReturnsAsync(activeInfo);

        // Act
        var result = await _service.AddActiveInfoToDbAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.IsActive);
        Assert.Equal("AddedActive", result.Declarant);
    }

    [Fact]
    public async Task AddActiveInfoToDbAsync_ShouldReturnNull_WhenNoActiveInfoExists()
    {
        // Arrange
        var userId = "user1";

        _mockRepository
            .Setup(r => r.AddActiveInfoToDbAsync(userId))
            .ReturnsAsync((UserInformationDTO?)null);

        // Act
        var result = await _service.AddActiveInfoToDbAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInfoUserFromDbAsync_ShouldReturnInfo_WhenDocumentExists()
    {
        // Arrange
        var userId = "user1";
        var info = new UserInformationDTO
        {
            Declarant = "FromDb",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetInfoUserFromDbAsync(userId))
            .ReturnsAsync(info);

        // Act
        var result = await _service.GetInfoUserFromDbAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FromDb", result!.Declarant);
    }

    [Fact]
    public async Task GetInfoUserFromDbAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Arrange
        var userId = "user1";

        _mockRepository
            .Setup(r => r.GetInfoUserFromDbAsync(userId))
            .ReturnsAsync((UserInformationDTO?)null);

        // Act
        var result = await _service.GetInfoUserFromDbAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveInfoAsync_ShouldReturnActiveInfo_WhenExists()
    {
        // Arrange
        var userId = "user1";
        var activeInfo = new UserInformationDTO
        {
            Declarant = "ActiveInfo",
            Tip = "Profesor",
            DirectorDepartament = "Dr. Radu",
            Decan = "Prof. Ionescu",
            Universitate = "ASE",
            Facultate = "Economie",
            Departament = "Finanțe",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetActiveInfoAsync(userId))
            .ReturnsAsync(activeInfo);

        // Act
        var result = await _service.GetActiveInfoAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.IsActive);
        Assert.Equal("ActiveInfo", result.Declarant);
    }

    [Fact]
    public async Task GetActiveInfoAsync_ShouldReturnNull_WhenNoActiveInfoExists()
    {
        // Arrange
        var userId = "user1";

        _mockRepository
            .Setup(r => r.GetActiveInfoAsync(userId))
            .ReturnsAsync((UserInformationDTO?)null);

        // Act
        var result = await _service.GetActiveInfoAsync(userId);

        // Assert
        Assert.Null(result);
    }
}
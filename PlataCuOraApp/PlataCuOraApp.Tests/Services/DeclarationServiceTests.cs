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

        //[Fact]
        //public void CalculateRows_ForLRType_ComputesCorrectTotalAndCoef()
        //{
        //    var zile = new List<DateTime> { new DateTime(2025, 3, 3) }; // Luni

        //    var orar = new List<UserScheduleDTO>
        //    {
        //        new UserScheduleDTO
        //        {
        //            NrPost = 1,
        //            DenPost = "Lect. Univ.",
        //            Ziua = "Luni",
        //            OreCurs = 2,
        //            Tip = "LR",
        //            Formatia = "A1",
        //            TotalOre = 4
        //        }
        //    };

        //    var paritati = new List<WeekParityDTO>
        //    {
        //        new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" }
        //    };

        //    var result = _service.TestCalculateFinalRows(orar, paritati, zile);

        //    Assert.Single(result);
        //    var row = result[0];
        //    Assert.Equal(2, row.c);
        //    Assert.Equal("LR", row.tip);
        //    Assert.Equal(2.0, row.coef);
        //    Assert.Equal(4.0, row.total); // 2 ore * 2 coef = 4
        //}

        //[Fact]
        //public void CalculateRows_RespectsMax12HoursPerDay()
        //{
        //    var zi = new DateTime(2025, 3, 4); // Marti

        //    var orar = new List<UserScheduleDTO>();
        //    for (int i = 0; i < 6; i++)
        //    {
        //        orar.Add(new UserScheduleDTO
        //        {
        //            NrPost = i + 1,
        //            DenPost = "Asist.",
        //            Ziua = "Marti",
        //            OreCurs = 2,
        //            Tip = "LR",
        //            Formatia = "A" + i,
        //            TotalOre = 10
        //        });
        //    }

        //    var paritati = new List<WeekParityDTO>
        //    {
        //        new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" }
        //    };

        //    var result = _service.TestCalculateFinalRows(orar, paritati, new List<DateTime> { zi });

        //    int totalPhysicalHours = result.Sum(r => r.c + r.s + r.la + r.p);
        //    Assert.True(totalPhysicalHours <= 12);
        //}

        //[Fact]
        //public void CalculateRows_IgnoresHoursFromFutureWeeks()
        //{
        //    var zi = new DateTime(2025, 3, 3); // Saptamana 1

        //    var orar = new List<UserScheduleDTO>
        //    {
        //        new UserScheduleDTO
        //        {
        //            NrPost = 1,
        //            DenPost = "Conf. dr.",
        //            Ziua = "Luni",
        //            OreCurs = 2,
        //            Tip = "LR",
        //            Formatia = "X",
        //            SaptamanaInceput = "S3", // începe abia în S3
        //            TotalOre = 4
        //        }
        //    };

        //    var paritati = new List<WeekParityDTO>
        //    {
        //        new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" },
        //        new WeekParityDTO { Sapt = "S3", Data = "17.03.2025", Paritate = "Impar" }
        //    };

        //    var result = _service.TestCalculateFinalRows(orar, paritati, new List<DateTime> { zi });

        //    Assert.Empty(result); // încă nu a început săptămâna
        //}

        //[Fact]
        //public void CalculateRows_OnlyAllocatesIfParityMatches()
        //{
        //    var zi = new DateTime(2025, 3, 3); // S1 - Impar

        //    var orar = new List<UserScheduleDTO>
        //    {
        //        new UserScheduleDTO
        //        {
        //            NrPost = 1,
        //            DenPost = "Asist.",
        //            Ziua = "Luni",
        //            OreCurs = 2,
        //            Tip = "LR",
        //            Formatia = "Y",
        //            ImparPar = "Par", // NU e potrivită paritatea
        //            TotalOre = 4
        //        }
        //    };

        //    var paritati = new List<WeekParityDTO>
        //    {
        //        new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" },
        //        new WeekParityDTO { Sapt = "S2", Data = "10.03.2025", Paritate = "Par" }
        //    };

        //    var result = _service.TestCalculateFinalRows(orar, paritati, new List<DateTime> { zi });

        //    Assert.Empty(result); // paritatea nu se potrivește
        //}

        [Fact]
        public void CalculateRows_ForComplexCoefficients_ComputesCorrectTotals()
        {
            // Arrange
            var zi = new DateTime(2025, 3, 3); // Luni
            var orar = new List<UserScheduleDTO>
            {
                new UserScheduleDTO { NrPost = 1, DenPost = "Lect. Univ.", Ziua = "Luni", OreCurs = 2, Tip = "LE", Formatia = "A1_E" }, // 2 * 2.5 = 5
                new UserScheduleDTO { NrPost = 2, DenPost = "Asist. Univ.", Ziua = "Luni", OreSem = 2, Tip = "LE", Formatia = "A1_E" }, // 2 * 1.25 = 2.5
                new UserScheduleDTO { NrPost = 3, DenPost = "Prof. Univ.", Ziua = "Luni", OreCurs = 2, Tip = "ME", Formatia = "M1_E" }, // 2 * 3.125 = 6.25
                new UserScheduleDTO { NrPost = 4, DenPost = "Sef Lucr.", Ziua = "Luni", OreLab = 2, Tip = "ME", Formatia = "M1_E" }  // 2 * 1.875 = 3.75
            };
            var paritati = new List<WeekParityDTO> { new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" } };

            // Act
            var result = _service.TestCalculateFinalRows(orar, paritati, new List<DateTime> { zi });

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Contains(result, r => r.tip == "LE" && r.c > 0 && r.coef == 2.5 && r.total == 5.0);
            Assert.Contains(result, r => r.tip == "LE" && r.s > 0 && r.coef == 1.25 && r.total == 2.5);
            Assert.Contains(result, r => r.tip == "ME" && r.c > 0 && r.coef == 3.125 && r.total == 6.25);
            Assert.Contains(result, r => r.tip == "ME" && r.la > 0 && r.coef == 1.875 && r.total == 3.75);

            // Total ore echivalente
            Assert.Equal(17.5, result.Sum(r => r.total), 3); // Suma: 5 + 2.5 + 6.25 + 3.75 = 17.5
        }

        [Fact]
        public void CalculateRows_RespectsMax12HoursPerDay_WithMixedActivities()
        {
            // Arrange
            var zi = new DateTime(2025, 3, 4); // Marti
            var orar = new List<UserScheduleDTO>
            {
                // Activitate "speciala" care va fi procesata prima
                new UserScheduleDTO { NrPost = 1, DenPost = "Special", Ziua = "Marti", OreCurs = 2, Tip = "LR", Formatia = "S", SaptamanaInceput = "S1", TotalOre = 20 },
                // Activitati "normale"
                new UserScheduleDTO { NrPost = 2, DenPost = "Normal1", Ziua = "Marti", OreLab = 8, Tip = "LR", Formatia = "N1" },
                new UserScheduleDTO { NrPost = 3, DenPost = "Normal2", Ziua = "Marti", OreSem = 4, Tip = "LR", Formatia = "N2" } // Aceasta nu ar trebui sa intre
            };
            var paritati = new List<WeekParityDTO> { new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" } };

            // Act
            var result = _service.TestCalculateFinalRows(orar, paritati, new List<DateTime> { zi });

            // Assert
            int totalPhysicalHours = result.Sum(r => r.c + r.s + r.la + r.p);
            Assert.Equal(10, totalPhysicalHours); // 2h (special) + 8h (normal1) = 10h. Urmatoarele 4h depasesc limita de 12.
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.c == 2);
            Assert.Contains(result, r => r.la == 8);
        }

        [Fact]
        public void CalculateRows_ForRecurringActivity_SpanningMultipleWeeks()
        {
            // Arrange
            // Activitate de 2 ore/saptamana care incepe in S2, cu un total de 6 ore (deci ar trebui sa apara in S2, S3, S4)
            var orar = new List<UserScheduleDTO>
            {
                new UserScheduleDTO
                {
                    NrPost = 1, DenPost = "Curs Recurent", Ziua = "Miercuri",
                    OreCurs = 2, Tip = "LR", Formatia = "Rec",
                    SaptamanaInceput = "S2", TotalOre = 6
                }
            };

            // Definim paritatile pentru mai multe saptamani
            var paritati = new List<WeekParityDTO>
            {
                new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" },
                new WeekParityDTO { Sapt = "S2", Data = "10.03.2025", Paritate = "Par" },
                new WeekParityDTO { Sapt = "S3", Data = "17.03.2025", Paritate = "Impar" },
                new WeekParityDTO { Sapt = "S4", Data = "24.03.2025", Paritate = "Par" },
                new WeekParityDTO { Sapt = "S5", Data = "31.03.2025", Paritate = "Impar" },
            };

            // Zilele lucrate: una inainte de start, doua in timpul perioadei, una dupa
            var zileLucrate = new List<DateTime>
            {
                new DateTime(2025, 3, 5),  // Miercuri in S1 -> NU trebuie sa apara
                new DateTime(2025, 3, 12), // Miercuri in S2 -> DA, trebuie sa apara (primele 2 ore)
                new DateTime(2025, 3, 26), // Miercuri in S4 -> DA, trebuie sa apara (urmatoarele 2 ore, nu 2 * 2 = 4)
                new DateTime(2025, 4, 2)   // Miercuri in S5 -> NU trebuie sa apara (s-au consumat cele 6 ore totale in S2,S3,S4)
            };

            // Act
            var result = _service.TestCalculateFinalRows(orar, paritati, zileLucrate);

            // Assert
            Assert.Equal(2, result.Count); // Doar 2 intrari valide
            Assert.Contains(result, r => r.ziua.Day == 12); // Contine ziua din S2
            Assert.Contains(result, r => r.ziua.Day == 26); // Contine ziua din S4
            Assert.DoesNotContain(result, r => r.ziua.Day == 5); // NU contine ziua din S1
            Assert.DoesNotContain(result, r => r.ziua.Day == 2);  // NU contine ziua din S5
        }

        [Fact]
        public void CalculateRows_IgnoresHours_WhenParityDoesNotMatch()
        {
            // Arrange
            // Activitate care necesita saptamana PARA
            var orar = new List<UserScheduleDTO>
            {
                new UserScheduleDTO
                {
                    NrPost = 1, DenPost = "Asist.", Ziua = "Luni", OreSem = 2, Tip = "LR",
                    Formatia = "Y", ImparPar = "Par", TotalOre = 14
                }
            };

            var paritati = new List<WeekParityDTO>
            {
                new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" },
                new WeekParityDTO { Sapt = "S2", Data = "10.03.2025", Paritate = "Par" }
            };

            // Lucram o zi de Luni in saptamana IMPARA
            var zileLucrate = new List<DateTime> { new DateTime(2025, 3, 3) };

            // Act
            var result = _service.TestCalculateFinalRows(orar, paritati, zileLucrate);

            // Assert
            Assert.Empty(result); // Nu ar trebui sa gaseasca nicio ora, deoarece paritatea nu corespunde
        }

        [Fact]
        public void CalculateRows_HonorsBoth_StartDateAndParity()
        {
            // Arrange
            // Activitate care incepe in S3 si este IMPARA
            var orar = new List<UserScheduleDTO>
            {
                new UserScheduleDTO
                {
                    NrPost = 1, DenPost = "Conf. dr.", Ziua = "Joi", OreCurs = 2, Tip = "MR",
                    Formatia = "Z", SaptamanaInceput = "S3", ImparPar = "Impar", TotalOre = 4
                }
            };

            var paritati = new List<WeekParityDTO>
            {
                new WeekParityDTO { Sapt = "S1", Data = "03.03.2025", Paritate = "Impar" },
                new WeekParityDTO { Sapt = "S2", Data = "10.03.2025", Paritate = "Par" },
                new WeekParityDTO { Sapt = "S3", Data = "17.03.2025", Paritate = "Impar" },
                new WeekParityDTO { Sapt = "S4", Data = "24.03.2025", Paritate = "Par" }
            };

            var zileLucrate = new List<DateTime>
            {
                new DateTime(2025, 3, 6),  // Joi in S1 (Impar, dar prea devreme) -> NU
                new DateTime(2025, 3, 13), // Joi in S2 (Start NU e ok, si paritate gresita) -> NU
                new DateTime(2025, 3, 20)  // Joi in S3 (Start ok, paritate ok) -> DA
            };

            // Act
            var result = _service.TestCalculateFinalRows(orar, paritati, zileLucrate);

            // Assert
            Assert.Single(result);
            Assert.Equal(new DateTime(2025, 3, 20), result.First().ziua);
            Assert.Equal(2, result.First().c);
            Assert.Equal("MR", result.First().tip);
        }
    }
}
using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IHolidaysService
    {
        Task<List<PublicHolidayDTO>> GetHolidaysAsync(int year);
    }
}

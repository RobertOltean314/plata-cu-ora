using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Services.Interfaces
{
    public interface IWorkingDaysService
    {
        Task<List<WorkingDayDTO>> GetWorkingDaysAsync(string userId, DateTime startDate, DateTime endDate);
    }
}
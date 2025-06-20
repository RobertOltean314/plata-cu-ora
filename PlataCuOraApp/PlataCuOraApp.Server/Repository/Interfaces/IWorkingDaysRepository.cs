using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
namespace PlataCuOraApp.Server.Repository.Interfaces
{
    public interface IWorkingDaysRepository
    {
        Task<List<WorkingDayDTO>> GetWorkingDaysAsync(string userId, DateTime startDate, DateTime endDate);
    }
}
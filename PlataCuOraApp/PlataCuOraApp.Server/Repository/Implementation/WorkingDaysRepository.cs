using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Repositories
{
    public class WorkingDaysRepository : IWorkingDaysRepository
    {
        private readonly IWorkingDaysService _service;

        public WorkingDaysRepository(IWorkingDaysService service)
        {
            _service = service;
        }

        public async Task<List<WorkingDayDTO>> GetWorkingDaysAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _service.GetWorkingDaysAsync(userId, startDate, endDate);
        }
    }
}

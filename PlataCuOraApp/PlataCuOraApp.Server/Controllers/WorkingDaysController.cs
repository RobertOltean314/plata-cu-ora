using Microsoft.AspNetCore.Mvc;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkingDaysController : ControllerBase
    {
        private readonly IWorkingDaysRepository _repository;

        public WorkingDaysController(IWorkingDaysRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{userId}/{start}/{end}")]
        public async Task<IActionResult> GetWorkingDays(string userId, DateTime start, DateTime end)
        {
            Console.WriteLine($"GetWorkingDays called with userId={userId}, start={start}, end={end}");
            var days = await _repository.GetWorkingDaysAsync(userId, start, end);
            return Ok(days);
        }
    }
}
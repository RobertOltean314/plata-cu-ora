using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Domain.DTOs;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOra.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        [HttpGet("{year}")]
        public async Task<IActionResult> GetPublicHolidays(int year)
        {
            using var client = new HttpClient();
            var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/RO";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return StatusCode(500, "Failed to fetch holidays");

            var json = await response.Content.ReadAsStringAsync();
            var holidays = JsonSerializer.Deserialize<List<PublicHolidayDTO>>(json);

            if (holidays == null || holidays.Count == 0)
                return NotFound("No holidays found.");

            return Ok(holidays);
        }
    }
}
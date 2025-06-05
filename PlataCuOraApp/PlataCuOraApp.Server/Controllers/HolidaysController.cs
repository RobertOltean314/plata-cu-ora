//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using System;
//using PlataCuOraApp.Server.Domain.DTOs;

//namespace PlataCuOraApp.Server.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class HolidaysController : ControllerBase
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ILogger<HolidaysController> _logger;
//        private const string HolidayApiBaseUrl = "https://date.nager.at/api/v3/PublicHolidays";

//        public HolidaysController(IHttpClientFactory httpClientFactory, ILogger<HolidaysController> logger)
//        {
//            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        [HttpGet("{year}")]
//        public async Task<IActionResult> GetHolidays(int year)
//        {
//            try
//            {
//                var client = _httpClientFactory.CreateClient();
//                var url = $"{HolidayApiBaseUrl}/{year}/RO";

//                _logger.LogInformation("Fetching holidays for year {Year}", year);
//                var response = await client.GetAsync(url);

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogWarning("Failed to fetch holidays. Status code: {StatusCode}", response.StatusCode);
//                    return StatusCode((int)response.StatusCode, "Failed to fetch holidays");
//                }

//                var json = await response.Content.ReadAsStringAsync();
//                var holidays = JsonSerializer.Deserialize<List<PublicHolidayDTO>>(json, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (holidays == null || holidays.Count == 0)
//                {
//                    _logger.LogInformation("No holidays found for year {Year}", year);
//                    return NotFound($"No holidays found for year {year}.");
//                }

//                return Ok(holidays);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while fetching holidays for year {Year}", year);
//                return StatusCode(500, "An unexpected error occurred while fetching holidays");
//            }
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HolidaysController> _logger;
        private const string HolidayApiBaseUrl = "https://zilelibere.webventure.ro/api";

        public HolidaysController(IHttpClientFactory httpClientFactory, ILogger<HolidaysController> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{year}")]
        public async Task<IActionResult> GetHolidays(int year)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{HolidayApiBaseUrl}/{year}";

                _logger.LogInformation("Fetching holidays for year {Year}", year);
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch holidays. Status code: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, "Failed to fetch holidays");
                }

                var json = await response.Content.ReadAsStringAsync();
                var holidays = JsonSerializer.Deserialize<List<PublicHolidayDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (holidays == null || holidays.Count == 0)
                {
                    _logger.LogInformation("No holidays found for year {Year}", year);
                    return NotFound($"No holidays found for year {year}.");
                }

                return Ok(holidays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching holidays for year {Year}", year);
                return StatusCode(500, "An unexpected error occurred while fetching holidays");
            }
        }
    }
}

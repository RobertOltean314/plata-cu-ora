using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using PlataCuOraApp.Server.Domain.DTOs;
using Google.Cloud.Firestore;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Services;
using PlataCuOraApp.Server.Services.Interfaces;

namespace PlataCuOraApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly IHolidaysService _holidaysService;

        public HolidaysController(IHolidaysService holidaysService)
        {
            _holidaysService = holidaysService;
        }

        [HttpGet("{year}")]
        public async Task<IActionResult> GetHolidays(int year)
        {
            var holidays = await _holidaysService.GetHolidaysAsync(year);
            if (holidays == null || holidays.Count == 0)
            {
                return NotFound($"No holidays found for year {year}.");
            }

            return Ok(holidays);
        }
    }
}
using PlataCuOraApp.Server.Domain.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using PlataCuOraApp.Server.Services.Interfaces;

public class HolidaysService : IHolidaysService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HolidaysService> _logger;
    private const string HolidayApiBaseUrl = "https://zilelibere.webventure.ro/api";

    public HolidaysService(IHttpClientFactory httpClientFactory, ILogger<HolidaysService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<PublicHolidayDTO>> GetHolidaysAsync(int year)
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
                return new List<PublicHolidayDTO>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var holidays = JsonSerializer.Deserialize<List<PublicHolidayDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return holidays ?? new List<PublicHolidayDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching holidays for year {Year}", year);
            return new List<PublicHolidayDTO>();
        }
    }
}

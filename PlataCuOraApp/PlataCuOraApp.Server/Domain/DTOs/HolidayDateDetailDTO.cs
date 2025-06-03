using System.Text.Json.Serialization;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class HolidayDateDetailDTO
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("weekday")]
        public string Weekday { get; set; }
    }

}

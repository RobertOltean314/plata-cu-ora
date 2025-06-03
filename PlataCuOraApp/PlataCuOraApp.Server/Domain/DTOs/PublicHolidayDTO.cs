using System.Text.Json.Serialization;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class PublicHolidayDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("date")]
        public List<HolidayDateDetailDTO> Dates { get; set; }
    }

}

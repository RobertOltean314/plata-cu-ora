using System.Text.Json.Serialization;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class PublicHolidayDTO
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("localName")]
        public string LocalName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

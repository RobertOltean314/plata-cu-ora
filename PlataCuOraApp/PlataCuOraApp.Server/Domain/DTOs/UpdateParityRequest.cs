using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class UpdateParityRequest
    {
        public WeekParityDTO OldEntry { get; set; }
        public WeekParityDTO NewEntry { get; set; }
    }

}

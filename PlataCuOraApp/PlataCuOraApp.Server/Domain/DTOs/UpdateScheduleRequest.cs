namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class UpdateScheduleRequest
    {
        public UserScheduleDTO OldEntry { get; set; }
        public UserScheduleDTO NewEntry { get; set; }
    }
}

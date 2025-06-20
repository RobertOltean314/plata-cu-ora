namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class UpdateWorkingDayRequest
    {
        public WorkingDayDTO OldEntry { get; set; }
        public WorkingDayDTO NewEntry { get; set; }
    }

}

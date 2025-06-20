namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class WorkingDayDTO
    {
        public string Date { get; set; }
        public string DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }
        public string Parity { get; set; }
    }

}
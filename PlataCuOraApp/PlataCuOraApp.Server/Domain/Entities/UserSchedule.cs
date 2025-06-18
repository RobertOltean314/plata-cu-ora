using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Domain.Entities
{
    public class UserSchedule
    {
        public string Id { get; set; }
        public List<UserScheduleDTO> Orar { get; set; } = new();
    }
}

using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Domain.Entities
{
    public class OrarUser
    {
        public string Id { get; set; }
        public List<OrarUserDTO> Orar { get; set; } = new();
    }
}

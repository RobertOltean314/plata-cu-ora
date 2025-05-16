using PlataCuOraApp.Server.Domain.DTOs;

namespace PlataCuOraApp.Server.Controllers
{
    public class UpdateOrarRequest
    {
        public OrarUserDTO OldEntry { get; set; }
        public OrarUserDTO NewEntry { get; set; }
    }
}

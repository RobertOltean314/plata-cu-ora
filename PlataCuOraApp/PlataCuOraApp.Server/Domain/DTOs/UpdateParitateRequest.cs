using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class UpdateParitateRequest
    {
        public ParitateSaptamanaDTO OldEntry { get; set; }
        public ParitateSaptamanaDTO NewEntry { get; set; }
    }

}

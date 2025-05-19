namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class UpdateOrarRequest
    {
        public OrarUserDTO OldEntry { get; set; }
        public OrarUserDTO NewEntry { get; set; }
    }
}

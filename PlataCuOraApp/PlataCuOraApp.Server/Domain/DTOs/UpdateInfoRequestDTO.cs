namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class UpdateInfoRequestDTO
    {
        public UserInformationDTO Old { get; set; }
        public UserInformationDTO New { get; set; }
    }
}

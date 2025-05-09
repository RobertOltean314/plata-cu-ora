namespace PlataCuOraApp.Server.Domain.DTOs
{
    public class LoginRequestDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
namespace PlataCuOra.Server.Domain.DTOs
{
    public class UserDTO
    {
        // Removed Password - should not be in the response DTO
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;   
    }
}
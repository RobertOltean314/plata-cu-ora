namespace PlataCuOra.Server.Domain.Entities
{
    public class User
    {
        // Note: Password should NOT be stored in this entity
        // It should only be used in the registration DTO

        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
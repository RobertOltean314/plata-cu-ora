namespace PlataCuOra.Server.Infrastructure.Firebase
{
    public interface IFirebaseConfig
    {
        string ApiKey { get; }
        string ProjectId { get; }
    }

    public class FirebaseConfig : IFirebaseConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }
}
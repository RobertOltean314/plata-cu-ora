using Google.Cloud.Firestore;
using System;

namespace PlataCuOra.Server.Domain.Entities
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("role")]
        public string Role { get; set; } = string.Empty;

        public User() { }
    }
}
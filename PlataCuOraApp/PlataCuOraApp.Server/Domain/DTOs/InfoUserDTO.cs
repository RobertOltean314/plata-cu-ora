using Google.Cloud.Firestore;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    [FirestoreData]
    public class InfoUserDTO
    {
        [FirestoreProperty]
        public string Declarant { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Tip { get; set; } = string.Empty;

        [FirestoreProperty]
        public string DirectorDepartament { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Decan { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Universitate { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Facultate { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Departament { get; set; } = string.Empty;

        [FirestoreProperty]
        public bool IsActive { get; set; } = false;
    }
}

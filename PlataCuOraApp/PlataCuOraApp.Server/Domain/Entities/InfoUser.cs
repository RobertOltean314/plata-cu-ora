using Google.Cloud.Firestore;

namespace PlataCuOraApp.Server.Domain.Entities
{
    [FirestoreData]
    public class InfoUser
    {
        [FirestoreDocumentId]  
        public string Id { get; set; }

        [FirestoreProperty("declarant")]
        public string Declarant { get; set; }

        [FirestoreProperty("tip")]
        public string Tip { get; set; }

        [FirestoreProperty("directorDepartament")]
        public string DirectorDepartament { get; set; }

        [FirestoreProperty("decan")]
        public string Decan { get; set; }

        [FirestoreProperty("universitate")]
        public string Universitate { get; set; }

        [FirestoreProperty("facultate")]
        public string Facultate { get; set; }

        [FirestoreProperty("departament")]
        public string Departament { get; set; }
    }
}

using Google.Cloud.Firestore;

namespace PlataCuOraApp.Server.Domain.DTO
{
    [FirestoreData]
    public class ParitateSaptamanaDTO
    {

        [FirestoreProperty("sapt")]
        public string Sapt { get; set; }

        [FirestoreProperty("data")]
        public string Data { get; set; }

        [FirestoreProperty("paritate")]
        public string Paritate { get; set; }
    }
}

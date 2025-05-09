using Google.Cloud.Firestore;

namespace PlataCuOraApp.Server.Infrastructure.Firebase
{
    public class FirestoreDbContext
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreDbContext(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public CollectionReference GetCollection(string collectionName)
        {
            return _firestoreDb.Collection(collectionName);
        }
    }
}
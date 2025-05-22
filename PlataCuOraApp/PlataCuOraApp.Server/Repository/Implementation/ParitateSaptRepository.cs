using Google.Cloud.Firestore;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Repositories;

namespace PlataCuOraApp.Server.Repositories
{
    public class ParitateSaptRepository : IParitateSaptRepository
    {
        private readonly FirestoreDb _firestore;

        public ParitateSaptRepository(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        public async Task AddOrUpdateParitateSaptAsync(string parId, List<ParitateSaptamanaDTO> saptamani)
        {
            var docRef = _firestore.Collection("paritateSapt").Document(parId);

            var saptamaniDictionaries = saptamani.Select(s => new Dictionary<string, object>
            {
                { "sapt", s.Sapt },
                { "data", s.Data },
                { "paritate", s.Paritate }
            }).ToArray();

            var updates = new Dictionary<string, object>
            {
                { "saptamani", FieldValue.ArrayUnion(saptamaniDictionaries) }
            };

            await docRef.SetAsync(updates, SetOptions.MergeAll);
        }


        public async Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string parId)
        {
            var docRef = _firestore.Collection("paritateSapt").Document(parId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists || !snapshot.ContainsField("saptamani"))
                return new List<ParitateSaptamanaDTO>();

            var saptamaniRaw = snapshot.GetValue<List<Dictionary<string, object>>>("saptamani");

            return saptamaniRaw.Select(s => new ParitateSaptamanaDTO
            {
                Sapt = s.ContainsKey("sapt") ? s["sapt"].ToString() : "",
                Data = s.ContainsKey("data") ? s["data"].ToString() : "",
                Paritate = s.ContainsKey("paritate") ? s["paritate"].ToString() : ""
            }).ToList();
        }
    }
}

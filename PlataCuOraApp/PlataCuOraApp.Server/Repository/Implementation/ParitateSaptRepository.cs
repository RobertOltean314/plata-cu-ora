using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Repository.Interfaces;

namespace PlataCuOraApp.Server.Repositories
{
    public class ParitateSaptRepository : IParitateSaptRepository
    {
        private readonly FirestoreDb _firestore;
        private readonly ILogger<ParitateSaptRepository> _logger;

        public ParitateSaptRepository(FirestoreDb firestore, ILogger<ParitateSaptRepository> logger)
        {
            _firestore = firestore;
            _logger = logger;
        }

        private const string COLLECTION = "paritateSapt";

        public async Task AddOrUpdateParitateSaptAsync(string userId, List<ParitateSaptamanaDTO> saptamani)
        {
            _logger.LogInformation("Adding or updating parity weeks for userId={userId}", userId);
            var docRef = _firestore.Collection(COLLECTION).Document(userId);

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
            _logger.LogInformation("Successfully added or updated parity weeks for userId={userId}", userId);
        }

        public async Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string userId)
        {
            _logger.LogInformation("Fetching parity weeks for userId={userId}", userId);
            var docRef = _firestore.Collection(COLLECTION).Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists || !snapshot.ContainsField("saptamani"))
            {
                _logger.LogWarning("No parity data found for userId={userId}", userId);
                return new List<ParitateSaptamanaDTO>();
            }

            var saptamaniRaw = snapshot.GetValue<List<Dictionary<string, object>>>("saptamani");

            var result = saptamaniRaw.Select(s => new ParitateSaptamanaDTO
            {
                Sapt = s.ContainsKey("sapt") ? s["sapt"].ToString() : string.Empty,
                Data = s.ContainsKey("data") ? s["data"].ToString() : string.Empty,
                Paritate = s.ContainsKey("paritate") ? s["paritate"].ToString() : string.Empty
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} parity week entries for userId={userId}", result.Count, userId);
            return result;
        }

        public async Task<bool> UpdateParitateAsync(string userId, ParitateSaptamanaDTO oldEntry, ParitateSaptamanaDTO newEntry)
        {
            _logger.LogInformation("Updating parity week for userId={userId}", userId);
            var list = await GetParitateSaptAsync(userId);
            if (!list.RemoveAll(x => x.Sapt == oldEntry.Sapt && x.Data == oldEntry.Data && x.Paritate == oldEntry.Paritate).Equals(1))
            {
                _logger.LogWarning("Old parity entry not found for update: {OldEntry}", oldEntry);
                return false;
            }

            list.Add(newEntry);
            await _firestore.Collection(COLLECTION).Document(userId).SetAsync(new { saptamani = list });
            _logger.LogInformation("Successfully updated parity entry for userId={userId}", userId);
            return true;
        }

        public async Task<bool> DeleteParitateAsync(string userId, ParitateSaptamanaDTO entry)
        {
            _logger.LogInformation("Deleting parity entry for userId={userId}", userId);
            var list = await GetParitateSaptAsync(userId);
            if (!list.RemoveAll(x => x.Sapt == entry.Sapt && x.Data == entry.Data && x.Paritate == entry.Paritate).Equals(1))
            {
                _logger.LogWarning("Entry to delete not found for userId={userId}: {Entry}", userId, entry);
                return false;
            }

            await _firestore.Collection(COLLECTION).Document(userId).SetAsync(new { saptamani = list });
            _logger.LogInformation("Successfully deleted parity entry for userId={userId}", userId);
            return true;
        }
    }
}

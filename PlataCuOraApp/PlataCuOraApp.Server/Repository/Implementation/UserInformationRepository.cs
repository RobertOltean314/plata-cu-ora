using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using System.Text.Json;

namespace PlataCuOraApp.Server.Repository.Implementation
{
    public class UserInformationRepository : IUserInformationRepository
    {
        private readonly FirestoreDb _db;
        private readonly ILogger<UserInformationRepository> _logger;
        private const string COLLECTION = "infosUsers";

        public UserInformationRepository(FirestoreDb db, ILogger<UserInformationRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<UserInformationDTO>> GetAllInfoAsync(string userId)
        {
            var doc = await _db.Collection(COLLECTION).Document(userId).GetSnapshotAsync();
            if (!doc.Exists)
            {
                _logger.LogWarning($"Document for user {userId} not found.");
                return new List<UserInformationDTO>();
            }

            if (doc.TryGetValue<object>("infoList", out var data))
            {
                var json = JsonSerializer.Serialize(data);
                var list = JsonSerializer.Deserialize<List<UserInformationDTO>>(json);
                return list ?? new List<UserInformationDTO>();
            }

            return new List<UserInformationDTO>();
        }

        public async Task<bool> UpdateAllInfoAsync(string userId, List<UserInformationDTO> infoList)
        {
            try
            {
                await _db.Collection(COLLECTION).Document(userId).SetAsync(new { infoList });
                _logger.LogInformation($"Updated infoList for user {userId}.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update infoList for user {userId}.");
                return false;
            }
        }

        public async Task<bool> AddInfoAsync(string userId, UserInformationDTO newInfo)
        {
            var list = await GetAllInfoAsync(userId);

            if (list.Any(i => JsonSerializer.Serialize(i) == JsonSerializer.Serialize(newInfo)))
            {
                _logger.LogWarning($"Duplicate info entry for user {userId}, skipping add.");
                return false;
            }

            if (newInfo.IsActive)
            {
                foreach (var item in list)
                {
                    item.IsActive = false;
                }
            }

            list.Add(newInfo);

            return await UpdateAllInfoAsync(userId, list);
        }


        public async Task<bool> UpdateInfoAsync(string userId, UserInformationDTO oldInfo, UserInformationDTO newInfo)
        {
            var list = await GetAllInfoAsync(userId);

            var index = list.FindIndex(i => JsonSerializer.Serialize(i) == JsonSerializer.Serialize(oldInfo));
            if (index == -1)
            {
                _logger.LogWarning($"Old info entry not found for user {userId}, update failed.");
                return false;
            }

            if (newInfo.IsActive)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].IsActive = i == index; 
                }
            }
            else
            {
                newInfo.IsActive = list[index].IsActive;
            }

            list[index] = newInfo;

            return await UpdateAllInfoAsync(userId, list);
        }

        public async Task<bool> DeleteInfoAsync(string userId, UserInformationDTO info)
        {
            var list = await GetAllInfoAsync(userId);

            var index = list.FindIndex(i => JsonSerializer.Serialize(i) == JsonSerializer.Serialize(info));
            if (index == -1)
            {
                _logger.LogWarning($"Info entry to delete not found for user {userId}.");
                return false;
            }

            list.RemoveAt(index);

            return await UpdateAllInfoAsync(userId, list);
        }

        public async Task<bool> SetActiveAsync(string userId, UserInformationDTO activeInfo)
        {
            var list = await GetAllInfoAsync(userId);

            var index = list.FindIndex(i => JsonSerializer.Serialize(i) == JsonSerializer.Serialize(activeInfo));
            if (index == -1)
            {
                _logger.LogWarning($"Info entry to set active not found for user {userId}.");
                return false;
            }

            list[index].IsActive = true;

            return await UpdateAllInfoAsync(userId, list);
        }
        public async Task<bool> UnsetActiveAsync(string userId, UserInformationDTO info)
        {
            var list = await GetAllInfoAsync(userId);

            var index = list.FindIndex(i => JsonSerializer.Serialize(i) == JsonSerializer.Serialize(info));
            if (index == -1)
            {
                _logger.LogWarning($"Info entry to unset active not found for user {userId}.");
                return false;
            }

            list[index].IsActive = false;

            return await UpdateAllInfoAsync(userId, list);
        }

        public async Task<UserInformationDTO?> AddActiveInfoToDbAsync(string userId)
        {
            var list = await GetAllInfoAsync(userId);
            var activeInfo = list.FirstOrDefault(i => i.IsActive);

            if (activeInfo != null)
            {
                try
                {
                    DocumentReference docRef = _db.Collection("infoUsers").Document(userId);
                    await docRef.SetAsync(activeInfo);
                    _logger.LogInformation($"Active info for user {userId} copied to infoUsers.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to copy active info for user {userId}.");
                }
            }
            else
            {
                _logger.LogWarning($"No active info found for user {userId}.");
            }

            return activeInfo;
        }

        public async Task<UserInformationDTO?> GetInfoUserFromDbAsync(string userId)
        {
            try
            {
                DocumentReference docRef = _db.Collection("infoUsers").Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    var infoUser = snapshot.ConvertTo<UserInformationDTO>();
                    _logger.LogInformation($"Fetched infoUser for user {userId} from DB.");
                    return infoUser;
                }
                else
                {
                    _logger.LogWarning($"No document found in infoUsers for user {userId}.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch infoUser for user {userId}.");
                return null;
            }
        }

        public async Task<UserInformationDTO?> GetActiveInfoAsync(string userId)
        {
            var doc = await _db.Collection(COLLECTION).Document(userId).GetSnapshotAsync();
            if (!doc.Exists)
                return null;

            if (doc.TryGetValue<object>("infoList", out var data))
            {
                var json = JsonSerializer.Serialize(data);
                var list = JsonSerializer.Deserialize<List<UserInformationDTO>>(json);
                return list?.FirstOrDefault(i => i.IsActive);
            }

            return null;
        }


    }
}

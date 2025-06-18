using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using PlataCuOra.Server.Repository.Implementation;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repository.Interfaces;
using System.Text.Json;

public class UserScheduleRepository : IUserScheduleRepository
{
    private readonly FirestoreDb _db;
    private readonly ILogger<UserScheduleRepository> _logger;
    private const string COLLECTION = "orarUser";

    public UserScheduleRepository(FirestoreDb db, ILogger<UserScheduleRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<UserScheduleDTO>?> GetAllAsync(string userId)
    {
        _logger.LogInformation("Retrieving schedule for user: {UserId}", userId);
        var doc = await _db.Collection(COLLECTION).Document(userId).GetSnapshotAsync();
        if (!doc.Exists)
        {
            _logger.LogWarning("No schedule found for user: {UserId}", userId);
            return null;
        }

        if (doc.TryGetValue<object>("orar", out var data))
        {
            var json = JsonSerializer.Serialize(data);
            var userScheduleList = JsonSerializer.Deserialize<List<UserScheduleDTO>>(json);
            _logger.LogInformation("Successfully retrieved schedule for user: {UserId}", userId);
            return userScheduleList;
        }
        else
        {
            _logger.LogWarning("Schedule field 'orar' missing for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> AddAsync(string userId, UserScheduleDTO entry)
    {
        _logger.LogInformation("Attempting to add schedule entry for user: {UserId}", userId);
        var docRef = _db.Collection(COLLECTION).Document(userId);
        var doc = await docRef.GetSnapshotAsync();

        List<UserScheduleDTO> list = new();

        if (doc.Exists && doc.TryGetValue<object>("orar", out var data))
        {
            var json = JsonSerializer.Serialize(data);
            list = JsonSerializer.Deserialize<List<UserScheduleDTO>>(json) ?? new();

            if (list.Any(e => JsonSerializer.Serialize(e) == JsonSerializer.Serialize(entry)))
            {
                _logger.LogWarning("Duplicate entry detected. Skipping add for user: {UserId}", userId);
                return false;
            }

            list.Add(entry);
            await docRef.SetAsync(new { orar = list });
            _logger.LogInformation("Appended new unique schedule entry for user: {UserId}", userId);
        }
        else
        {
            list.Add(entry);
            await docRef.CreateAsync(new { orar = list });
            _logger.LogInformation("Created new schedule document for user: {UserId}", userId);
        }

        return true;
    }

    public async Task<bool> UpdateAsync(string userId, UserScheduleDTO oldEntry, UserScheduleDTO newEntry)
    {
        _logger.LogInformation("Updating schedule entry for user: {UserId}", userId);
        var list = await GetAllAsync(userId);
        if (list == null || !list.Remove(oldEntry))
        {
            _logger.LogWarning("Entry to update not found for user: {UserId}", userId);
            return false;
        }

        list.Add(newEntry);
        await _db.Collection(COLLECTION).Document(userId).SetAsync(new { orar = list });
        _logger.LogInformation("Successfully updated schedule entry for user: {UserId}", userId);
        return true;
    }

    public async Task<bool> DeleteAsync(string userId, UserScheduleDTO entry)
    {
        _logger.LogInformation("Deleting schedule entry for user: {UserId}", userId);
        var list = await GetAllAsync(userId);
        if (list == null || !list.Remove(entry))
        {
            _logger.LogWarning("Entry to delete not found for user: {UserId}", userId);
            return false;
        }

        await _db.Collection(COLLECTION).Document(userId).SetAsync(new { orar = list });
        _logger.LogInformation("Successfully deleted schedule entry for user: {UserId}", userId);
        return true;
    }
}

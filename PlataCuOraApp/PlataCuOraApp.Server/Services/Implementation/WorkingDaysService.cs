using Google.Cloud.Firestore;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;
using System.Globalization;
using System.Text.Json;

namespace PlataCuOraApp.Server.Services
{
    public class WorkingDaysService : IWorkingDaysService
    {
        private readonly FirestoreDb _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkingDaysService(FirestoreDb db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<WorkingDayDTO>> GetWorkingDaysAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var workingDays = new List<WorkingDayDTO>();

            var docRef = _db.Collection("paritateSapt").Document(userId);
            var docSnap = await docRef.GetSnapshotAsync();

            if (!docSnap.Exists ||
                !docSnap.TryGetValue("saptamani", out object saptamaniObj) ||
                saptamaniObj == null ||
                !(saptamaniObj is IEnumerable<object> saptamaniEnumerable))
            {
                for (DateTime day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
                {
                    workingDays.Add(new WorkingDayDTO
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        DayOfWeek = day.DayOfWeek.ToString(),
                        IsWorkingDay = false,
                        Parity = ""
                    });
                }
                return workingDays;
            }

            var saptamaniList = new List<Dictionary<string, object>>();
            foreach (var item in saptamaniEnumerable)
            {
                if (item is Dictionary<string, object> dict)
                    saptamaniList.Add(dict);
            }

            for (DateTime day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                {
                    workingDays.Add(new WorkingDayDTO
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        DayOfWeek = day.DayOfWeek.ToString(),
                        IsWorkingDay = false,
                        Parity = ""
                    });
                    continue;
                }

                Dictionary<string, object> matchedWeek = null;

                foreach (var sapt in saptamaniList)
                {
                    if (!sapt.TryGetValue("data", out var dataVal)) continue;
                    if (!DateTime.TryParseExact(dataVal.ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var weekStart))
                        continue;

                    var weekEnd = weekStart.AddDays(6); 

                    if (day >= weekStart && day <= weekEnd)
                    {
                        matchedWeek = sapt;
                        break;
                    }
                }

                if (matchedWeek == null)
                {
                    workingDays.Add(new WorkingDayDTO
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        DayOfWeek = day.DayOfWeek.ToString(),
                        IsWorkingDay = false,
                        Parity = ""
                    });
                    continue;
                }

                string paritate = matchedWeek.TryGetValue("paritate", out var p) ? p.ToString() : "";

                workingDays.Add(new WorkingDayDTO
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    DayOfWeek = day.DayOfWeek.ToString(),
                    IsWorkingDay = true,
                    Parity = paritate
                });
            }

            return workingDays;
        }
    }
}
using Google.Cloud.Firestore;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Services.Interfaces;
using System.Globalization;

namespace PlataCuOraApp.Server.Services
{
    public class WorkingDaysService : IWorkingDaysService
    {
        private readonly FirestoreDb _db;
        private readonly IHolidaysService _holidaysService;

        public WorkingDaysService(FirestoreDb db, IHolidaysService holidaysService)
        {
            _db = db;
            _holidaysService = holidaysService;
        }

        public async Task<List<WorkingDayDTO>> GetWorkingDaysAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var workingDays = new List<WorkingDayDTO>();
            var holidayDates = new HashSet<DateTime>();

            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                var holidays = await _holidaysService.GetHolidaysAsync(year);
                foreach (var holiday in holidays)
                {
                    if (holiday.Dates != null)
                    {
                        foreach (var dateDetail in holiday.Dates)
                        {
                            var formats = new[] { "yyyy-MM-dd", "yyyy/MM/dd" };
                            if (DateTime.TryParseExact(dateDetail.Date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                            {
                                holidayDates.Add(dt.Date);
                            }
                        }
                    }
                }
            }

            var docRef = _db.Collection("paritateSapt").Document(userId);
            var docSnap = await docRef.GetSnapshotAsync();

            List<Dictionary<string, object>> saptamaniList = new();

            if (docSnap.Exists &&
                docSnap.TryGetValue("saptamani", out object saptamaniObj) &&
                saptamaniObj is IEnumerable<object> saptamaniEnumerable)
            {
                foreach (var item in saptamaniEnumerable)
                {
                    if (item is Dictionary<string, object> dict)
                        saptamaniList.Add(dict);
                }
            }

            for (DateTime day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                bool isWeekend = day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
                bool isHoliday = holidayDates.Contains(day.Date);

                if (isWeekend || isHoliday)
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

                string paritate = matchedWeek?.TryGetValue("paritate", out var p) == true ? p.ToString() : "";

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

using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services;
using static iTextSharp.text.pdf.AcroFields;

public class DeclarationService : IDeclarationService
{
    private readonly IUserInformationRepository _userInformationRepository;
    private readonly IUserScheduleRepository _userScheduleRepository;
    private readonly IWeekParityRepository _weekParityRepository;
    private readonly ILogger<DeclarationService> _logger;

    public DeclarationService(
        IUserInformationRepository userInformationRepository,
        IUserScheduleRepository userScheduleRepository,
        IWeekParityRepository weekParityRepository,
        ILogger<DeclarationService> logger)
    {
        _userInformationRepository = userInformationRepository;
        _userScheduleRepository = userScheduleRepository;
        _weekParityRepository = weekParityRepository;
        _logger = logger;

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    public async Task<byte[]> GenerateDeclarationAsync(string userId, List<DateTime> workedDays)
    {
        _logger.LogInformation("Starting declaration generation for userId={UserId}, days={Zile}", userId, workedDays);

        var user = await _userInformationRepository.GetActiveInfoAsync(userId);
        if (user == null)
            throw new Exception("User not found.");


        var orar = await _userScheduleRepository.GetAllAsync(userId);
        if (orar == null || !orar.Any())
            throw new Exception("No schedule exists for this user.");

        var paritati = await _weekParityRepository.GetWeekParityAsync(userId) ?? new List<WeekParityDTO>();
        if (paritati == null || !paritati.Any())
            throw new Exception("No parity data exists for this user.");

        var userDto = new UserInformationDTO
        {
            Declarant = user.Declarant,
            Tip = user.Tip,
            DirectorDepartament = user.DirectorDepartament,
            Decan = user.Decan,
            Universitate = user.Universitate,
            Facultate = user.Facultate,
            Departament = user.Departament
        };

        var pdf = GeneratePdfDocument(userDto, orar, workedDays, paritati);

        _logger.LogInformation("Declaration PDF generated successfully for userId={UserId}", userId);
        return pdf;
    }

    private byte[] GeneratePdfDocument(UserInformationDTO user, List<UserScheduleDTO> ore, List<DateTime> zileLucrate, List<WeekParityDTO> paritati)
    {
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        AddHeader(doc, user);
        AddTitleAndParagraph(doc, user, zileLucrate);
        AddScheduleTable(doc, ore, zileLucrate, paritati);
        AddCoefficientsTable(doc);
        AddParagraphLegal(doc);
        AddSognatures(doc, user);

        doc.Close();
        return ms.ToArray();
    }

    private IEnumerable<T> FilterHoursGeneric<T>(
    List<UserScheduleDTO> schedule,
    List<WeekParityDTO> weekParities,
    List<DateTime> workedDays,
    Func<UserScheduleDTO, DateTime, T> selector)
    {
        var hoursGloballyConsumed = new Dictionary<(int NrPost, string Formatia, string Tip, string SaptInceput), int>();
        var hoursPerDay = new Dictionary<DateTime, int>();

        foreach (var day in workedDays.OrderBy(z => z))
        {
            var weekDay = day.DayOfWeek;

            var schedulePerDay = schedule.Where(o =>
                ConvertDayTextToDayOfWeek(o.Ziua) == weekDay &&
                !string.IsNullOrWhiteSpace(o.Formatia) &&
                new[] { o.OreCurs, o.OreSem, o.OreLab, o.OreProi }.Count(x => x > 0) == 1 &&
                (string.IsNullOrWhiteSpace(o.ImparPar) || CheckWeekParity(weekParities, day, o.ImparPar))
            ).ToList();

            var special = schedulePerDay
                .Where(o => !string.IsNullOrWhiteSpace(o.SaptamanaInceput) && o.TotalOre > 0)
                .GroupBy(o => new { o.NrPost, o.Formatia, o.Tip, o.SaptamanaInceput, o.ImparPar });

            foreach (var group in special)
            {
                if (!int.TryParse(group.Key.SaptamanaInceput.Replace("S", ""), out int startWeek))
                    continue;

                var currentWeek = CalculateWeekNumber(day, weekParities);
                if (!currentWeek.HasValue || currentWeek.Value < startWeek)
                    continue;

                var key = (group.Key.NrPost, group.Key.Formatia, group.Key.Tip, group.Key.SaptamanaInceput);
                hoursGloballyConsumed.TryGetValue(key, out int alreadyConsumed);

                int scheduleHours = group.Sum(o => o.OreCurs + o.OreSem + o.OreLab + o.OreProi);
                if (scheduleHours == 0) continue;

                int availableHours = CalculateRemainingHoursInWeek(currentWeek.Value, startWeek, group.Key.ImparPar, group.First().TotalOre, scheduleHours);
                int maxAllowedHours = group.First().TotalOre - alreadyConsumed;

                if (maxAllowedHours <= 0)
                {
                    foreach (var o in group)
                        if (o.OreCurs + o.OreSem + o.OreLab + o.OreProi > 0)
                            yield return selector(o, day);
                    continue;
                }

                if (availableHours <= 0) continue;

                int toAdd = Math.Min(availableHours, maxAllowedHours);
                int allocated = 0;

                foreach (var o in group)
                {
                    int physicalHours = o.OreCurs + o.OreSem + o.OreLab + o.OreProi;
                    if (physicalHours == 0 || allocated >= toAdd) continue;

                    int alocHere = Math.Min(physicalHours, toAdd - allocated);
                    hoursPerDay.TryGetValue(day.Date, out int totalInDay);

                    if (totalInDay + alocHere > 12) break;

                    if (o.OreCurs > 0) o.OreCurs = alocHere;
                    if (o.OreSem > 0) o.OreSem = alocHere;
                    if (o.OreLab > 0) o.OreLab = alocHere;
                    if (o.OreProi > 0) o.OreProi = alocHere;

                    hoursPerDay[day.Date] = totalInDay + alocHere;
                    yield return selector(o, day);
                    allocated += alocHere;
                }

                hoursGloballyConsumed[key] = alreadyConsumed + allocated;
            }

            var normal = schedulePerDay
                .Where(o => string.IsNullOrWhiteSpace(o.SaptamanaInceput) || o.TotalOre <= 0);

            foreach (var o in normal)
            {
                int phisicalHours = o.OreCurs + o.OreSem + o.OreLab + o.OreProi;
                if (phisicalHours == 0) continue;

                hoursPerDay.TryGetValue(day.Date, out int totalInZi);
                if (totalInZi + phisicalHours > 12) continue;

                hoursPerDay[day.Date] = totalInZi + phisicalHours;
                yield return selector(o, day);
            }
        }
    }

    private void AddHeader(Document doc, UserInformationDTO user)
    {
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

        var headerTable = new PdfPTable(2);
        headerTable.WidthPercentage = 100;
        headerTable.SetWidths(new float[] { 70, 30 });

        var leftHeader = new PdfPTable(1);
        leftHeader.AddCell(new PdfPCell(new Phrase(user.Universitate ?? "", fontNormal)) { Border = 0 });
        leftHeader.AddCell(new PdfPCell(new Phrase(user.Facultate ?? "", fontNormal)) { Border = 0 });
        leftHeader.AddCell(new PdfPCell(new Phrase(user.Departament ?? "", fontNormal)) { Border = 0 });

        var rightHeader = new PdfPTable(1);
        rightHeader.AddCell(new PdfPCell(new Phrase("Aprobat,", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
        rightHeader.AddCell(new PdfPCell(new Phrase("Decan", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
        rightHeader.AddCell(new PdfPCell(new Phrase(user.Decan ?? "", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        headerTable.AddCell(new PdfPCell(leftHeader) { Border = 0 });
        headerTable.AddCell(new PdfPCell(rightHeader) { Border = 0 });

        doc.Add(headerTable);
        doc.Add(new Paragraph(" "));
    }

    private void AddTitleAndParagraph(Document doc, UserInformationDTO user, List<DateTime> workedDays)
    {
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var fontBoldUnderline = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        fontBoldUnderline.SetStyle(Font.UNDERLINE);

        var title = new Paragraph("DECLARATIE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12))
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(title);

        var firstDay = workedDays.First();
        var dataStart = new DateTime(firstDay.Year, firstDay.Month, 1).ToString("dd.MM.yyyy");
        var dataEnd = new DateTime(firstDay.Year, firstDay.Month, DateTime.DaysInMonth(firstDay.Year, firstDay.Month)).ToString("dd.MM.yyyy");

        var paragraph = new Paragraph();
        paragraph.Add(new Chunk("Subsemnatul(a), ", fontNormal));
        paragraph.Add(new Chunk(user.Declarant ?? "", fontBoldUnderline));
        paragraph.Add(new Chunk(", am suplinit in intervalul ", fontNormal));
        paragraph.Add(new Chunk($"{dataStart} - {dataEnd}", fontBoldUnderline));
        paragraph.Add(new Chunk($" in {user.Departament ?? ""} activitati didactice dupa cum urmeaza:", fontNormal));
        paragraph.SpacingBefore = 10f;
        paragraph.SpacingAfter = 10f;
        doc.Add(paragraph);
    }

    private void AddCoefficientsTable(Document doc)
    {
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

        var textCoef = new Paragraph("**Coeficientii sunt conform tabelului:", fontNormal)
        {
            Alignment = Element.ALIGN_LEFT,
            SpacingBefore = 10f,
            SpacingAfter = 5f
        };
        doc.Add(textCoef);

        var coefTable = new PdfPTable(3);
        coefTable.WidthPercentage = 30;
        coefTable.HorizontalAlignment = Element.ALIGN_LEFT;
        coefTable.SetWidths(new float[] { 15, 10, 10 });

        coefTable.AddCell(new PdfPCell(new Phrase("Coeficienti", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        coefTable.AddCell(new PdfPCell(new Phrase("Curs", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        coefTable.AddCell(new PdfPCell(new Phrase("S/L/P/A", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });

        void AddCoefRow(string tip, string curs, string rest)
        {
            coefTable.AddCell(new PdfPCell(new Phrase(tip, fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
            coefTable.AddCell(new PdfPCell(new Phrase(curs, fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
            coefTable.AddCell(new PdfPCell(new Phrase(rest, fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
        }

        AddCoefRow("LR - Licenta romana", "2", "1");
        AddCoefRow("LE - Licenta engleza", "2,5", "1,25");
        AddCoefRow("MR - Master romana", "2,5", "1,5");
        AddCoefRow("ME - Master engleza", "3,125", "1,875");

        doc.Add(coefTable);
        doc.Add(new Paragraph("\n"));
    }

    private void AddParagraphLegal(Document doc)
    {
        var font = FontFactory.GetFont(FontFactory.HELVETICA, 8);
        var paragraphLong = new Paragraph("Subsemnatul/Subsemnata, cunoscand prevederile art. 326 din Codul Penal cu privire la falsul în declaratii, declar pe propria raspundere ca in luna pentru care fac prezenta declaratie de plata cu ora, in afara functiei/normei de baza, am desfasurat activitati in regim de plata cu ora si/sau cu contract individual de munca cu timp partial si/sau activitati in cadrul proiectelor respectand legislatia muncii cu privire la numarul maxim de ore ce pot fi efectuate in cadrul activitatilor în afara functiei de baza/normei de baza, fara a depasi o medie de 4 ore/zi, respectiv o medie de 84 ore/luna in anul universitar, cu respectarea duratei zilnice si saptamanale maxime legale a timpului de munca si a perioadelor minime de repaus zilnic si saptamanal. De asemenea, declar ca sunt de acord sa pun la dispozitia institutiilor abilitate, la solicitarea acestora, documentele doveditoare in scopul verificarii si confirmarii informatiilor furnizare prin aceasta declaratie.", font)
        {
            SpacingBefore = 5f,
            SpacingAfter = 10f
        };
        doc.Add(paragraphLong);

        var paragraphShort = new Paragraph("Se certifica de noi ca orele declarate mai sus au fost efectuate de declarant intocmai.")
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(paragraphShort);
        doc.Add(new Paragraph("\n\n"));
    }

    private void AddSognatures(Document doc, UserInformationDTO user)
    {
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var signaturesTable = new PdfPTable(2);
        signaturesTable.WidthPercentage = 100;
        signaturesTable.SetWidths(new float[] { 50, 50 });

        signaturesTable.AddCell(new PdfPCell(new Phrase("DIRECTOR DEPARTAMENT", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
        signaturesTable.AddCell(new PdfPCell(new Phrase("DECLARANT", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        signaturesTable.AddCell(new PdfPCell(new Phrase(" ")) { Border = 0, MinimumHeight = 20f });
        signaturesTable.AddCell(new PdfPCell(new Phrase(" ")) { Border = 0, MinimumHeight = 20f });

        signaturesTable.AddCell(new PdfPCell(new Phrase(user.DirectorDepartament ?? "", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
        signaturesTable.AddCell(new PdfPCell(new Phrase(user.Declarant ?? "", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        doc.Add(signaturesTable);
    }

    private void AddScheduleTable(Document doc, List<UserScheduleDTO> hours, List<DateTime> weekDays, List<WeekParityDTO> weekParities)
    {
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

        var table = new PdfPTable(10) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 9, 10, 5, 5, 5, 5, 5, 5, 6, 25 });

        table.AddCell(new PdfPCell(new Phrase("Poz. din stat", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Data", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Numar ore fizice", fontBold)) { Colspan = 4, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Tip", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Coef.", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Nr. ore", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Anul, grupa, semigrupa", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });

        foreach (var sub in new[] { "C", "S", "L/A", "P" })
            table.AddCell(new PdfPCell(new Phrase(sub, fontBold)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });

        int totalC = 0, totalS = 0, totalLA = 0, totalP = 0;
        double totalHoursCoefficient = 0;

        var HoursPerDayPost = FilterHoursGeneric(hours, weekParities, weekDays, (o, zi) => new
        {
            Zi = zi.Date,
            o.NrPost,
            o.DenPost,
            o.Tip,
            o.Formatia,
            o.OreCurs,
            o.OreSem,
            o.OreLab,
            o.OreProi
        })
        .Distinct()
        .OrderBy(x => x.NrPost)
        .ThenBy(x => x.Zi)
        .ToList();

        var finalGroups = HoursPerDayPost
        .GroupBy(x => x.NrPost + " " + x.DenPost)
        .ToList();

        foreach (var group in finalGroups)
        {
            var rows = group.ToList();
            bool isFirst = true;
            int rowspan = rows.Count;

            foreach (var rand in rows)
            {
                var day = rand.Zi;
                var type = rand.Tip;

                int c = rand.OreCurs;
                int s = rand.OreSem;
                int la = rand.OreLab;
                int p = rand.OreProi;

                double coef = type switch
                {
                    "LR" => c > 0 ? 2.0 : 1.0,
                    "LE" => c > 0 ? 2.5 : 1.25,
                    "MR" => c > 0 ? 2.5 : 1.5,
                    "ME" => c > 0 ? 3.125 : 1.875,
                    _ => 1.0
                };

                double totalHours = (c + s + la + p) * coef;

                totalC += c;
                totalS += s;
                totalLA += la;
                totalP += p;
                totalHoursCoefficient += totalHours;

                if (isFirst)
                {
                    table.AddCell(new PdfPCell(new Phrase(group.Key, fontNormal))
                    {
                        Rowspan = rowspan,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    isFirst = false;
                }


                table.AddCell(new PdfPCell(new Phrase(day.ToString("dd.MM.yyyy"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(c > 0 ? c.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(s > 0 ? s.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(la > 0 ? la.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(p > 0 ? p.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(type, fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(coef.ToString("0.###"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(totalHours.ToString("0.###"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(rand.Formatia, fontNormal)));

            }
        }

        int totalGeneral = totalC + totalS + totalLA + totalP;

        PdfPCell emptyCell = new PdfPCell(new Phrase(""))
        {
            BackgroundColor = BaseColor.LIGHT_GRAY,
            Border = Rectangle.BOX
        };

        table.AddCell(new PdfPCell(new Phrase("TOTAL", fontBold)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase(totalC.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase(totalS.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase(totalLA.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase(totalP.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(emptyCell);
        table.AddCell(emptyCell);
        table.AddCell(new PdfPCell(new Phrase(totalHoursCoefficient.ToString("0.###"), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(emptyCell);


        PdfPCell totalFinalCell = new PdfPCell(new Phrase(totalGeneral.ToString(), fontBold))
        {
            Colspan = 4,
            HorizontalAlignment = Element.ALIGN_CENTER,
            BackgroundColor = BaseColor.LIGHT_GRAY,
            Border = Rectangle.BOX
        };

        PdfPCell noBorderCell = new PdfPCell(new Phrase(""))
        {
            Border = Rectangle.NO_BORDER
        };

        table.AddCell(noBorderCell);
        table.AddCell(noBorderCell);
        table.AddCell(totalFinalCell);
        table.AddCell(noBorderCell);
        table.AddCell(noBorderCell);
        table.AddCell(noBorderCell);
        table.AddCell(noBorderCell);

        doc.Add(table);
    }

    private static DayOfWeek? ConvertDayTextToDayOfWeek(string day) => day.Trim().ToLower() switch
    {
        "luni" => DayOfWeek.Monday,
        "marti" or "marți" => DayOfWeek.Tuesday,
        "miercuri" => DayOfWeek.Wednesday,
        "joi" => DayOfWeek.Thursday,
        "vineri" => DayOfWeek.Friday,
        "sambata" or "sâmbătă" => DayOfWeek.Saturday,
        "duminica" or "duminică" => DayOfWeek.Sunday,
        _ => null
    };

    private static bool CheckWeekParity(List<WeekParityDTO> weekParities, DateTime day, string parityType)
    {
        var dateDay = day.Date;

        foreach (var p in weekParities)
        {
            if (!DateTime.TryParseExact(p.Data, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataStart))
                continue;

            var start = dataStart.Date;
            var end = start.AddDays(7).Date;

            if (dateDay >= start && dateDay < end &&
                p.Paritate.Equals(parityType, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private int CalculateRemainingHoursInWeek(
    int currentWeek,
    int startWeek,
    string parity,
    int totalHours,
    int scheduleHours,
    int maxWeeks = 10)
    {
        int diff = currentWeek - startWeek;
        int weeksPassed;

        if (diff < 0)
        {
            return scheduleHours;
        }

        if (string.IsNullOrWhiteSpace(parity))
        {
            weeksPassed = diff;
        }
        else
        {
            weeksPassed = diff / 2;

            if (diff % 2 == 1)
            {
                if ((parity.Equals("Par", StringComparison.OrdinalIgnoreCase) && startWeek % 2 == 0) ||
                    (parity.Equals("Impar", StringComparison.OrdinalIgnoreCase) && startWeek % 2 == 1))
                {
                    weeksPassed++;
                }
            }
        }

        if (weeksPassed >= maxWeeks)
        {
            return -1;
        }

        int remainingHours = totalHours - weeksPassed * scheduleHours;
        int hoursToReturn = Math.Min(remainingHours, scheduleHours);

        return hoursToReturn > 0 ? hoursToReturn : -1;
    }

    private static int? CalculateWeekNumber(DateTime day, List<WeekParityDTO> weekParities)
    {
        for (int i = 0; i < weekParities.Count; i++)
        {
            if (DateTime.TryParseExact(weekParities[i].Data, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataStart))
            {
                if (day >= dataStart && day < dataStart.AddDays(7))
                {
                    if (int.TryParse(weekParities[i].Sapt.Replace("S", ""), out var sapt))
                        return sapt;
                }
            }
        }
        return null;
    }

    // Metoda pentru testare

    public List<(DateTime ziua, string tip, int c, int s, int la, int p, double coef, double total)> TestCalculateFinalRows(
        List<UserScheduleDTO> schedule,
        List<WeekParityDTO> parities,
        List<DateTime> workedDays)
    {
        var rows = FilterHoursGeneric(schedule, parities, workedDays, (o, zi) => new
        {
            Zi = zi.Date,
            Tip = o.Tip,
            C = o.OreCurs,
            S = o.OreSem,
            LA = o.OreLab,
            P = o.OreProi
        }).ToList();

        return rows.Select(r =>
        {
            double coef = r.Tip switch
            {
                "LR" => r.C > 0 ? 2.0 : 1.0,
                "LE" => r.C > 0 ? 2.5 : 1.25,
                "MR" => r.C > 0 ? 2.5 : 1.5,
                "ME" => r.C > 0 ? 3.125 : 1.875,
                _ => 1.0
            };

            double total = (r.C + r.S + r.LA + r.P) * coef;

            return (r.Zi, r.Tip, r.C, r.S, r.LA, r.P, coef, total);
        }).ToList();
    }
}
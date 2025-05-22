using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services;
using static iTextSharp.text.pdf.AcroFields;

public class DeclaratieService : IDeclaratieService
{
    private readonly IInfoUserRepository _infoUserRepo;
    private readonly IOrarUserRepository _orarUserRepo;
    private readonly IParitateSaptRepository _paritateRepo;
    private readonly ILogger<DeclaratieService> _logger;

    public DeclaratieService(
        IInfoUserRepository infoUserRepo,
        IOrarUserRepository orarUserRepo,
        IParitateSaptRepository paritateRepo,
        ILogger<DeclaratieService> logger)
    {
        _infoUserRepo = infoUserRepo;
        _orarUserRepo = orarUserRepo;
        _paritateRepo = paritateRepo;
        _logger = logger;
    }

    public async Task<byte[]> GenereazaDeclaratieAsync(string userId, List<DateTime> zileLucrate)
    {
        _logger.LogInformation("Starting declaration generation for userId={UserId}, days={Zile}", userId, zileLucrate);

        var user = await _infoUserRepo.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError("User not found: {UserId}", userId);
            throw new Exception("User not found.");
        }

        var orar = await _orarUserRepo.GetAllAsync(userId);
        if (orar == null || !orar.Any())
        {
            _logger.LogWarning("Schedule not found for user: {UserId}", userId);
            throw new Exception("No schedule exists for this user.");
        }

        var paritati = await _paritateRepo.GetParitateSaptAsync(userId) ?? new List<ParitateSaptamanaDTO>();
        if (paritati == null || !paritati.Any())
        {
            _logger.LogWarning("Parity data not found for user: {UserId}", userId);
            throw new Exception("No parity data exists for this user.");
        }

        var oreFiltrate = FiltreazaOreGeneric(orar, paritati, zileLucrate, (o, _) => o).ToList();
        if (!oreFiltrate.Any())
        {
            _logger.LogWarning("No filtered hours found for user: {UserId}", userId);
            throw new Exception("No hours found for the specified days and criteria.");
        }

        var userDto = new InfoUserDTO
        {
            Declarant = user.Declarant,
            Tip = user.Tip,
            DirectorDepartament = user.DirectorDepartament,
            Decan = user.Decan,
            Universitate = user.Universitate,
            Facultate = user.Facultate,
            Departament = user.Departament
        };

        var pdf = GenereazaPdf(userDto, orar, zileLucrate, paritati);

        _logger.LogInformation("Declaration PDF generated successfully for userId={UserId}", userId);
        return pdf;
    }

    private byte[] GenereazaPdf(InfoUserDTO user, List<OrarUserDTO> ore, List<DateTime> zileLucrate, List<ParitateSaptamanaDTO> paritati)
    {
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        AdaugaHeader(doc, user);
        AdaugaTitluSiParagraf(doc, user, zileLucrate);
        AdaugaTabelOre(doc, ore, zileLucrate, paritati);
        AdaugaTabelCoeficienti(doc);
        AdaugaParagrafLegal(doc);
        AdaugaSemnaturi(doc, user);

        doc.Close();
        return ms.ToArray();
    }

    private IEnumerable<T> FiltreazaOreGeneric<T>(
        List<OrarUserDTO> orar,
        List<ParitateSaptamanaDTO> paritati,
        List<DateTime> zileLucrate,
        Func<OrarUserDTO, DateTime, T> selector)
    {
        foreach (var zi in zileLucrate)
        {
            var ziWeekDay = zi.DayOfWeek;
            foreach (var o in orar)
            {
                if (ConvertZiTextToDayOfWeek(o.Ziua) != ziWeekDay) 
                    continue;
                if (!string.IsNullOrEmpty(o.SaptamanaInceput) && zi < GetDataInceputSaptamana(paritati, o.SaptamanaInceput)) 
                    continue;
                if (!string.IsNullOrEmpty(o.ImparPar) && !VerificaParitate(paritati, zi, o.ImparPar)) 
                    continue;
                yield return selector(o, zi);
            }
        }
    }

    private void AdaugaHeader(Document doc, InfoUserDTO user)
    {
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

        var headerTable = new PdfPTable(2);
        headerTable.WidthPercentage = 100;
        headerTable.SetWidths(new float[] { 70, 30 });

        var stangaHeader = new PdfPTable(1);
        stangaHeader.AddCell(new PdfPCell(new Phrase(user.Universitate ?? "", fontNormal)) { Border = 0 });
        stangaHeader.AddCell(new PdfPCell(new Phrase(user.Facultate ?? "", fontNormal)) { Border = 0 });
        stangaHeader.AddCell(new PdfPCell(new Phrase(user.Departament ?? "", fontNormal)) { Border = 0 });

        var dreaptaHeader = new PdfPTable(1);
        dreaptaHeader.AddCell(new PdfPCell(new Phrase("Aprobat,", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
        dreaptaHeader.AddCell(new PdfPCell(new Phrase("Decan", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
        dreaptaHeader.AddCell(new PdfPCell(new Phrase(user.Decan ?? "", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        headerTable.AddCell(new PdfPCell(stangaHeader) { Border = 0 });
        headerTable.AddCell(new PdfPCell(dreaptaHeader) { Border = 0 });

        doc.Add(headerTable);
        doc.Add(new Paragraph(" "));
    }

    private void AdaugaTitluSiParagraf(Document doc, InfoUserDTO user, List<DateTime> zileLucrate)
    {
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var fontBoldUnderline = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        fontBoldUnderline.SetStyle(Font.UNDERLINE);

        var titlu = new Paragraph("DECLARATIE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12))
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(titlu);

        var primaZi = zileLucrate.First();
        var dataStart = new DateTime(primaZi.Year, primaZi.Month, 1).ToString("dd.MM.yyyy");
        var dataEnd = new DateTime(primaZi.Year, primaZi.Month, DateTime.DaysInMonth(primaZi.Year, primaZi.Month)).ToString("dd.MM.yyyy");

        var paragraf = new Paragraph();
        paragraf.Add(new Chunk("Subsemnatul(a), ", fontNormal));
        paragraf.Add(new Chunk(user.Declarant ?? "", fontBoldUnderline));
        paragraf.Add(new Chunk(", am suplinit in intervalul ", fontNormal));
        paragraf.Add(new Chunk($"{dataStart} - {dataEnd}", fontBoldUnderline));
        paragraf.Add(new Chunk($" in {user.Departament ?? ""} activitati didactice dupa cum urmeaza:", fontNormal));
        paragraf.SpacingBefore = 10f;
        paragraf.SpacingAfter = 10f;
        doc.Add(paragraf);
    }

    private void AdaugaTabelCoeficienti(Document doc)
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

    private void AdaugaParagrafLegal(Document doc)
    {
        var font = FontFactory.GetFont(FontFactory.HELVETICA, 8);
        var paragrafLung = new Paragraph("Subsemnatul/Subsemnata, cunoscând prevederile art. 326 din Codul Penal cu privire la falsul în declaraţii, declar pe propria răspundere că în luna pentru care fac prezenta declarație de plată cu ora, în afara funcției/normei de bază, am desfășurat activități în regim de plată cu ora și/sau cu contract individual de muncă cu timp parțial și/sau activități în cadrul proiectelor respectând legislația muncii cu privire la numărul maxim de ore ce pot fi efectuate în cadrul activităților în afara funcției de bază/normei de bază, fără a depăși o medie de 4 ore/zi, respectiv o medie de 84 ore/lunǎ în anul universitar, cu respectarea duratei zilnice și săptămânale maxime legale a timpului de muncă și a perioadelor minime de repaus zilnic și săptămânal. De asemenea, declar că sunt de acord să pun la dispoziția instituțiilor abilitate, la solicitarea acestora, documentele doveditoare în scopul verificării și confirmării informațiilor furnizare prin această declarație.", font)
        {
            SpacingBefore = 5f,
            SpacingAfter = 10f
        };
        doc.Add(paragrafLung);

        var paragrafScurt = new Paragraph("Se certifica de noi ca orele declarate mai sus au fost efectuate de declarant intocmai.")
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(paragrafScurt);
        doc.Add(new Paragraph("\n\n"));
    }

    private void AdaugaSemnaturi(Document doc, InfoUserDTO user)
    {
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var semnaturiTable = new PdfPTable(2);
        semnaturiTable.WidthPercentage = 100;
        semnaturiTable.SetWidths(new float[] { 50, 50 });

        semnaturiTable.AddCell(new PdfPCell(new Phrase("DIRECTOR DEPARTAMENT", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
        semnaturiTable.AddCell(new PdfPCell(new Phrase("DECLARANT", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        semnaturiTable.AddCell(new PdfPCell(new Phrase(" ")) { Border = 0, MinimumHeight = 20f });
        semnaturiTable.AddCell(new PdfPCell(new Phrase(" ")) { Border = 0, MinimumHeight = 20f });

        semnaturiTable.AddCell(new PdfPCell(new Phrase(user.DirectorDepartament ?? "", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
        semnaturiTable.AddCell(new PdfPCell(new Phrase(user.Declarant ?? "", fontBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        doc.Add(semnaturiTable);
    }

    private void AdaugaTabelOre(Document doc, List<OrarUserDTO> ore, List<DateTime> zileLucrate, List<ParitateSaptamanaDTO> paritati)
    {
        var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

        var table = new PdfPTable(10) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 9, 10, 5, 5, 5, 5, 5, 5, 6, 25 });

        table.AddCell(new PdfPCell(new Phrase("Poz. din stat", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Data", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Numar ore fizice", fontBold)) { Colspan = 4, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Tip", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Coef.**", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Nr. ore", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Anul, grupa, semigrupa", fontBold)) { Rowspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LIGHT_GRAY });

        foreach (var sub in new[] { "C", "S", "L/A", "P" })
            table.AddCell(new PdfPCell(new Phrase(sub, fontBold)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });

        int totalC = 0, totalS = 0, totalLA = 0, totalP = 0;
        double totalOreCoef = 0;

        var orePerZiPost = FiltreazaOreGeneric(ore, paritati, zileLucrate, (o, zi) => new
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

        var grupuriFinale = orePerZiPost
        .GroupBy(x => x.NrPost + " " + x.DenPost)
        .ToList();

        foreach (var grup in grupuriFinale)
        {
            var randuri = grup.ToList();
            bool isFirst = true;
            int rowspan = randuri.Count;

            foreach (var rand in randuri)
            {
                var zi = rand.Zi;
                var tip = rand.Tip;

                int c = rand.OreCurs;
                int s = rand.OreSem;
                int la = rand.OreLab;
                int p = rand.OreProi;

                double coef = tip switch
                {
                    "LR" => c > 0 ? 2.0 : 1.0,
                    "LE" => c > 0 ? 2.5 : 1.25,
                    "MR" => c > 0 ? 2.5 : 1.5,
                    "ME" => c > 0 ? 3.125 : 1.875,
                    _ => 1.0
                };

                double oreTotal = (c + s + la + p) * coef;

                totalC += c;
                totalS += s;
                totalLA += la;
                totalP += p;
                totalOreCoef += oreTotal;

                if (isFirst)
                {
                    table.AddCell(new PdfPCell(new Phrase(grup.Key, fontNormal))
                    {
                        Rowspan = rowspan,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    isFirst = false;
                }

                table.AddCell(new PdfPCell(new Phrase(zi.ToString("dd.MM.yyyy"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(c > 0 ? c.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(s > 0 ? s.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(la > 0 ? la.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(p > 0 ? p.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(tip, fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(coef.ToString("0.###"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(oreTotal.ToString("0.###"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(rand.Formatia, fontNormal)));
            }
        }

        table.AddCell(new PdfPCell(new Phrase("TOTAL", fontBold)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER });
        table.AddCell(new PdfPCell(new Phrase(totalC.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        table.AddCell(new PdfPCell(new Phrase(totalS.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        table.AddCell(new PdfPCell(new Phrase(totalLA.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        table.AddCell(new PdfPCell(new Phrase(totalP.ToString(), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        table.AddCell("");
        table.AddCell("");
        table.AddCell(new PdfPCell(new Phrase(totalOreCoef.ToString("0.###"), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
        table.AddCell("");

        doc.Add(table);
    }

    private static DayOfWeek? ConvertZiTextToDayOfWeek(string zi) => zi.Trim().ToLower() switch
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

    private static DateTime? GetDataInceputSaptamana(List<ParitateSaptamanaDTO> paritati, string sapt)
    {
        var p = paritati.FirstOrDefault(x => x.Sapt == sapt);
        return DateTime.TryParse(p?.Data, out var dt) ? dt : null;
    }

    private static bool VerificaParitate(List<ParitateSaptamanaDTO> paritati, DateTime zi, string imparPar)
    {
        foreach (var p in paritati)
        {
            if (!DateTime.TryParse(p.Data, out var dataStart)) continue;
            if (dataStart <= zi && zi < dataStart.AddDays(7) && p.Paritate.Equals(imparPar, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Domain.DTOs;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Repository.Interfaces;
using PlataCuOraApp.Server.Services;
using static iTextSharp.text.pdf.AcroFields;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;
using static QuestPDF.Helpers.Colors;
using System.Text;


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

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    public async Task<byte[]> GenereazaDeclaratieAsync(string userId, List<DateTime> zileLucrate, DateTime firstDay, DateTime lastDay)
    {
        _logger.LogInformation("Starting declaration generation for userId={UserId}, days={Zile}", userId, zileLucrate);

        var user = await _infoUserRepo.GetActiveInfoAsync(userId);
        if (user == null)
            throw new Exception("User not found.");


        var orar = await _orarUserRepo.GetAllAsync(userId);
        if (orar == null || !orar.Any())
            throw new Exception("No schedule exists for this user.");

        var paritati = await _paritateRepo.GetParitateSaptAsync(userId) ?? new List<ParitateSaptamanaDTO>();
        if (paritati == null || !paritati.Any())
            throw new Exception("No parity data exists for this user.");

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

        var pdf = GenereazaPdf(userDto, orar, zileLucrate, paritati, firstDay, lastDay);

        _logger.LogInformation("Declaration PDF generated successfully for userId={UserId}", userId);
        return pdf;
    }

    private byte[] GenereazaPdf(InfoUserDTO user, List<OrarUserDTO> ore, List<DateTime> zileLucrate, List<ParitateSaptamanaDTO> paritati, DateTime firstDay, DateTime lastDay)
    {
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        AdaugaHeader(doc, user);
        AdaugaTitluSiParagraf(doc, user, zileLucrate, firstDay, lastDay);
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
        var oreConsumatGlobal = new Dictionary<(int NrPost, string Formatia, string Tip, string SaptInceput), int>();
        var orePeZi = new Dictionary<DateTime, int>();

        foreach (var zi in zileLucrate.OrderBy(z => z))
        {
            var ziSaptamana = zi.DayOfWeek;

            var orarPeZi = orar.Where(o =>
                ConvertZiTextToDayOfWeek(o.Ziua) == ziSaptamana &&
                !string.IsNullOrWhiteSpace(o.Formatia) &&
                new[] { o.OreCurs, o.OreSem, o.OreLab, o.OreProi }.Count(x => x > 0) == 1 &&
                (string.IsNullOrWhiteSpace(o.ImparPar) || VerificaParitate(paritati, zi, o.ImparPar))
            ).ToList();

            var speciale = orarPeZi
                .Where(o => !string.IsNullOrWhiteSpace(o.SaptamanaInceput) && o.TotalOre > 0)
                .GroupBy(o => new { o.NrPost, o.Formatia, o.Tip, o.SaptamanaInceput, o.ImparPar });

            foreach (var grup in speciale)
            {
                if (!int.TryParse(grup.Key.SaptamanaInceput.Replace("S", ""), out int saptInceput))
                    continue;

                var saptCurenta = AflaNrSaptamana(zi, paritati);
                if (!saptCurenta.HasValue || saptCurenta.Value < saptInceput)
                    continue;

                var key = (grup.Key.NrPost, grup.Key.Formatia, grup.Key.Tip, grup.Key.SaptamanaInceput);
                oreConsumatGlobal.TryGetValue(key, out int dejaConsumate);

                int oreOrar = grup.Sum(o => o.OreCurs + o.OreSem + o.OreLab + o.OreProi);
                if (oreOrar == 0) continue;

                int oreRamase = CalculeazaOreRamaseInSaptamana(saptCurenta.Value, saptInceput, grup.Key.ImparPar, grup.First().TotalOre, oreOrar);
                int maxPermis = grup.First().TotalOre - dejaConsumate;

                if (maxPermis <= 0)
                {
                    foreach (var o in grup)
                        if (o.OreCurs + o.OreSem + o.OreLab + o.OreProi > 0)
                            yield return selector(o, zi);
                    continue;
                }

                if (oreRamase <= 0) continue;

                int deAdaugat = Math.Min(oreRamase, maxPermis);
                int alocat = 0;

                foreach (var o in grup)
                {
                    int oreFizice = o.OreCurs + o.OreSem + o.OreLab + o.OreProi;
                    if (oreFizice == 0 || alocat >= deAdaugat) continue;

                    int alocHere = Math.Min(oreFizice, deAdaugat - alocat);
                    orePeZi.TryGetValue(zi.Date, out int totalInZi);

                    if (totalInZi + alocHere > 12) break;

                    if (o.OreCurs > 0) o.OreCurs = alocHere;
                    if (o.OreSem > 0) o.OreSem = alocHere;
                    if (o.OreLab > 0) o.OreLab = alocHere;
                    if (o.OreProi > 0) o.OreProi = alocHere;

                    orePeZi[zi.Date] = totalInZi + alocHere;
                    yield return selector(o, zi);
                    alocat += alocHere;
                }

                oreConsumatGlobal[key] = dejaConsumate + alocat;
            }

            var normale = orarPeZi
                .Where(o => string.IsNullOrWhiteSpace(o.SaptamanaInceput) || o.TotalOre <= 0);

            foreach (var o in normale)
            {
                int oreFizice = o.OreCurs + o.OreSem + o.OreLab + o.OreProi;
                if (oreFizice == 0) continue;

                orePeZi.TryGetValue(zi.Date, out int totalInZi);
                if (totalInZi + oreFizice > 12) continue;

                orePeZi[zi.Date] = totalInZi + oreFizice;
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

    private void AdaugaTitluSiParagraf(Document doc, InfoUserDTO user, List<DateTime> zileLucrate, DateTime firstDay, DateTime lastDay)
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
        var dataStart = firstDay.ToString("dd.MM.yyyy");
        var dataEnd = lastDay.ToString("dd.MM.yyyy");

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
        var paragrafLung = new Paragraph("Subsemnatul/Subsemnata, cunoscand prevederile art. 326 din Codul Penal cu privire la falsul în declaratii, declar pe propria raspundere ca in luna pentru care fac prezenta declaratie de plata cu ora, in afara functiei/normei de baza, am desfasurat activitati in regim de plata cu ora si/sau cu contract individual de munca cu timp partial si/sau activitati in cadrul proiectelor respectand legislatia muncii cu privire la numarul maxim de ore ce pot fi efectuate in cadrul activitatilor în afara functiei de baza/normei de baza, fara a depasi o medie de 4 ore/zi, respectiv o medie de 84 ore/luna in anul universitar, cu respectarea duratei zilnice si saptamanale maxime legale a timpului de munca si a perioadelor minime de repaus zilnic si saptamanal. De asemenea, declar ca sunt de acord sa pun la dispozitia institutiilor abilitate, la solicitarea acestora, documentele doveditoare in scopul verificarii si confirmarii informatiilor furnizare prin aceasta declaratie.", font)
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
        .GroupBy(x => x.NrPost)
        .ToList();

        foreach (var grup in grupuriFinale)
        {
            var subgrupuri = grup
                .GroupBy(x => new
                {
                    x.Zi,
                    Coef = x.Tip switch
                    {
                        "LR" => x.OreCurs > 0 ? 2.0 : 1.0,
                        "LE" => x.OreCurs > 0 ? 2.5 : 1.25,
                        "MR" => x.OreCurs > 0 ? 2.5 : 1.5,
                        "ME" => x.OreCurs > 0 ? 3.125 : 1.875,
                        _ => 1.0
                    },
                    x.Tip
                })
                .ToList();

            int rowspan = subgrupuri.Count;
            bool isFirst = true;

            var denPostConcat = string.Join(", ", grup.Select(g => g.DenPost).Distinct());

            foreach (var subgrup in subgrupuri)
            {
                int totalC1 = subgrup.Sum(o => o.OreCurs);
                int totalS1 = subgrup.Sum(o => o.OreSem);
                int totalLA1 = subgrup.Sum(o => o.OreLab);
                int totalP1 = subgrup.Sum(o => o.OreProi);

                totalC += totalC1;
                totalS += totalS1;
                totalLA += totalLA1;
                totalP += totalP1;

                var zi = subgrup.Key.Zi;
                var coef = subgrup.Key.Coef;
                var tip = subgrup.Key.Tip;

                var formatiaConcat = string.Join(", ", subgrup.Select(o => o.Formatia).Distinct());

                double oreTotal = (totalC1 + totalS1 + totalLA1 + totalP1) * coef;
                totalOreCoef += oreTotal;

                if (isFirst)
                {
                    string textPozitie = $"{grup.Key}\n{denPostConcat}";
                    table.AddCell(new PdfPCell(new Phrase(textPozitie, fontNormal))
                    {
                        Rowspan = rowspan,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    isFirst = false;
                }

                table.AddCell(new PdfPCell(new Phrase(zi.ToString("dd.MM.yyyy"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(totalC1 > 0 ? totalC1.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(totalS1 > 0 ? totalS1.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(totalLA1 > 0 ? totalLA1.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(totalP1 > 0 ? totalP1.ToString() : "", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(tip, fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(coef.ToString("0.###"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(oreTotal.ToString("0.###"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(formatiaConcat, fontNormal)) { HorizontalAlignment = Element.ALIGN_LEFT });
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
        table.AddCell(new PdfPCell(new Phrase(totalOreCoef.ToString("0.###"), fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
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

    private static bool VerificaParitate(List<ParitateSaptamanaDTO> paritati, DateTime zi, string imparPar)
    {
        var ziData = zi.Date;

        foreach (var p in paritati)
        {
            if (!DateTime.TryParseExact(p.Data, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataStart))
                continue;

            var start = dataStart.Date;
            var end = start.AddDays(7).Date;

            if (ziData >= start && ziData < end &&
                p.Paritate.Equals(imparPar, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private int CalculeazaOreRamaseInSaptamana(
        int saptCurenta,
        int saptInceput,
        string paritate,
        int totalOre,
        int oreOrar,
        int maxSaptamani = 10)
    {
        int diferenta = saptCurenta - saptInceput;
        int saptamaniTrecute;

        if (diferenta < 0)
        {
            return oreOrar;
        }

        if (string.IsNullOrWhiteSpace(paritate))
        {
            saptamaniTrecute = diferenta;
        }
        else
        {
            saptamaniTrecute = diferenta / 2;

            if (diferenta % 2 == 1)
            {
                if ((paritate.Equals("Par", StringComparison.OrdinalIgnoreCase) && saptInceput % 2 == 0) ||
                    (paritate.Equals("Impar", StringComparison.OrdinalIgnoreCase) && saptInceput % 2 == 1))
                {
                    saptamaniTrecute++;
                }
            }
        }

        if (saptamaniTrecute >= maxSaptamani)
        {
            return -1;
        }

        int oreRamase = totalOre - saptamaniTrecute * oreOrar;
        int oreDeIntors = Math.Min(oreRamase, oreOrar);

        return oreDeIntors > 0 ? oreDeIntors : -1;
    }

    private static int? AflaNrSaptamana(DateTime zi, List<ParitateSaptamanaDTO> paritati)
    {
        for (int i = 0; i < paritati.Count; i++)
        {
            if (DateTime.TryParseExact(paritati[i].Data, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataStart))
            {
                if (zi >= dataStart && zi < dataStart.AddDays(7))
                {
                    if (int.TryParse(paritati[i].Sapt.Replace("S", ""), out var sapt))
                        return sapt;
                }
            }
        }
        return null;
    }

    //EXCEL

    public async Task<byte[]> GenereazaDeclaratieEXCELAsync(string userId, List<DateTime> zileLucrate, DateTime firstDay, DateTime lastDay)
    {
        _logger.LogInformation("Starting declaration generation for userId={UserId}, days={Zile}", userId, zileLucrate);

        var user = await _infoUserRepo.GetActiveInfoAsync(userId);
        if (user == null)
            throw new Exception("User not found.");


        var orar = await _orarUserRepo.GetAllAsync(userId);
        if (orar == null || !orar.Any())
            throw new Exception("No schedule exists for this user.");

        var paritati = await _paritateRepo.GetParitateSaptAsync(userId) ?? new List<ParitateSaptamanaDTO>();
        if (paritati == null || !paritati.Any())
            throw new Exception("No parity data exists for this user.");

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

        var excel = GenereazaDeclaratieExcel(userDto, orar, zileLucrate, paritati, firstDay, lastDay);

        _logger.LogInformation("Declaration EXCEL generated successfully for userId={UserId}", userId);
        return excel;
    }

    public byte[] GenereazaDeclaratieExcel(
    InfoUserDTO user,
    List<OrarUserDTO> ore,
    List<DateTime> zileLucrate,
    List<ParitateSaptamanaDTO> paritati,
    DateTime firstDay,
    DateTime lastDay)
    {
        using var workbook = new XLWorkbook();
        var sheetName = $"PO {lastDay:dd.MM.yyyy}";
        var ws = workbook.Worksheets.Add(sheetName);


        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.Margins.Left = 1.75;
        ws.PageSetup.Margins.Right = 0.5;
        ws.PageSetup.Margins.Top = 0.5;
        ws.PageSetup.Margins.Bottom = 0.5;
        ws.PageSetup.Margins.Header = 0;
        ws.PageSetup.Margins.Footer = 0;

        ws.Column("A").Width = 9;
        ws.Column("B").Width = 11;
        ws.Columns("C", "F").Width = 5;
        ws.Column("G").Width = 3;
        ws.Column("H").Width = 7;
        ws.Column("I").Width = 9;
        ws.Column("J").Width = 27;

        int row = 1;

        row = AdaugaHeaderExcel(ws, user, row);

        row = AdaugaTitluSiParagrafExcel(ws, user, zileLucrate, firstDay, lastDay, row);

        row = AdaugaTabelOreExcel(ws, ore, zileLucrate, paritati, row);

        row = AdaugaTabelCoeficientiExcel(ws, row);

        row = AdaugaParagrafLegalExcel(ws, row);

        AdaugaSemnaturiExcel(ws, user, row);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private int AdaugaHeaderExcel(IXLWorksheet ws, InfoUserDTO user, int row)
    {
        ws.Range(row, 1, row, 7).Merge();
        ws.Cell(row, 1).Value = user.Universitate?.ToUpper();
        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        ws.Range(row, 9, row, 10).Merge();
        ws.Cell(row, 9).Value = "Aprobat,";
        ws.Cell(row, 9).Style.Font.Bold = true;
        ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        row++;

        ws.Range(row, 1, row, 7).Merge();
        ws.Cell(row, 1).Value = user.Facultate;

        ws.Range(row, 9, row, 10).Merge();
        ws.Cell(row, 9).Value = "Decan";
        ws.Cell(row, 9).Style.Font.Bold = true;
        ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        row++;

        ws.Range(row, 1, row, 8).Merge();
        ws.Cell(row, 1).Value = $"{user.Departament}";

        row++; ;

        ws.Range(row, 9, row, 10).Merge();
        ws.Cell(row, 9).Value = user.Decan;
        ws.Cell(row, 9).Style.Font.Bold = true;
        ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        return row + 1; 
    }

    private int AdaugaTitluSiParagrafExcel(IXLWorksheet ws, InfoUserDTO user, List<DateTime> zileLucrate, DateTime firstDay, DateTime lastDay, int row)
    {
        ws.Cell(row, 1).Value = "DECLARAȚIE";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 12;
        ws.Range(row, 1, row, 10).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;

        string interval = $"{firstDay:dd.MM.yyyy} - {lastDay:dd.MM.yyyy}";

        string text = $"\tSubsemnatul(a),{user.Declarant}, am suplinit în intervalul {interval} în {user.Departament} activități didactice după cum urmează:";

        var range = ws.Range(row, 1, row, 10).Merge();
        var cell = ws.Cell(row, 1);
        cell.Value = text;

        range.Style.Alignment.WrapText = true;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
        ws.Row(row).Height = 30;

        var richText = cell.GetRichText();


        int indexNume = text.IndexOf(user.Declarant, StringComparison.Ordinal);
        if (indexNume >= 0)
            richText.Substring(indexNume, user.Declarant.Length).SetBold().SetUnderline();

        int indexInterval = text.IndexOf(interval);
        if (indexInterval >= 0)
            richText.Substring(indexInterval, interval.Length).SetBold().SetUnderline();

        return row + 2;
    }

    private int AdaugaTabelOreExcel(IXLWorksheet ws, List<OrarUserDTO> ore, List<DateTime> zileLucrate, List<ParitateSaptamanaDTO> paritati, int row)
    {
        ws.Range(row, 1, row + 1, 1).Merge().Value = "Poz.\ndin stat";
        ws.Range(row, 2, row + 1, 2).Merge().Value = "Data";
        ws.Range(row, 3, row, 6).Merge().Value = "Număr ore fizice";
        ws.Cell(row + 1, 3).Value = "C";
        ws.Cell(row + 1, 4).Value = "S";
        ws.Cell(row + 1, 5).Value = "L/A";
        ws.Cell(row + 1, 6).Value = "P";
        ws.Range(row, 7, row + 1, 7).Merge().Value = "Tip";
        ws.Range(row, 8, row + 1, 8).Merge().Value = "Coef.**";
        ws.Range(row, 9, row + 1, 9).Merge().Value = "Nr.\nore";
        ws.Range(row, 10, row + 1, 10).Merge().Value = "Anul , grupa, semigrupa";

        var headerRange = ws.Range(row, 1, row + 1, 10);
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Alignment.WrapText = true;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        row += 2;
        int startRowTabel = row;

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
        }).Distinct().OrderBy(x => x.NrPost).ThenBy(x => x.Zi).ToList();

        var grupuriFinale = orePerZiPost.GroupBy(x => x.NrPost);

        foreach (var grup in grupuriFinale)
        {
            var denPostConcat = string.Join(", ", grup.Select(g => g.DenPost).Distinct());

            var subgrupuri = grup.GroupBy(x => new
            {
                x.Zi,
                Coef = x.Tip switch
                {
                    "LR" => x.OreCurs > 0 ? 2.0 : 1.0,
                    "LE" => x.OreCurs > 0 ? 2.5 : 1.25,
                    "MR" => x.OreCurs > 0 ? 2.5 : 1.5,
                    "ME" => x.OreCurs > 0 ? 3.125 : 1.875,
                    _ => 1.0
                },
                x.Tip
            });

            int startRowGrup = row;
            foreach (var sub in subgrupuri)
            {
                int c = sub.Sum(o => o.OreCurs);
                int s = sub.Sum(o => o.OreSem);
                int la = sub.Sum(o => o.OreLab);
                int p = sub.Sum(o => o.OreProi);

                double oreTotal = (c + s + la + p) * sub.Key.Coef;
                totalC += c; totalS += s; totalLA += la; totalP += p;
                totalOreCoef += oreTotal;

                int col = 1;
                ws.Cell(row, col++).Value = "";
                ws.Cell(row, col++).Value = sub.Key.Zi.ToString("dd.MM.yyyy");
                ws.Cell(row, col++).Value = c > 0 ? c : "";
                ws.Cell(row, col++).Value = s > 0 ? s : "";
                ws.Cell(row, col++).Value = la > 0 ? la : "";
                ws.Cell(row, col++).Value = p > 0 ? p : "";
                ws.Cell(row, col++).Value = sub.Key.Tip;
                ws.Cell(row, col++).Value = sub.Key.Coef;
                ws.Cell(row, col++).Value = Math.Round(oreTotal, 3);
                ws.Cell(row, col++).Value = string.Join(", ", sub.Select(o => o.Formatia).Distinct());

                var rowRange = ws.Range(row, 1, row, 10);
                rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;


                row++;
            }
            var cellPoz = ws.Range(startRowGrup, 1, row - 1, 1).Merge();
            cellPoz.Value = $"{grup.Key}\n{denPostConcat}";
            cellPoz.Style.Alignment.WrapText = true;
            cellPoz.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cellPoz.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cellPoz.Style.Font.Bold = false;
            cellPoz.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Range(startRowGrup, 1, row - 1, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        }

        ws.Cell(row, 2).Value = "TOTAL";
        ws.Cell(row, 3).Value = totalC;
        ws.Cell(row, 4).Value = totalS;
        ws.Cell(row, 5).Value = totalLA;
        ws.Cell(row, 6).Value = totalP;
        ws.Cell(row, 9).Value = Math.Round(totalOreCoef, 3);

        var totalRange = ws.Range(row, 1, row, 10);
        totalRange.Style.Font.Bold = true;
        totalRange.Style.Font.FontSize = 12;
        totalRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        totalRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        totalRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        totalRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Range(startRowTabel - 2, 1, row, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

        row++;

        int totalFizice = totalC + totalS + totalLA + totalP;
        var subtotalCell = ws.Range(row, 3, row, 6).Merge();
        subtotalCell.Value = totalFizice;
        subtotalCell.Style.Font.Bold = true;
        subtotalCell.Style.Font.FontSize = 12;
        subtotalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        subtotalCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        subtotalCell.Style.Border.TopBorder = XLBorderStyleValues.Thick;
        subtotalCell.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
        subtotalCell.Style.Border.LeftBorder = XLBorderStyleValues.Thick;
        subtotalCell.Style.Border.RightBorder = XLBorderStyleValues.Thick;
        subtotalCell.Style.Fill.BackgroundColor = XLColor.LightGray;

        return row + 2;
    }

    private int AdaugaTabelCoeficientiExcel(IXLWorksheet ws, int row)
    {
        ws.Cell(row, 1).Value = "**Coeficienții sunt conform tabelului:";
        ws.Range(row, 1, row, 6).Merge();
        ws.Range(row, 1, row, 6).Style.Alignment.WrapText = true;
        ws.Range(row, 1, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        ws.Range(row, 1, row, 6).Style.Font.Bold = true;
        ws.Row(row).Height = 20;
        row++;

        ws.Range(row, 1, row, 2).Merge().Value = "Coeficienți";
        ws.Range(row, 3, row, 4).Merge().Value = "Curs";
        ws.Range(row, 5, row, 6).Merge().Value = "S/L/P/A";

        var headerRange = ws.Range(row, 1, row, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(row).Height = 20;
        row++;

        (string, double, double)[] data = {
            ("LR - Licența română", 2.0, 1.0),
            ("LE - Licența engleză", 2.5, 1.25),
            ("MR - Master română", 2.5, 1.5),
            ("ME - Master engleză", 3.125, 1.875)
        };

        foreach (var (tip, coefCurs, coefAlt) in data)
        {
            ws.Range(row, 1, row, 2).Merge().Value = tip;
            ws.Range(row, 3, row, 4).Merge().Value = coefCurs;
            ws.Range(row, 5, row, 6).Merge().Value = coefAlt;

            ws.Range(row, 1, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(row, 1, row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(row).Height = 18;

            row++;
        }

        int lastRow = row - 1;
        var tableRange = ws.Range(row - data.GetLength(0) - 1, 1, lastRow, 6);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        return row + 2;
    }

    private int AdaugaParagrafLegalExcel(IXLWorksheet ws, int row)
    {
        string textLung = "Subsemnatul/Subsemnata, cunoscând prevederile art. 326 din Codul Penal cu privire la falsul în declarații, declar pe propria răspundere că în luna pentru care fac prezenta declarație de plată cu ora, în afara funcției/normei de bază, am desfășurat activități în regim de plată cu ora și/sau cu contract individual de muncă cu timp parțial și/sau activități în cadrul proiectelor respectând legislația muncii cu privire la numărul maxim de ore ce pot fi efectuate în cadrul activităților în afara funcției de bază/normei de bază, fără a depăși o medie de 4 ore/zi, respectiv o medie de 84 ore/lună în anul universitar, cu respectarea duratei zilnice și săptămânale maxime legale a timpului de muncă și a perioadelor minime de repaus zilnic și săptămânal. De asemenea, declar că sunt de acord să pun la dispoziția instituțiilor abilitate, la solicitarea acestora, documentele doveditoare în scopul verificării și confirmării informațiilor furnizate prin această declarație.";

        var rangeLung = ws.Range(row, 1, row, 10).Merge();
        rangeLung.Value = textLung;
        rangeLung.Style.Alignment.WrapText = true;
        rangeLung.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Justify;
        rangeLung.Style.Font.FontSize = 8;
        ws.Row(row).Height = 75;

        row += 2;

        string textScurt = "Se certifică de noi că orele declarate mai sus au fost efectuate de declarant întocmai.";

        var rangeScurt = ws.Range(row, 1, row, 10).Merge();
        rangeScurt.Value = textScurt;
        rangeScurt.Style.Alignment.WrapText = true;
        rangeScurt.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        rangeScurt.Style.Font.FontSize = 10;
        ws.Row(row).Height = 15;

        return row + 2;
    }

    private void AdaugaSemnaturiExcel(IXLWorksheet ws, InfoUserDTO user, int row)
    {
        ws.Range(row, 1, row, 3).Merge();
        ws.Cell(row, 1).Value = "DIRECTOR DEPARTAMENT";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        ws.Range(row, 7, row, 8).Merge();
        ws.Cell(row, 7).Value = "DECLARANT";
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        row += 2;

        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Value = user.DirectorDepartament;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        ws.Range(row, 7, row, 10).Merge();
        ws.Cell(row, 7).Value = user.Declarant;
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    }

}
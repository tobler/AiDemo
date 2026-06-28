using AiDemo.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OoxmlDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using OoxmlColor = DocumentFormat.OpenXml.Wordprocessing.Color;

namespace AiDemo.Services;

public class DocumentService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _outputPath;

    public DocumentService(IWebHostEnvironment env)
    {
        _env = env;
        _outputPath = Path.Combine(env.ContentRootPath, "wwwroot", "generated");
        Directory.CreateDirectory(_outputPath);
    }

    public Task<GeneratedDocument> CreatePdfAsync(string titel, string inhalt, string firma)
    {
        var fileName = $"{SanitizeFileName(titel)}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(_outputPath, fileName);

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(2, Unit.Centimetre);
                page.MarginBottom(2, Unit.Centimetre);
                page.MarginLeft(2.5f, Unit.Centimetre);
                page.MarginRight(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    if (!string.IsNullOrEmpty(firma))
                    {
                        col.Item().Text(firma).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                    }

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text(titel).FontSize(14).Bold();
                        row.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy")).FontSize(10);
                    });
                    col.Item().PaddingBottom(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    var lines = inhalt.Split('\n');
                    foreach (var line in lines)
                    {
                        var trimmed = line.TrimStart();
                        if (trimmed.StartsWith("## "))
                        {
                            col.Item().PaddingTop(10).PaddingBottom(3)
                                .Text(trimmed[3..]).FontSize(13).Bold();
                        }
                        else if (trimmed.StartsWith("# "))
                        {
                            col.Item().PaddingTop(12).PaddingBottom(5)
                                .Text(trimmed[2..]).FontSize(15).Bold();
                        }
                        else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                        {
                            col.Item().PaddingLeft(15).Row(row =>
                            {
                                row.ConstantItem(15).Text("•").FontSize(11);
                                row.RelativeItem().Text(trimmed[2..]).FontSize(11);
                            });
                        }
                        else if (trimmed.StartsWith("**") && trimmed.EndsWith("**"))
                        {
                            col.Item().Text(trimmed.Trim('*')).Bold();
                        }
                        else if (string.IsNullOrWhiteSpace(trimmed))
                        {
                            col.Item().PaddingBottom(5);
                        }
                        else
                        {
                            col.Item().Text(trimmed).FontSize(11);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Seite ");
                    text.CurrentPageNumber();
                    text.Span(" von ");
                    text.TotalPages();
                });
            });
        });

        document.GeneratePdf(filePath);
        var data = File.ReadAllBytes(filePath);

        return Task.FromResult(new GeneratedDocument
        {
            FileName = fileName,
            FileType = "pdf",
            Data = data,
            DownloadUrl = $"/generated/{fileName}"
        });
    }

    public Task<GeneratedDocument> CreateDocxAsync(string titel, string inhalt, string firma)
    {
        var fileName = $"{SanitizeFileName(titel)}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
        var filePath = Path.Combine(_outputPath, fileName);

        using (var wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new OoxmlDocument();
            var body = mainPart.Document.AppendChild(new Body());

            // Firma Header
            if (!string.IsNullOrEmpty(firma))
            {
                var firmaPara = new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Left }),
                    new Run(
                        new RunProperties(
                            new Bold(),
                            new FontSize { Val = "32" },
                            new OoxmlColor { Val = "1F4E79" }
                        ),
                        new Text(firma)
                    )
                );
                body.AppendChild(firmaPara);
                body.AppendChild(new Paragraph()); // Leerzeile
            }

            // Titel
            var titlePara = new Paragraph(
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = "28" }),
                    new Text(titel)
                )
            );
            body.AppendChild(titlePara);

            // Datum
            var datePara = new Paragraph(
                new Run(
                    new RunProperties(new FontSize { Val = "20" }, new OoxmlColor { Val = "666666" }),
                    new Text($"Datum: {DateTime.Now:dd.MM.yyyy}")
                )
            );
            body.AppendChild(datePara);
            body.AppendChild(new Paragraph());

            // Inhalt
            var lines = inhalt.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                var para = new Paragraph();

                if (trimmed.StartsWith("## "))
                {
                    para.AppendChild(new Run(
                        new RunProperties(new Bold(), new FontSize { Val = "26" }),
                        new Text(trimmed[3..])
                    ));
                }
                else if (trimmed.StartsWith("# "))
                {
                    para.AppendChild(new Run(
                        new RunProperties(new Bold(), new FontSize { Val = "30" }),
                        new Text(trimmed[2..])
                    ));
                }
                else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                {
                    para.AppendChild(new ParagraphProperties(
                        new Indentation { Left = "720" }
                    ));
                    para.AppendChild(new Run(new Text($"• {trimmed[2..]}")));
                }
                else if (string.IsNullOrWhiteSpace(trimmed))
                {
                    // Leerzeile
                }
                else
                {
                    if (trimmed.StartsWith("**") && trimmed.EndsWith("**"))
                    {
                        para.AppendChild(new Run(
                            new RunProperties(new Bold()),
                            new Text(trimmed.Trim('*'))
                        ));
                    }
                    else
                    {
                        para.AppendChild(new Run(new Text(trimmed) { Space = SpaceProcessingModeValues.Preserve }));
                    }
                }

                body.AppendChild(para);
            }
        }

        var data = File.ReadAllBytes(filePath);

        return Task.FromResult(new GeneratedDocument
        {
            FileName = fileName,
            FileType = "docx",
            Data = data,
            DownloadUrl = $"/generated/{fileName}"
        });
    }

    public Task<GeneratedDocument> CreateExcelAsync(string titel, List<string> spalten, List<List<string>> zeilen)
    {
        var fileName = $"{SanitizeFileName(titel)}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(_outputPath, fileName);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add(titel.Length > 31 ? titel[..31] : titel);

            // Header
            for (int i = 0; i < spalten.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = spalten[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Daten
            for (int row = 0; row < zeilen.Count; row++)
            {
                for (int col = 0; col < zeilen[row].Count; col++)
                {
                    var cell = worksheet.Cell(row + 2, col + 1);
                    if (double.TryParse(zeilen[row][col], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var numVal))
                    {
                        cell.Value = numVal;
                        cell.Style.NumberFormat.Format = "#,##0.00";
                    }
                    else
                    {
                        cell.Value = zeilen[row][col];
                    }

                    if (row % 2 == 0)
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
                }
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        var data = File.ReadAllBytes(filePath);

        return Task.FromResult(new GeneratedDocument
        {
            FileName = fileName,
            FileType = "xlsx",
            Data = data,
            DownloadUrl = $"/generated/{fileName}"
        });
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Replace(" ", "_");
    }
}

using Proj1.Interfaces;
using Proj1.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Proj1.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateResidentListPdf(List<ResidentViewModel> residents)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("Residents Report")
                    .SemiBold().FontSize(18).AlignCenter();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Full name
                        columns.RelativeColumn(4); // Address
                        columns.RelativeColumn(2); // Birth date
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Full Name");
                        header.Cell().Element(CellStyle).Text("Address");
                        header.Cell().Element(CellStyle).Text("Birth Date");

                        static IContainer CellStyle(IContainer container) =>
                            container.DefaultTextStyle(x => x.SemiBold())
                                     .PaddingVertical(5)
                                     .BorderBottom(1)
                                     .BorderColor(Colors.Grey.Lighten2);
                    });

                    foreach (var r in residents)
                    {
                        table.Cell().Element(RowStyle).Text(r.FullName);
                        table.Cell().Element(RowStyle).Text(r.Address);
                        table.Cell().Element(RowStyle).Text(r.BirthDate.ToString("yyyy-MM-dd"));
                    }

                    static IContainer RowStyle(IContainer container) =>
                        container.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                });

                page.Footer()
                    .AlignRight()
                    .Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });

        return document.GeneratePdf();
    }
}
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Proj1.Interfaces;
using Proj1.Models.Configuration;
using Proj1.Models.ViewModels;

namespace Proj1.Services;

/// <summary>
/// PDF generation service using QuestPDF.
/// Best Practice: Centralized PDF generation with consistent styling.
/// </summary>
public class PdfService : IPdfService
{
    private readonly BarangayInfo _barangayInfo;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PdfService> _logger;

    public PdfService(
        IOptions<BarangayInfo> barangayInfo,
        IWebHostEnvironment environment,
        ILogger<PdfService> logger)
    {
        _barangayInfo = barangayInfo.Value;
        _environment = environment;
        _logger = logger;
        
        // Best Practice: Set license once in constructor
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates a formatted PDF list of residents.
    /// Best Practice: Consistent document structure and professional styling.
    /// </summary>
    public byte[] GenerateResidentListPdf(List<ResidentViewModel> residents)
    {
        try
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(0.75f, Unit.Inch);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeResidentList(content, residents));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating resident list PDF");
            throw;
        }
    }

    /// <summary>
    /// Composes the document header with barangay information.
    /// </summary>
    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Logo (if exists)
            var logoPath = Path.Combine(_environment.WebRootPath, _barangayInfo.LogoPath);
            if (File.Exists(logoPath))
            {
                column.Item()
                    .AlignCenter()
                    .Width(60)
                    .Image(logoPath);
            }

            // Header text
            column.Item()
                .AlignCenter()
                .DefaultTextStyle(x => x.FontSize(9).SemiBold())
                .Text("Republic of the Philippines");
            
            column.Item()
                .AlignCenter()
                .DefaultTextStyle(x => x.FontSize(9))
                .Text($"Province of {_barangayInfo.Province}");
            
            column.Item()
                .AlignCenter()
                .DefaultTextStyle(x => x.FontSize(9))
                .Text($"Municipality of {_barangayInfo.Municipality}");
            
            column.Item()
                .AlignCenter()
                .DefaultTextStyle(x => x.FontSize(10).Bold())
                .Text(_barangayInfo.Name);

            // Title
            column.Item()
                .PaddingTop(15)
                .PaddingBottom(10)
                .AlignCenter()
                .DefaultTextStyle(x => x.FontSize(12).Bold())
                .Text("RESIDENT DIRECTORY");

            // Divider line
            column.Item()
                .PaddingBottom(10)
                .LineHorizontal(1)
                .LineColor(Colors.Grey.Darken2);
        });
    }

    /// <summary>
    /// Composes the main content with resident table.
    /// Best Practice: Responsive table with clear headers and data.
    /// </summary>
    private void ComposeResidentList(IContainer container, List<ResidentViewModel> residents)
    {
        container.Column(column =>
        {
            // Summary information
            column.Item()
                .PaddingBottom(10)
                .DefaultTextStyle(x => x.FontSize(9))
                .Text(text =>
                {
                    text.Span("Total Residents: ");
                    text.Span(residents.Count.ToString()).Bold();
                    text.Span($" | Generated: {DateTime.Now:MMMM dd, yyyy}");
                });

            // Table
            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);      // No.
                    columns.RelativeColumn(3);        // Full Name
                    columns.RelativeColumn(3);        // Address
                    columns.RelativeColumn(1.5f);    // Birth Date
                    columns.RelativeColumn(1);        // Age
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("No.");
                    header.Cell().Element(HeaderCellStyle).Text("Full Name");
                    header.Cell().Element(HeaderCellStyle).Text("Address");
                    header.Cell().Element(HeaderCellStyle).Text("Birth Date");
                    header.Cell().Element(HeaderCellStyle).Text("Age");
                });

                // Data rows
                var rowNumber = 1;
                foreach (var resident in residents)
                {
                    var isEven = rowNumber % 2 == 0;
                    
                    table.Cell().Element(cell => DataCellStyle(cell, isEven))
                        .AlignCenter()
                        .Text(rowNumber.ToString());
                    
                    table.Cell().Element(cell => DataCellStyle(cell, isEven))
                        .Text(resident.FullName);
                    
                    table.Cell().Element(cell => DataCellStyle(cell, isEven))
                        .Text(resident.Address);
                    
                    table.Cell().Element(cell => DataCellStyle(cell, isEven))
                        .AlignCenter()
                        .Text(resident.BirthDate.ToString("MMM dd, yyyy"));
                    
                    table.Cell().Element(cell => DataCellStyle(cell, isEven))
                        .AlignCenter()
                        .Text(CalculateAge(resident.BirthDate).ToString());

                    rowNumber++;
                }
            });

            // No residents message
            if (residents.Count == 0)
            {
                column.Item()
                    .PaddingTop(20)
                    .AlignCenter()
                    .DefaultTextStyle(x => x.Italic().FontColor(Colors.Grey.Darken1))
                    .Text("No residents found.");
            }
        });
    }

    /// <summary>
    /// Composes the document footer.
    /// </summary>
    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter()
            .DefaultTextStyle(x => x.FontSize(7).Italic().FontColor(Colors.Grey.Darken1))
            .Text(text =>
            {
                text.Span("Generated on ");
                text.Span(DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"));
                text.Span(" | This is a computer-generated document");
            });
    }

    // ========================================
    // STYLING HELPERS
    // ========================================

    /// <summary>
    /// Applies consistent styling to table header cells.
    /// </summary>
    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Background(Colors.Grey.Lighten2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    /// <summary>
    /// Applies consistent styling to table data cells with zebra striping.
    /// </summary>
    private static IContainer DataCellStyle(IContainer container, bool isEven)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(isEven ? Colors.Grey.Lighten4 : Colors.White)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Calculates age from birth date.
    /// </summary>
    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }
}
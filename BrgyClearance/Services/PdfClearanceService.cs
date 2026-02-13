using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using Proj1.Data;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Common.Enums;
using Proj1.Models.Configuration;

namespace Proj1.Services;

public class PdfClearanceService : IPdfClearanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly BarangayInfo _barangayInfo;
    private readonly ClearanceSettings _clearanceSettings;
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly ILogger<PdfClearanceService> _logger;
    private readonly IWebHostEnvironment _environment;

    public PdfClearanceService(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        IOptions<BarangayInfo> barangayInfo,
        IOptions<ClearanceSettings> clearanceSettings,
        IOptions<FileStorageSettings> fileStorageSettings,
        ILogger<PdfClearanceService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _fileStorage = fileStorage;
        _barangayInfo = barangayInfo.Value;
        _clearanceSettings = clearanceSettings.Value;
        _fileStorageSettings = fileStorageSettings.Value;
        _logger = logger;
        _environment = environment;
        
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ServiceResult<string>> GenerateClearancePdfAsync(int requestId)
    {
        try
        {
            var request = await _context.ClearanceRequests
                .Include(r => r.Resident)
                .Include(r => r.ClearanceType)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogWarning("Clearance request not found: {RequestId}", requestId);
                return ServiceResult<string>.Failure("Clearance request not found");
            }

            if (request.Status != RequestStatus.Released)
            {
                _logger.LogWarning("Cannot generate PDF for unreleased clearance: {RequestId}", requestId);
                return ServiceResult<string>.Failure("Cannot generate PDF. Clearance must be in 'Released' status");
            }

            if (request.Resident == null)
            {
                _logger.LogError("Resident data missing for request: {RequestId}", requestId);
                return ServiceResult<string>.Failure("Resident information is missing");
            }

            if (request.ClearanceType == null)
            {
                _logger.LogError("ClearanceType data missing for request: {RequestId}", requestId);
                return ServiceResult<string>.Failure("Clearance type information is missing");
            }

            var documentDto = MapToDocumentDto(request);
            var pdfBytes = GeneratePdfDocument(documentDto);

            var sanitizedRefNum = SanitizeFileName(request.ReferenceNumber ?? "Unknown");
            var fileName = $"Clearance_{sanitizedRefNum}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            
            var saveResult = await _fileStorage.SaveFileAsync(
                pdfBytes, 
                fileName, 
                _fileStorageSettings.ClearancesFolder);

            if (!saveResult.Succeeded)
            {
                _logger.LogError("Failed to save PDF: {Error}", saveResult.ErrorMessage);
                return ServiceResult<string>.Failure(saveResult.ErrorMessage ?? "Failed to save PDF");
            }

            request.ClearanceDocumentPath = saveResult.Data;
            request.DocumentGeneratedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("PDF generated successfully for request: {RequestId}", requestId);
            
            return ServiceResult<string>.Success(saveResult.Data!, "PDF generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for request: {RequestId}", requestId);
            return ServiceResult<string>.Failure($"Error generating PDF: {ex.Message}");
        }
    }

    public async Task<ServiceResult<string>> RegenerateClearancePdfAsync(int requestId)
    {
        try
        {
            var request = await _context.ClearanceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return ServiceResult<string>.Failure("Clearance request not found");

            if (!string.IsNullOrEmpty(request.ClearanceDocumentPath))
            {
                try
                {
                    await _fileStorage.DeleteFileAsync(request.ClearanceDocumentPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old PDF, continuing with regeneration");
                }
            }

            return await GenerateClearancePdfAsync(requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating PDF for request: {RequestId}", requestId);
            return ServiceResult<string>.Failure($"Error regenerating PDF: {ex.Message}");
        }
    }

    private ClearanceDocumentDto MapToDocumentDto(Models.Entities.ClearanceRequest request)
    {
        var age = CalculateAge(request.Resident.BirthDate);
        var fullName = $"{request.Resident.FirstName} {request.Resident.LastName}";

        return new ClearanceDocumentDto
        {
            ReferenceNumber = request.ReferenceNumber ?? "N/A",
            ResidentFullName = fullName,
            Age = age,
            CivilStatus = "Single",
            Address = request.Resident.Address,
            Purpose = request.Purpose ?? "General purposes",
            ClearanceType = request.ClearanceType.Name,
            Fee = request.ClearanceType.Fee,
            OfficialReceiptNumber = request.OfficialReceiptNumber,
            IssueDate = request.ReleasedDate ?? DateTime.Now,
            ExpiryDate = request.ExpiryDate,
            IssuedBy = _barangayInfo.PunongBarangay
        };
    }

    private byte[] GeneratePdfDocument(ClearanceDocumentDto data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Times New Roman"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, data));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            var logoPath = Path.Combine(_environment.WebRootPath, _barangayInfo.LogoPath);
            if (File.Exists(logoPath))
            {
                column.Item()
                    .AlignCenter()
                    .Width(80)
                    .Image(logoPath);
            }

            column.Item().AlignCenter().Text("Republic of the Philippines").FontSize(10).SemiBold();
            
            column.Item().AlignCenter().Text($"Province of {_barangayInfo.Province}").FontSize(10);
            
            column.Item().AlignCenter().Text($"Municipality of {_barangayInfo.Municipality}").FontSize(10);
            
            column.Item().AlignCenter().Text(_barangayInfo.Name).FontSize(11).Bold();

            column.Item().PaddingTop(10).AlignCenter().Text("OFFICE OF THE PUNONG BARANGAY").FontSize(11).Bold();

            column.Item().PaddingTop(5).PaddingBottom(15).AlignCenter().Text("BARANGAY CLEARANCE").FontSize(14).Bold().Underline();
        });
    }

    private void ComposeContent(IContainer container, ClearanceDocumentDto data)
    {
        container.Column(column =>
        {
            column.Item().AlignRight().Text(data.IssueDate.ToString("MMMM dd, yyyy")).FontSize(11);

            column.Item().PaddingTop(20).Text("TO WHOM IT MAY CONCERN:").FontSize(11).Bold();

            column.Item()
                .PaddingTop(15)
                .DefaultTextStyle(x => x.FontSize(11).LineHeight(1.5f))
                .Text(text =>
                {
                    text.Justify();
                    text.Span("This is to certify that ");
                    text.Span(data.ResidentFullName.ToUpper()).Bold();
                    text.Span($", {data.Age} years old, {data.CivilStatus}, Filipino, is personally known to me as a resident of ");
                    text.Span(data.Address).Bold();
                    text.Span(" with the following findings:");
                });

            column.Item().PaddingTop(15).AlignCenter().Text("- no derogatory/criminal record filed against him/her as of this date.").FontSize(11).Bold();

            column.Item()
                .PaddingTop(15)
                .DefaultTextStyle(x => x.FontSize(11).LineHeight(1.5f))
                .Text(text =>
                {
                    text.Justify();
                    text.Span("This certification is issued upon the request of the above-mentioned name for ");
                    text.Span(data.Purpose.ToLower()).Bold();
                    text.Span(".");
                });

            column.Item()
                .PaddingTop(30)
                .Column(sig =>
                {
                    sig.Item().Text(data.ResidentFullName.ToUpper()).Bold().FontSize(11);
                    
                    sig.Item().Text("(Name and signature of applicant)").FontSize(9).Italic();
                });

            column.Item()
                .PaddingTop(40)
                .Row(row =>
                {
                    row.RelativeItem();
                    row.RelativeItem()
                        .Column(official =>
                        {
                            official.Item().Text(data.IssuedBy.ToUpper()).Bold().FontSize(11);
                            
                            official.Item().Text("Punong Barangay").FontSize(10);
                        });
                });

            column.Item()
                .PaddingTop(30)
                .Column(payment =>
                {
                    payment.Item().Text($"Cert. Fee:        ₱{data.Fee:F2}").FontSize(10);
                    
                    payment.Item().Text($"OR. No.:          {data.OfficialReceiptNumber ?? "N/A"}").FontSize(10);
                    
                    payment.Item().Text($"DST Paid:         ₱{_clearanceSettings.DocumentaryStampTax:F2}").FontSize(10);
                    
                    payment.Item().Text($"Date:             {data.IssueDate:MMMM dd, yyyy}").FontSize(10);
                });

            column.Item()
                .PaddingTop(20)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Column(notes =>
                        {
                            notes.Item().Text("Note: Not Valid Without Barangay Seal.").FontSize(9).Italic();
                            
                            notes.Item().Text($"Reference No: {data.ReferenceNumber}").FontSize(8).Italic();
                            
                            if (data.ExpiryDate.HasValue)
                            {
                                notes.Item().Text($"Valid Until: {data.ExpiryDate.Value:MMMM dd, yyyy}").FontSize(8).Italic();
                            }
                        });

                    row.ConstantItem(80)
                        .AlignRight()
                        .Image(GenerateQRCode(data.ReferenceNumber));
                });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter()
            .DefaultTextStyle(x => x.FontSize(7).Italic())
            .Text(text =>
            {
                text.Span("Generated on ");
                text.Span(DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"));
                text.Span(" | This is a computer-generated document");
            });
    }

    private byte[] GenerateQRCode(string referenceNumber)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode($"REF:{referenceNumber}", QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(5);
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) 
            age--;
        return age;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Models.Entities;

namespace Proj1.Seed;

public static class ClearanceTypeSeeder
{
    public static async Task SeedClearanceTypesAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        await context.Database.MigrateAsync();

        // Check if ClearanceTypes already exist
        if (await context.ClearanceTypes.AnyAsync())
        {
            return; // Already seeded
        }

        var clearanceTypes = new List<ClearanceType>
        {
            new ClearanceType
            {
                TypeName = "Barangay Clearance",
                Description = "Certificate of residency and good moral character",
                Fee = 50.00m,
                ProcessingDays = 3,
                IsActive = true
            },
            new ClearanceType
            {
                TypeName = "Business Permit Clearance",
                Description = "Required for business permit applications",
                Fee = 150.00m,
                ProcessingDays = 5,
                IsActive = true
            },
            new ClearanceType
            {
                TypeName = "Employment Clearance",
                Description = "Certificate for employment purposes",
                Fee = 75.00m,
                ProcessingDays = 3,
                IsActive = true
            },
            new ClearanceType
            {
                TypeName = "Police Clearance",
                Description = "Barangay endorsement for police clearance",
                Fee = 100.00m,
                ProcessingDays = 7,
                IsActive = true
            },
            new ClearanceType
            {
                TypeName = "Indigency Certificate",
                Description = "Certificate of low-income status",
                Fee = 0.00m,
                ProcessingDays = 2,
                IsActive = true
            }
        };

        await context.ClearanceTypes.AddRangeAsync(clearanceTypes);
        await context.SaveChangesAsync();
    }
}
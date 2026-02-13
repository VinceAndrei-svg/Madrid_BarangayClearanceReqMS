using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proj1.Models.Entities;

namespace Proj1.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<ClearanceRequest> ClearanceRequests { get; set; } = null!;
    public DbSet<ClearanceType> ClearanceTypes { get; set; } = null!;
    public DbSet<Resident> Residents { get; set; } = null!;
    
    // ✅ NEW: Audit logging table
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // RESIDENT CONFIGURATION
        // ========================================
        
        modelBuilder.Entity<Resident>()
            .HasIndex(r => r.UserId)
            .IsUnique();

        // ========================================
        // CLEARANCE REQUEST CONFIGURATION
        // ========================================
        
        modelBuilder.Entity<ClearanceRequest>()
            .HasOne(cr => cr.Resident)
            .WithMany()
            .HasForeignKey(cr => cr.ResidentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClearanceRequest>()
            .HasOne(cr => cr.ClearanceType)
            .WithMany()
            .HasForeignKey(cr => cr.ClearanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // AUDIT LOG CONFIGURATION
        // ========================================
        
        modelBuilder.Entity<AuditLog>(entity =>
        {
            // Primary key (configured by convention, but explicit is clearer)
            entity.HasKey(a => a.Id);
            
            // Performance indexes for common query patterns
            // Index on Timestamp - most queries filter by recent logs
            entity.HasIndex(a => a.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");
            
            // Index on UserId - queries filter by specific user
            entity.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");
            
            // Composite index on EntityType and EntityId - for entity history queries
            entity.HasIndex(a => new { a.EntityType, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");
            
            // Composite index on EntityType and Timestamp - for filtered queries
            entity.HasIndex(a => new { a.EntityType, a.Timestamp })
                .HasDatabaseName("IX_AuditLogs_EntityType_Timestamp");
            
            // No foreign key constraint to AspNetUsers
            // Audit logs must persist even if the user is deleted
            // This is intentional for audit trail integrity
        });
    }
}
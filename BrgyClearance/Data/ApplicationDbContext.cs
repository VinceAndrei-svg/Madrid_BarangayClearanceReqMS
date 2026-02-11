using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proj1.Models.Entities;

namespace Proj1.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<ClearanceRequest> ClearanceRequests { get; set; } = null!;
    public DbSet<ClearanceType> ClearanceTypes { get; set; } = null!;
    public DbSet<Resident> Residents { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resident>()
            .HasIndex(r => r.UserId)
            .IsUnique();

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
    }
}
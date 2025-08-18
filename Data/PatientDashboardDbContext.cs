using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PatientDashboard.Models;

namespace PatientDashboard.Data;

public class PatientDashboardDbContext : IdentityDbContext<IdentityUser>
{
    public PatientDashboardDbContext(DbContextOptions<PatientDashboardDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }

    public DbSet<VitalSign> VitalSigns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //calling base method to make sure Identity tables are created
        base.OnModelCreating(modelBuilder);

        // Soft-delete global filter
        modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<VitalSign>().HasQueryFilter(v => !v.IsDeleted);

        modelBuilder.Entity<VitalSign>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.VitalSigns)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VitalSign>().HasIndex(v => new { v.PatientId, v.MeasuredAt });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var time = DateTime.UtcNow;
        foreach(var e in ChangeTracker.Entries<BaseEntity>())
        {
            if(e.State == EntityState.Modified)
                e.Entity.UpdatedAt = time;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

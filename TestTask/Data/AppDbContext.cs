using Microsoft.EntityFrameworkCore;
using TestTask.Domain;

namespace TestTask.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CertificateRequest> Requests => Set<CertificateRequest>();
    public DbSet<RequestStatusHistory> History => Set<RequestStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var req = modelBuilder.Entity<CertificateRequest>();

        req.HasKey(c => c.Id);
        req.Property(c  => c.EmployerId).IsRequired();
        req.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        req.Property(x => x.Type).HasConversion<string>();
        req.Property(x => x.Status).HasConversion<string>();
        req.Property(x => x.RowVersion).IsConcurrencyToken();
        
        req.HasIndex(x => x.IdempotencyKey).IsUnique();

        req.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(h => h.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
        
        var hist = modelBuilder.Entity<RequestStatusHistory>();
        
        hist.HasKey(x => x.Id);
        hist.Property(x => x.FromStatus).HasConversion<string>();
        hist.Property(x => x.ToStatus).HasConversion<string>();
        hist.Property(x => x.ChangedBy).IsRequired();
        hist.HasIndex(x => x.RequestId);
    }
}
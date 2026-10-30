using Microsoft.EntityFrameworkCore;

namespace PhysioCenter.Notifications.Data;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications", "dbo");
            e.HasKey(n => n.NotificationId);
            e.Property(n => n.EventType).HasMaxLength(30).IsUnicode(false);
            e.Property(n => n.PatientName).HasMaxLength(101);
            e.Property(n => n.TherapistName).HasMaxLength(101);
            e.Property(n => n.SessionDateTime).HasColumnType("datetime2(0)");
            e.Property(n => n.SessionStatus).HasMaxLength(20).IsUnicode(false);
            e.Property(n => n.Message).HasMaxLength(500);
            e.Property(n => n.ReceivedAt).HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()").ValueGeneratedOnAdd();
        });
    }
}

using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Entities;

namespace PhysioCenter.Services.Data;

/// <summary>
/// EF Core mapping of the existing PhysioCenter database (created by the scripts in /sql).
/// The database is not created or migrated from code - the model only describes it.
/// </summary>
public class PhysioDbContext(DbContextOptions<PhysioDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Therapist> Therapists => Set<Therapist>();
    public DbSet<TherapyPlan> TherapyPlans => Set<TherapyPlan>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ProgressNote> ProgressNotes => Set<ProgressNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patients", "dbo");
            e.HasKey(p => p.PatientId);
            e.Property(p => p.FirstName).HasMaxLength(50);
            e.Property(p => p.LastName).HasMaxLength(50);
            e.Property(p => p.BirthDate).HasColumnType("date");
            e.Property(p => p.Gender).HasMaxLength(1).IsFixedLength().IsUnicode(false);
            e.Property(p => p.Phone).HasMaxLength(20);
            e.Property(p => p.Email).HasMaxLength(100);
            e.Property(p => p.Address).HasMaxLength(200);
            e.Property(p => p.CreatedAt).HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Therapist>(e =>
        {
            e.ToTable("Therapists", "dbo");
            e.HasKey(t => t.TherapistId);
            e.Property(t => t.FirstName).HasMaxLength(50);
            e.Property(t => t.LastName).HasMaxLength(50);
            e.Property(t => t.Specialization).HasMaxLength(100);
            e.Property(t => t.Phone).HasMaxLength(20);
            e.Property(t => t.Email).HasMaxLength(100);
        });

        modelBuilder.Entity<TherapyPlan>(e =>
        {
            e.ToTable("TherapyPlans", "dbo");
            e.HasKey(p => p.PlanId);
            e.Property(p => p.Diagnosis).HasMaxLength(200);
            e.Property(p => p.Goal).HasMaxLength(500);
            e.Property(p => p.StartDate).HasColumnType("date");
            e.Property(p => p.EndDate).HasColumnType("date");
            e.Property(p => p.Status).HasMaxLength(20).IsUnicode(false);
            e.Property(p => p.CreatedAt).HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()").ValueGeneratedOnAdd();

            // NO ACTION in the database: a patient / therapist with plans cannot be deleted.
            e.HasOne(p => p.Patient).WithMany(x => x.TherapyPlans)
                .HasForeignKey(p => p.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Therapist).WithMany(x => x.TherapyPlans)
                .HasForeignKey(p => p.TherapistId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.ToTable("Sessions", "dbo");
            e.HasKey(s => s.SessionId);
            e.Property(s => s.SessionDateTime).HasColumnType("datetime2(0)");
            e.Property(s => s.TreatmentType).HasMaxLength(100);
            e.Property(s => s.Status).HasMaxLength(20).IsUnicode(false);

            // ON DELETE CASCADE: sessions belong to their plan.
            e.HasOne(s => s.Plan).WithMany(p => p.Sessions)
                .HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Therapist).WithMany(t => t.Sessions)
                .HasForeignKey(s => s.TherapistId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProgressNote>(e =>
        {
            e.ToTable("ProgressNotes", "dbo");
            e.HasKey(n => n.NoteId);
            e.Property(n => n.NoteText).HasMaxLength(2000);
            e.Property(n => n.CreatedAt).HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()").ValueGeneratedOnAdd();

            // ON DELETE CASCADE: notes belong to their session.
            e.HasOne(n => n.Session).WithMany(s => s.ProgressNotes)
                .HasForeignKey(n => n.SessionId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

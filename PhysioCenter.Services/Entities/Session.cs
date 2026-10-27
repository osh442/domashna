namespace PhysioCenter.Services.Entities;

public class Session
{
    public int SessionId { get; set; }
    public int PlanId { get; set; }
    public int TherapistId { get; set; }
    public DateTime SessionDateTime { get; set; }
    public short DurationMinutes { get; set; } = 45;
    public string TreatmentType { get; set; } = string.Empty;
    public string Status { get; set; } = SessionStatus.Scheduled;

    public TherapyPlan Plan { get; set; } = null!;
    public Therapist Therapist { get; set; } = null!;
    public ICollection<ProgressNote> ProgressNotes { get; set; } = new List<ProgressNote>();
}

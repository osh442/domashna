namespace PhysioCenter.Services.Entities;

public class TherapyPlan
{
    public int PlanId { get; set; }
    public int PatientId { get; set; }
    public int TherapistId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public short PlannedSessions { get; set; }
    public string Status { get; set; } = PlanStatus.Active;
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Therapist Therapist { get; set; } = null!;
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

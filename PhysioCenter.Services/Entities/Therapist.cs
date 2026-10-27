namespace PhysioCenter.Services.Entities;

public class Therapist
{
    public int TherapistId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<TherapyPlan> TherapyPlans { get; set; } = new List<TherapyPlan>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

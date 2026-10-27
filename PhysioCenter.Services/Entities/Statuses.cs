namespace PhysioCenter.Services.Entities;

// Allowed values of the CHECK constraints on TherapyPlans.Status and Sessions.Status.
public static class PlanStatus
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string OnHold = "OnHold";
}

public static class SessionStatus
{
    public const string Scheduled = "Scheduled";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";
}

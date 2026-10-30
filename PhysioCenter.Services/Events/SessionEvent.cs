namespace PhysioCenter.Services.Events;

public record SessionEvent(
    string EventType,
    int SessionId,
    int PlanId,
    int PatientId,
    string PatientName,
    int TherapistId,
    string TherapistName,
    DateTime SessionDateTime,
    short DurationMinutes,
    string TreatmentType,
    string Status,
    DateTime OccurredAt);

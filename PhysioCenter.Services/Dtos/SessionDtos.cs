using System.ComponentModel.DataAnnotations;

namespace PhysioCenter.Services.Dtos;

/// <summary>Body of POST / PUT for a therapy session.</summary>
public class SessionRequest
{
    public int PlanId { get; init; }

    public int TherapistId { get; init; }

    /// <summary>Must fall inside the plan's period. The therapist must be free at that time.</summary>
    public DateTime SessionDateTime { get; init; }

    [Range(5, 240)]
    public short DurationMinutes { get; init; } = 45;

    [Required, StringLength(100)]
    public string TreatmentType { get; init; } = string.Empty;

    /// <summary>Scheduled, Completed, Cancelled or NoShow.</summary>
    [Required, AllowedValues("Scheduled", "Completed", "Cancelled", "NoShow")]
    public string Status { get; init; } = "Scheduled";
}

public record SessionResponse(
    int SessionId,
    int PlanId,
    int PatientId,
    string PatientName,
    int TherapistId,
    string TherapistName,
    DateTime SessionDateTime,
    short DurationMinutes,
    string TreatmentType,
    string Status);

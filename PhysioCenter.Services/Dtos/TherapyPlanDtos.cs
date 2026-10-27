using System.ComponentModel.DataAnnotations;

namespace PhysioCenter.Services.Dtos;

/// <summary>Body of POST / PUT for a therapy plan.</summary>
public class TherapyPlanRequest
{
    public int PatientId { get; init; }

    public int TherapistId { get; init; }

    [Required, StringLength(200)]
    public string Diagnosis { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Goal { get; init; }

    public DateOnly StartDate { get; init; }

    /// <summary>Optional; must not be before StartDate.</summary>
    public DateOnly? EndDate { get; init; }

    [Range(1, short.MaxValue)]
    public short PlannedSessions { get; init; }

    /// <summary>Active, Completed, Cancelled or OnHold.</summary>
    [Required, AllowedValues("Active", "Completed", "Cancelled", "OnHold")]
    public string Status { get; init; } = "Active";
}

public record TherapyPlanResponse(
    int PlanId,
    int PatientId,
    string PatientName,
    int TherapistId,
    string TherapistName,
    string Diagnosis,
    string? Goal,
    DateOnly StartDate,
    DateOnly? EndDate,
    short PlannedSessions,
    string Status,
    int CompletedSessions,
    DateTime CreatedAt);

/// <summary>One measurement of a patient's progress (a progress note of a session).</summary>
public record ProgressPoint(int SessionId, DateTime SessionDateTime, byte PainLevel, byte? MobilityScore);

/// <summary>Calculated summary of how a therapy plan is going.</summary>
public record PlanProgressResponse(
    int PlanId,
    int PatientId,
    string PatientName,
    string Diagnosis,
    string Status,
    short PlannedSessions,
    int CompletedSessions,
    double CompletionPercent,
    int NotesCount,
    double? AveragePainLevel,
    double? AverageMobilityScore,
    int? PainImprovement,
    int? MobilityImprovement,
    IReadOnlyList<ProgressPoint> Timeline);

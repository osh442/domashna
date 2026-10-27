using System.ComponentModel.DataAnnotations;

namespace PhysioCenter.Services.Dtos;

/// <summary>Body of POST / PUT for a progress note.</summary>
public class ProgressNoteRequest
{
    /// <summary>The session must be Completed.</summary>
    public int SessionId { get; init; }

    [Required, StringLength(2000)]
    public string NoteText { get; init; } = string.Empty;

    [Range(0, 10)]
    public byte PainLevel { get; init; }

    [Range(0, 10)]
    public byte? MobilityScore { get; init; }
}

public record ProgressNoteResponse(
    int NoteId,
    int SessionId,
    DateTime SessionDateTime,
    string NoteText,
    byte PainLevel,
    byte? MobilityScore,
    DateTime CreatedAt);

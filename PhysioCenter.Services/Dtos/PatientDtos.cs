using System.ComponentModel.DataAnnotations;

namespace PhysioCenter.Services.Dtos;

/// <summary>Body of POST / PUT for a patient.</summary>
public class PatientRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; init; } = string.Empty;

    /// <summary>Between 1900-01-01 and today.</summary>
    public DateOnly BirthDate { get; init; }

    /// <summary>M, F or O.</summary>
    [Required, AllowedValues("M", "F", "O")]
    public string Gender { get; init; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; init; } = string.Empty;

    /// <summary>Optional, but unique when present.</summary>
    [StringLength(100), RegularExpression(Validation.EmailPattern, ErrorMessage = "Email is not a valid e-mail address.")]
    public string? Email { get; init; }

    [StringLength(200)]
    public string? Address { get; init; }
}

public record PatientResponse(
    int PatientId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Gender,
    string Phone,
    string? Email,
    string? Address,
    DateTime CreatedAt);

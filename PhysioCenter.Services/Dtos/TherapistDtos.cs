using System.ComponentModel.DataAnnotations;

namespace PhysioCenter.Services.Dtos;

/// <summary>Body of POST / PUT for a therapist.</summary>
public class TherapistRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Specialization { get; init; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; init; }

    /// <summary>Unique.</summary>
    [Required, StringLength(100), RegularExpression(Validation.EmailPattern, ErrorMessage = "Email is not a valid e-mail address.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>Only active therapists can get new plans and sessions.</summary>
    public bool IsActive { get; init; } = true;
}

public record TherapistResponse(
    int TherapistId,
    string FirstName,
    string LastName,
    string Specialization,
    string? Phone,
    string Email,
    bool IsActive);

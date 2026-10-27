namespace PhysioCenter.Services.Dtos;

internal static class Validation
{
    // Same idea as the CHECK (Email LIKE '%_@_%._%') constraint in the database.
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
}

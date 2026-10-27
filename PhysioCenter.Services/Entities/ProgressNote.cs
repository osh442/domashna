namespace PhysioCenter.Services.Entities;

public class ProgressNote
{
    public int NoteId { get; set; }
    public int SessionId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public byte PainLevel { get; set; }
    public byte? MobilityScore { get; set; }
    public DateTime CreatedAt { get; set; }

    public Session Session { get; set; } = null!;
}

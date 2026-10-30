namespace PhysioCenter.Notifications.Data;

public class Notification
{
    public int NotificationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int TherapistId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    public DateTime SessionDateTime { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}

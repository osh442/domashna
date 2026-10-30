using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Notifications.Data;

namespace PhysioCenter.Notifications.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController(NotificationsDbContext db) : ControllerBase
{
    /// <summary>Lists notifications, newest first. Filter by event type, session or patient.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Notification>>> GetAll(
        [FromQuery] string? eventType, [FromQuery] int? sessionId, [FromQuery] int? patientId, CancellationToken ct)
    {
        var query = db.Notifications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(n => n.EventType == eventType);
        if (sessionId is not null) query = query.Where(n => n.SessionId == sessionId);
        if (patientId is not null) query = query.Where(n => n.PatientId == patientId);

        return Ok(await query.OrderByDescending(n => n.NotificationId).ToListAsync(ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Notification>> GetById(int id, CancellationToken ct)
    {
        var notification = await db.Notifications.AsNoTracking().FirstOrDefaultAsync(n => n.NotificationId == id, ct);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await db.Notifications.Where(n => n.NotificationId == id).ExecuteDeleteAsync(ct);
        return deleted == 0 ? NotFound() : NoContent();
    }
}

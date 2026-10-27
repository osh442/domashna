using Microsoft.AspNetCore.Mvc;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public class SessionsController(ISessionService sessions) : ControllerBase
{
    /// <summary>
    /// Lists sessions. Filter by plan, therapist (a therapist's schedule), status
    /// and a date range (<c>from</c> and <c>to</c> are inclusive).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> GetAll(
        [FromQuery] int? planId, [FromQuery] int? therapistId, [FromQuery] string? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) =>
        Ok(await sessions.GetAllAsync(planId, therapistId, status, from, to, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await sessions.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> Create(SessionRequest request, CancellationToken ct)
    {
        var created = await sessions.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.SessionId }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> Update(int id, SessionRequest request, CancellationToken ct) =>
        Ok(await sessions.UpdateAsync(id, request, ct));

    /// <summary>Deletes the session together with its progress notes.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sessions.DeleteAsync(id, ct);
        return NoContent();
    }
}

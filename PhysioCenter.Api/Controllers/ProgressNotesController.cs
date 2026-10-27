using Microsoft.AspNetCore.Mvc;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Api.Controllers;

[ApiController]
[Route("api/progress-notes")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public class ProgressNotesController(IProgressNoteService notes) : ControllerBase
{
    /// <summary>Lists progress notes, optionally for one session or one therapy plan.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgressNoteResponse>>> GetAll(
        [FromQuery] int? sessionId, [FromQuery] int? planId, CancellationToken ct) =>
        Ok(await notes.GetAllAsync(sessionId, planId, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgressNoteResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await notes.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(ProgressNoteResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProgressNoteResponse>> Create(ProgressNoteRequest request, CancellationToken ct)
    {
        var created = await notes.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.NoteId }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgressNoteResponse>> Update(int id, ProgressNoteRequest request, CancellationToken ct) =>
        Ok(await notes.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await notes.DeleteAsync(id, ct);
        return NoContent();
    }
}

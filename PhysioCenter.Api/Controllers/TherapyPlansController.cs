using Microsoft.AspNetCore.Mvc;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Api.Controllers;

[ApiController]
[Route("api/therapy-plans")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public class TherapyPlansController(ITherapyPlanService plans) : ControllerBase
{
    /// <summary>Lists therapy plans, optionally filtered by patient, therapist and status.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TherapyPlanResponse>>> GetAll(
        [FromQuery] int? patientId, [FromQuery] int? therapistId, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await plans.GetAllAsync(patientId, therapistId, status, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TherapyPlanResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await plans.GetByIdAsync(id, ct));

    /// <summary>Completed sessions, average pain / mobility and how much they improved over the plan.</summary>
    [HttpGet("{id:int}/progress")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanProgressResponse>> GetProgress(int id, CancellationToken ct) =>
        Ok(await plans.GetProgressAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(TherapyPlanResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<TherapyPlanResponse>> Create(TherapyPlanRequest request, CancellationToken ct)
    {
        var created = await plans.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.PlanId }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TherapyPlanResponse>> Update(int id, TherapyPlanRequest request, CancellationToken ct) =>
        Ok(await plans.UpdateAsync(id, request, ct));

    /// <summary>Deletes the plan together with its sessions and their progress notes.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await plans.DeleteAsync(id, ct);
        return NoContent();
    }
}

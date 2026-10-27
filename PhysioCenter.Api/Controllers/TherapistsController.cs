using Microsoft.AspNetCore.Mvc;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Api.Controllers;

[ApiController]
[Route("api/therapists")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public class TherapistsController(ITherapistService therapists) : ControllerBase
{
    /// <summary>Lists therapists; <c>activeOnly=true</c> hides inactive ones.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TherapistResponse>>> GetAll([FromQuery] bool activeOnly, CancellationToken ct) =>
        Ok(await therapists.GetAllAsync(activeOnly, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TherapistResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await therapists.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(TherapistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TherapistResponse>> Create(TherapistRequest request, CancellationToken ct)
    {
        var created = await therapists.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.TherapistId }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TherapistResponse>> Update(int id, TherapistRequest request, CancellationToken ct) =>
        Ok(await therapists.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await therapists.DeleteAsync(id, ct);
        return NoContent();
    }
}

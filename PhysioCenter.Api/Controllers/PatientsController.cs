using Microsoft.AspNetCore.Mvc;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public class PatientsController(IPatientService patients) : ControllerBase
{
    /// <summary>Lists patients; <c>search</c> matches name, phone or e-mail.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetAll([FromQuery] string? search, CancellationToken ct) =>
        Ok(await patients.GetAllAsync(search, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await patients.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatientResponse>> Create(PatientRequest request, CancellationToken ct)
    {
        var created = await patients.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.PatientId }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatientResponse>> Update(int id, PatientRequest request, CancellationToken ct) =>
        Ok(await patients.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await patients.DeleteAsync(id, ct);
        return NoContent();
    }
}

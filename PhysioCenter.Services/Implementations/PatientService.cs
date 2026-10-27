using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Entities;
using PhysioCenter.Services.Exceptions;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services.Implementations;

public class PatientService(PhysioDbContext db) : IPatientService
{
    private static readonly Expression<Func<Patient, PatientResponse>> ToResponse = p => new PatientResponse(
        p.PatientId, p.FirstName, p.LastName, p.BirthDate, p.Gender, p.Phone, p.Email, p.Address, p.CreatedAt);

    public async Task<IReadOnlyList<PatientResponse>> GetAllAsync(string? search, CancellationToken ct = default)
    {
        var query = db.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.FirstName.Contains(term) || p.LastName.Contains(term)
                                     || p.Phone.Contains(term) || (p.Email != null && p.Email.Contains(term)));
        }

        return await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(ToResponse).ToListAsync(ct);
    }

    public async Task<PatientResponse> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Patients.AsNoTracking().Where(p => p.PatientId == id).Select(ToResponse).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(Patient), id);

    public async Task<PatientResponse> CreateAsync(PatientRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, 0, ct);

        var patient = new Patient();
        Apply(patient, request);
        db.Patients.Add(patient);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(patient.PatientId, ct);
    }

    public async Task<PatientResponse> UpdateAsync(int id, PatientRequest request, CancellationToken ct = default)
    {
        var patient = await db.Patients.FindAsync([id], ct) ?? throw new NotFoundException(nameof(Patient), id);
        await ValidateAsync(request, id, ct);

        Apply(patient, request);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var patient = await db.Patients.FindAsync([id], ct) ?? throw new NotFoundException(nameof(Patient), id);

        if (await db.TherapyPlans.AnyAsync(p => p.PatientId == id, ct))
            throw new ConflictException("The patient has therapy plans and cannot be deleted. Delete the plans first.");

        db.Patients.Remove(patient);
        await db.SaveOrTranslateAsync(ct);
    }

    private async Task ValidateAsync(PatientRequest request, int patientId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (request.BirthDate < new DateOnly(1900, 1, 1) || request.BirthDate > today)
            throw new BusinessRuleException("BirthDate must be between 1900-01-01 and today.");

        var email = NullIfBlank(request.Email);
        if (email is not null && await db.Patients.AnyAsync(p => p.Email == email && p.PatientId != patientId, ct))
            throw new ConflictException($"A patient with e-mail '{email}' already exists.");
    }

    private static void Apply(Patient patient, PatientRequest request)
    {
        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.BirthDate = request.BirthDate;
        patient.Gender = request.Gender;
        patient.Phone = request.Phone.Trim();
        patient.Email = NullIfBlank(request.Email);
        patient.Address = NullIfBlank(request.Address);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

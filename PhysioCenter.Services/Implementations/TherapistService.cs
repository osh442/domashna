using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Entities;
using PhysioCenter.Services.Exceptions;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services.Implementations;

public class TherapistService(PhysioDbContext db) : ITherapistService
{
    private static readonly Expression<Func<Therapist, TherapistResponse>> ToResponse = t => new TherapistResponse(
        t.TherapistId, t.FirstName, t.LastName, t.Specialization, t.Phone, t.Email, t.IsActive);

    public async Task<IReadOnlyList<TherapistResponse>> GetAllAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = db.Therapists.AsNoTracking();
        if (activeOnly)
            query = query.Where(t => t.IsActive);

        return await query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .Select(ToResponse).ToListAsync(ct);
    }

    public async Task<TherapistResponse> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Therapists.AsNoTracking().Where(t => t.TherapistId == id).Select(ToResponse).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(Therapist), id);

    public async Task<TherapistResponse> CreateAsync(TherapistRequest request, CancellationToken ct = default)
    {
        await EnsureEmailIsFreeAsync(request.Email, 0, ct);

        var therapist = new Therapist();
        Apply(therapist, request);
        db.Therapists.Add(therapist);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(therapist.TherapistId, ct);
    }

    public async Task<TherapistResponse> UpdateAsync(int id, TherapistRequest request, CancellationToken ct = default)
    {
        var therapist = await db.Therapists.FindAsync([id], ct) ?? throw new NotFoundException(nameof(Therapist), id);
        await EnsureEmailIsFreeAsync(request.Email, id, ct);

        Apply(therapist, request);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var therapist = await db.Therapists.FindAsync([id], ct) ?? throw new NotFoundException(nameof(Therapist), id);

        if (await db.TherapyPlans.AnyAsync(p => p.TherapistId == id, ct) ||
            await db.Sessions.AnyAsync(s => s.TherapistId == id, ct))
            throw new ConflictException("The therapist has plans or sessions and cannot be deleted. Set IsActive to false instead.");

        db.Therapists.Remove(therapist);
        await db.SaveOrTranslateAsync(ct);
    }

    private async Task EnsureEmailIsFreeAsync(string email, int therapistId, CancellationToken ct)
    {
        var trimmed = email.Trim();
        if (await db.Therapists.AnyAsync(t => t.Email == trimmed && t.TherapistId != therapistId, ct))
            throw new ConflictException($"A therapist with e-mail '{trimmed}' already exists.");
    }

    private static void Apply(Therapist therapist, TherapistRequest request)
    {
        therapist.FirstName = request.FirstName.Trim();
        therapist.LastName = request.LastName.Trim();
        therapist.Specialization = request.Specialization.Trim();
        therapist.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        therapist.Email = request.Email.Trim();
        therapist.IsActive = request.IsActive;
    }
}

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Entities;
using PhysioCenter.Services.Exceptions;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services.Implementations;

public class TherapyPlanService(PhysioDbContext db) : ITherapyPlanService
{
    private static readonly Expression<Func<TherapyPlan, TherapyPlanResponse>> ToResponse = p => new TherapyPlanResponse(
        p.PlanId,
        p.PatientId, p.Patient.FirstName + " " + p.Patient.LastName,
        p.TherapistId, p.Therapist.FirstName + " " + p.Therapist.LastName,
        p.Diagnosis, p.Goal, p.StartDate, p.EndDate, p.PlannedSessions, p.Status,
        p.Sessions.Count(s => s.Status == SessionStatus.Completed),
        p.CreatedAt);

    public async Task<IReadOnlyList<TherapyPlanResponse>> GetAllAsync(
        int? patientId, int? therapistId, string? status, CancellationToken ct = default)
    {
        var query = db.TherapyPlans.AsNoTracking();

        if (patientId is not null) query = query.Where(p => p.PatientId == patientId);
        if (therapistId is not null) query = query.Where(p => p.TherapistId == therapistId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);

        return await query.OrderByDescending(p => p.StartDate).ThenBy(p => p.PlanId)
            .Select(ToResponse).ToListAsync(ct);
    }

    public async Task<TherapyPlanResponse> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.TherapyPlans.AsNoTracking().Where(p => p.PlanId == id).Select(ToResponse).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(TherapyPlan), id);

    public async Task<PlanProgressResponse> GetProgressAsync(int id, CancellationToken ct = default)
    {
        var plan = await db.TherapyPlans.AsNoTracking()
            .Where(p => p.PlanId == id)
            .Select(p => new
            {
                p.PlanId, p.PatientId, p.Diagnosis, p.Status, p.PlannedSessions,
                PatientName = p.Patient.FirstName + " " + p.Patient.LastName
            })
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException(nameof(TherapyPlan), id);

        var completed = await db.Sessions.CountAsync(s => s.PlanId == id && s.Status == SessionStatus.Completed, ct);

        var timeline = await db.ProgressNotes.AsNoTracking()
            .Where(n => n.Session.PlanId == id)
            .OrderBy(n => n.Session.SessionDateTime).ThenBy(n => n.NoteId)
            .Select(n => new ProgressPoint(n.SessionId, n.Session.SessionDateTime, n.PainLevel, n.MobilityScore))
            .ToListAsync(ct);

        var mobility = timeline.Where(t => t.MobilityScore is not null).ToList();

        // Improvement = how much better the patient is now than at the first note:
        // less pain is better, more mobility is better.
        int? painImprovement = timeline.Count >= 2 ? timeline[0].PainLevel - timeline[^1].PainLevel : null;
        int? mobilityImprovement = mobility.Count >= 2 ? mobility[^1].MobilityScore - mobility[0].MobilityScore : null;

        return new PlanProgressResponse(
            plan.PlanId, plan.PatientId, plan.PatientName, plan.Diagnosis, plan.Status,
            plan.PlannedSessions, completed,
            Math.Round(completed * 100.0 / plan.PlannedSessions, 1),
            timeline.Count,
            timeline.Count > 0 ? Math.Round(timeline.Average(t => (double)t.PainLevel), 1) : null,
            mobility.Count > 0 ? Math.Round(mobility.Average(t => (double)t.MobilityScore!.Value), 1) : null,
            painImprovement,
            mobilityImprovement,
            timeline);
    }

    public async Task<TherapyPlanResponse> CreateAsync(TherapyPlanRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, existing: null, ct);

        var plan = new TherapyPlan();
        Apply(plan, request);
        db.TherapyPlans.Add(plan);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(plan.PlanId, ct);
    }

    public async Task<TherapyPlanResponse> UpdateAsync(int id, TherapyPlanRequest request, CancellationToken ct = default)
    {
        var plan = await db.TherapyPlans.FindAsync([id], ct) ?? throw new NotFoundException(nameof(TherapyPlan), id);
        await ValidateAsync(request, plan, ct);

        Apply(plan, request);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var plan = await db.TherapyPlans.FindAsync([id], ct) ?? throw new NotFoundException(nameof(TherapyPlan), id);

        // The database cascades the delete to the plan's sessions and their progress notes.
        db.TherapyPlans.Remove(plan);
        await db.SaveOrTranslateAsync(ct);
    }

    private async Task ValidateAsync(TherapyPlanRequest request, TherapyPlan? existing, CancellationToken ct)
    {
        if (request.StartDate < new DateOnly(1900, 1, 1))
            throw new BusinessRuleException("StartDate must be a valid date (1900-01-01 or later).");
        if (request.EndDate is { } end && end < request.StartDate)
            throw new BusinessRuleException("EndDate cannot be before StartDate.");

        if (!await db.Patients.AnyAsync(p => p.PatientId == request.PatientId, ct))
            throw new BusinessRuleException($"Patient {request.PatientId} does not exist.");

        var therapist = await db.Therapists.AsNoTracking()
            .Where(t => t.TherapistId == request.TherapistId)
            .Select(t => new { t.IsActive })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException($"Therapist {request.TherapistId} does not exist.");

        // An inactive therapist may stay on old plans, but cannot be given a new one.
        if (!therapist.IsActive && (existing is null || existing.TherapistId != request.TherapistId))
            throw new BusinessRuleException($"Therapist {request.TherapistId} is not active.");

        if (existing is not null)
        {
            // The plan cannot promise fewer sessions than it already has booked or held.
            var booked = await db.Sessions.CountAsync(
                s => s.PlanId == existing.PlanId && s.Status != SessionStatus.Cancelled, ct);
            if (request.PlannedSessions < booked)
                throw new BusinessRuleException(
                    $"PlannedSessions ({request.PlannedSessions}) is less than the {booked} sessions already booked in this plan.");
        }
    }

    private static void Apply(TherapyPlan plan, TherapyPlanRequest request)
    {
        plan.PatientId = request.PatientId;
        plan.TherapistId = request.TherapistId;
        plan.Diagnosis = request.Diagnosis.Trim();
        plan.Goal = string.IsNullOrWhiteSpace(request.Goal) ? null : request.Goal.Trim();
        plan.StartDate = request.StartDate;
        plan.EndDate = request.EndDate;
        plan.PlannedSessions = request.PlannedSessions;
        plan.Status = request.Status;
    }
}

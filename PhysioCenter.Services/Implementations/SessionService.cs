using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Entities;
using PhysioCenter.Services.Events;
using PhysioCenter.Services.Exceptions;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services.Implementations;

public class SessionService(PhysioDbContext db, IEventPublisher events) : ISessionService
{
    private const int MaxDurationMinutes = 240; // CK_Sessions_Duration

    private static readonly Expression<Func<Session, SessionResponse>> ToResponse = s => new SessionResponse(
        s.SessionId, s.PlanId,
        s.Plan.PatientId, s.Plan.Patient.FirstName + " " + s.Plan.Patient.LastName,
        s.TherapistId, s.Therapist.FirstName + " " + s.Therapist.LastName,
        s.SessionDateTime, s.DurationMinutes, s.TreatmentType, s.Status);

    public async Task<IReadOnlyList<SessionResponse>> GetAllAsync(
        int? planId, int? therapistId, string? status, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = db.Sessions.AsNoTracking();

        if (planId is not null) query = query.Where(s => s.PlanId == planId);
        if (therapistId is not null) query = query.Where(s => s.TherapistId == therapistId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(s => s.Status == status);
        if (from is not null) query = query.Where(s => s.SessionDateTime >= from);
        if (to is not null) query = query.Where(s => s.SessionDateTime <= to);

        return await query.OrderBy(s => s.SessionDateTime).ThenBy(s => s.SessionId)
            .Select(ToResponse).ToListAsync(ct);
    }

    public async Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Sessions.AsNoTracking().Where(s => s.SessionId == id).Select(ToResponse).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(Session), id);

    public async Task<SessionResponse> CreateAsync(SessionRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, existing: null, ct);

        var session = new Session();
        Apply(session, request);
        db.Sessions.Add(session);
        await db.SaveOrTranslateAsync(ct);

        var created = await GetByIdAsync(session.SessionId, ct);
        Publish("SessionCreated", created);
        return created;
    }

    public async Task<SessionResponse> UpdateAsync(int id, SessionRequest request, CancellationToken ct = default)
    {
        var session = await db.Sessions.FindAsync([id], ct) ?? throw new NotFoundException(nameof(Session), id);
        await ValidateAsync(request, session, ct);

        Apply(session, request);
        await db.SaveOrTranslateAsync(ct);

        var updated = await GetByIdAsync(id, ct);
        Publish("SessionUpdated", updated);
        return updated;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var session = await db.Sessions.FindAsync([id], ct) ?? throw new NotFoundException(nameof(Session), id);

        var deleted = await GetByIdAsync(id, ct);

        // The database cascades the delete to the session's progress notes.
        db.Sessions.Remove(session);
        await db.SaveOrTranslateAsync(ct);

        Publish("SessionDeleted", deleted);
    }

    private void Publish(string eventType, SessionResponse s) =>
        events.Publish(new SessionEvent(
            eventType, s.SessionId, s.PlanId, s.PatientId, s.PatientName, s.TherapistId, s.TherapistName,
            s.SessionDateTime, s.DurationMinutes, s.TreatmentType, s.Status, DateTime.UtcNow));

    private async Task ValidateAsync(SessionRequest request, Session? existing, CancellationToken ct)
    {
        if (request.SessionDateTime.Year < 2000)
            throw new BusinessRuleException("SessionDateTime must be a valid date and time.");

        var plan = await db.TherapyPlans.AsNoTracking()
            .Where(p => p.PlanId == request.PlanId)
            .Select(p => new { p.Status, p.StartDate, p.EndDate, p.PlannedSessions })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException($"Therapy plan {request.PlanId} does not exist.");

        var therapist = await db.Therapists.AsNoTracking()
            .Where(t => t.TherapistId == request.TherapistId)
            .Select(t => new { t.IsActive })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException($"Therapist {request.TherapistId} does not exist.");

        // New sessions only go into running plans.
        if (existing is null && plan.Status != PlanStatus.Active)
            throw new BusinessRuleException($"Sessions can only be added to an Active plan (the plan is {plan.Status}).");

        if (!therapist.IsActive && (existing is null || existing.TherapistId != request.TherapistId))
            throw new BusinessRuleException($"Therapist {request.TherapistId} is not active.");

        // The session has to happen inside the period of its plan.
        var day = DateOnly.FromDateTime(request.SessionDateTime);
        if (day < plan.StartDate || (plan.EndDate is { } end && day > end))
            throw new BusinessRuleException(
                $"The session date {day:yyyy-MM-dd} is outside the plan period ({plan.StartDate:yyyy-MM-dd} - {plan.EndDate:yyyy-MM-dd}).");

        // A cancelled session does not use up a slot or a place in the plan.
        if (request.Status == SessionStatus.Cancelled)
            return;

        var selfId = existing?.SessionId ?? 0;

        var booked = await db.Sessions.CountAsync(
            s => s.PlanId == request.PlanId && s.Status != SessionStatus.Cancelled && s.SessionId != selfId, ct);
        if (booked + 1 > plan.PlannedSessions)
            throw new BusinessRuleException(
                $"The plan allows {plan.PlannedSessions} sessions and {booked} are already booked.");

        await EnsureTherapistIsFreeAsync(request, selfId, ct);
    }

    // The database only blocks two sessions with the same start time (UX_Sessions_Therapist_Start).
    // Here sessions that merely overlap are rejected too.
    private async Task EnsureTherapistIsFreeAsync(SessionRequest request, int selfId, CancellationToken ct)
    {
        var start = request.SessionDateTime;
        var end = start.AddMinutes(request.DurationMinutes);
        var earliestStart = start.AddMinutes(-MaxDurationMinutes);

        var nearby = await db.Sessions.AsNoTracking()
            .Where(s => s.TherapistId == request.TherapistId
                        && s.Status != SessionStatus.Cancelled
                        && s.SessionId != selfId
                        && s.SessionDateTime > earliestStart
                        && s.SessionDateTime < end)
            .Select(s => new { s.SessionId, s.SessionDateTime, s.DurationMinutes })
            .ToListAsync(ct);

        var clash = nearby.FirstOrDefault(s => s.SessionDateTime.AddMinutes(s.DurationMinutes) > start);
        if (clash is not null)
            throw new ConflictException(
                $"Therapist {request.TherapistId} already has session {clash.SessionId} at {clash.SessionDateTime:yyyy-MM-dd HH:mm} that overlaps this time.");
    }

    private static void Apply(Session session, SessionRequest request)
    {
        session.PlanId = request.PlanId;
        session.TherapistId = request.TherapistId;
        session.SessionDateTime = request.SessionDateTime;
        session.DurationMinutes = request.DurationMinutes;
        session.TreatmentType = request.TreatmentType.Trim();
        session.Status = request.Status;
    }
}

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Dtos;
using PhysioCenter.Services.Entities;
using PhysioCenter.Services.Exceptions;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services.Implementations;

public class ProgressNoteService(PhysioDbContext db) : IProgressNoteService
{
    private static readonly Expression<Func<ProgressNote, ProgressNoteResponse>> ToResponse = n => new ProgressNoteResponse(
        n.NoteId, n.SessionId, n.Session.SessionDateTime, n.NoteText, n.PainLevel, n.MobilityScore, n.CreatedAt);

    public async Task<IReadOnlyList<ProgressNoteResponse>> GetAllAsync(
        int? sessionId, int? planId, CancellationToken ct = default)
    {
        var query = db.ProgressNotes.AsNoTracking();

        if (sessionId is not null) query = query.Where(n => n.SessionId == sessionId);
        if (planId is not null) query = query.Where(n => n.Session.PlanId == planId);

        return await query.OrderBy(n => n.Session.SessionDateTime).ThenBy(n => n.NoteId)
            .Select(ToResponse).ToListAsync(ct);
    }

    public async Task<ProgressNoteResponse> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.ProgressNotes.AsNoTracking().Where(n => n.NoteId == id).Select(ToResponse).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException(nameof(ProgressNote), id);

    public async Task<ProgressNoteResponse> CreateAsync(ProgressNoteRequest request, CancellationToken ct = default)
    {
        await EnsureSessionIsCompletedAsync(request.SessionId, ct);

        var note = new ProgressNote();
        Apply(note, request);
        db.ProgressNotes.Add(note);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(note.NoteId, ct);
    }

    public async Task<ProgressNoteResponse> UpdateAsync(int id, ProgressNoteRequest request, CancellationToken ct = default)
    {
        var note = await db.ProgressNotes.FindAsync([id], ct) ?? throw new NotFoundException(nameof(ProgressNote), id);

        if (note.SessionId != request.SessionId)
            await EnsureSessionIsCompletedAsync(request.SessionId, ct);

        Apply(note, request);
        await db.SaveOrTranslateAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var note = await db.ProgressNotes.FindAsync([id], ct) ?? throw new NotFoundException(nameof(ProgressNote), id);

        db.ProgressNotes.Remove(note);
        await db.SaveOrTranslateAsync(ct);
    }

    // Notes document a session that took place.
    private async Task EnsureSessionIsCompletedAsync(int sessionId, CancellationToken ct)
    {
        var status = await db.Sessions.AsNoTracking()
            .Where(s => s.SessionId == sessionId)
            .Select(s => s.Status)
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException($"Session {sessionId} does not exist.");

        if (status != SessionStatus.Completed)
            throw new BusinessRuleException($"Progress notes can only be added to a Completed session (session {sessionId} is {status}).");
    }

    private static void Apply(ProgressNote note, ProgressNoteRequest request)
    {
        note.SessionId = request.SessionId;
        note.NoteText = request.NoteText.Trim();
        note.PainLevel = request.PainLevel;
        note.MobilityScore = request.MobilityScore;
    }
}

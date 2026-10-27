using PhysioCenter.Services.Dtos;

namespace PhysioCenter.Services.Interfaces;

// Contracts of the service layer. The API controllers depend only on these interfaces.
// Failures are reported with NotFoundException (404), ConflictException (409)
// and BusinessRuleException (400) from PhysioCenter.Services.Exceptions.

public interface IPatientService
{
    Task<IReadOnlyList<PatientResponse>> GetAllAsync(string? search, CancellationToken ct = default);
    Task<PatientResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PatientResponse> CreateAsync(PatientRequest request, CancellationToken ct = default);
    Task<PatientResponse> UpdateAsync(int id, PatientRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ITherapistService
{
    Task<IReadOnlyList<TherapistResponse>> GetAllAsync(bool activeOnly, CancellationToken ct = default);
    Task<TherapistResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TherapistResponse> CreateAsync(TherapistRequest request, CancellationToken ct = default);
    Task<TherapistResponse> UpdateAsync(int id, TherapistRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ITherapyPlanService
{
    Task<IReadOnlyList<TherapyPlanResponse>> GetAllAsync(int? patientId, int? therapistId, string? status, CancellationToken ct = default);
    Task<TherapyPlanResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PlanProgressResponse> GetProgressAsync(int id, CancellationToken ct = default);
    Task<TherapyPlanResponse> CreateAsync(TherapyPlanRequest request, CancellationToken ct = default);
    Task<TherapyPlanResponse> UpdateAsync(int id, TherapyPlanRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ISessionService
{
    Task<IReadOnlyList<SessionResponse>> GetAllAsync(int? planId, int? therapistId, string? status, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SessionResponse> CreateAsync(SessionRequest request, CancellationToken ct = default);
    Task<SessionResponse> UpdateAsync(int id, SessionRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IProgressNoteService
{
    Task<IReadOnlyList<ProgressNoteResponse>> GetAllAsync(int? sessionId, int? planId, CancellationToken ct = default);
    Task<ProgressNoteResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProgressNoteResponse> CreateAsync(ProgressNoteRequest request, CancellationToken ct = default);
    Task<ProgressNoteResponse> UpdateAsync(int id, ProgressNoteRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

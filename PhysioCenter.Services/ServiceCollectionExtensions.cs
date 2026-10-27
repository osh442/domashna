using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Implementations;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the DbContext and all services of the service layer.</summary>
    public static IServiceCollection AddPhysioCenterServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PhysioDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ITherapistService, TherapistService>();
        services.AddScoped<ITherapyPlanService, TherapyPlanService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IProgressNoteService, ProgressNoteService>();

        return services;
    }
}

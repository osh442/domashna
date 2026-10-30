using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Implementations;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the DbContext, the Kafka event publisher and all services of the service layer.</summary>
    public static IServiceCollection AddPhysioCenterServices(
        this IServiceCollection services, string connectionString, string kafkaBootstrapServers, string sessionsTopic)
    {
        services.AddDbContext<PhysioDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddSingleton<IEventPublisher>(provider => new KafkaEventPublisher(
            kafkaBootstrapServers, sessionsTopic, provider.GetRequiredService<ILogger<KafkaEventPublisher>>()));

        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ITherapistService, TherapistService>();
        services.AddScoped<ITherapyPlanService, TherapyPlanService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IProgressNoteService, ProgressNoteService>();

        return services;
    }
}

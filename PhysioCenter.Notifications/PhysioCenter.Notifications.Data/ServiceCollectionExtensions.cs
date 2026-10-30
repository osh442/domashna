using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PhysioCenter.Notifications.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        return services;
    }
}

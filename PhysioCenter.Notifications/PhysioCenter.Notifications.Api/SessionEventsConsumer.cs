using System.Text.Json;
using Confluent.Kafka;
using PhysioCenter.Notifications.Data;

namespace PhysioCenter.Notifications.Api;

public class SessionEventsConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SessionEventsConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(configuration["Kafka:SessionsTopic"]);
        logger.LogInformation("Listening to topic {Topic}", configuration["Kafka:SessionsTopic"]);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message is null)
                    continue;

                await SaveAsync(result.Message.Value, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not process a Kafka message");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        consumer.Close();
    }

    private async Task SaveAsync(string json, CancellationToken ct)
    {
        var sessionEvent = JsonSerializer.Deserialize<SessionEvent>(json, JsonOptions);
        if (sessionEvent is null)
            return;

        var notification = new Notification
        {
            EventType = sessionEvent.EventType,
            SessionId = sessionEvent.SessionId,
            PatientId = sessionEvent.PatientId,
            PatientName = sessionEvent.PatientName,
            TherapistId = sessionEvent.TherapistId,
            TherapistName = sessionEvent.TherapistName,
            SessionDateTime = sessionEvent.SessionDateTime,
            SessionStatus = sessionEvent.Status,
            Message = BuildMessage(sessionEvent)
        };

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Saved notification: {Message}", notification.Message);
    }

    private static string BuildMessage(SessionEvent e)
    {
        var who = $"session {e.SessionId} of {e.PatientName} with {e.TherapistName} on {e.SessionDateTime:yyyy-MM-dd HH:mm}";

        return e.EventType switch
        {
            "SessionCreated" => $"New {who} was scheduled.",
            "SessionUpdated" => $"The {who} was updated, status is now {e.Status}.",
            "SessionDeleted" => $"The {who} was deleted.",
            _ => $"Event {e.EventType} for {who}."
        };
    }
}

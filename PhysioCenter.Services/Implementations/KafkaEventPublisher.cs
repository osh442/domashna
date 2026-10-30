using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using PhysioCenter.Services.Events;
using PhysioCenter.Services.Interfaces;

namespace PhysioCenter.Services.Implementations;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IProducer<string, string> producer;
    private readonly string topic;
    private readonly ILogger<KafkaEventPublisher> logger;

    public KafkaEventPublisher(string bootstrapServers, string topic, ILogger<KafkaEventPublisher> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            MessageTimeoutMs = 5000
        };

        producer = new ProducerBuilder<string, string>(config).Build();
        this.topic = topic;
        this.logger = logger;
    }

    public void Publish(SessionEvent sessionEvent)
    {
        var message = new Message<string, string>
        {
            Key = sessionEvent.SessionId.ToString(),
            Value = JsonSerializer.Serialize(sessionEvent, JsonOptions)
        };

        producer.Produce(topic, message, report =>
        {
            if (report.Error.IsError)
                logger.LogWarning("Kafka message for session {SessionId} was not delivered: {Reason}",
                    sessionEvent.SessionId, report.Error.Reason);
        });
    }

    public void Dispose()
    {
        producer.Flush(TimeSpan.FromSeconds(5));
        producer.Dispose();
    }
}

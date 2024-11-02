using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitMq.QueueLibrary;

public class QueueProducer : QueueClient, IQueueProducer
{
    private readonly ILogger<QueueProducer> _logger;

    public QueueProducer(IOptions<QueueOptions> options, ILogger<QueueProducer> logger) : base(options.Value.ConnectionString, options.Value.QueueName)
    {
        this._logger = logger;
    }

    public void Send(string body, Guid documentId)
    {
        _logger.LogInformation($"Sending message with id {documentId}");

        IBasicProperties properties = base.RabbitMqChannel.CreateBasicProperties();
        properties.CorrelationId = documentId.ToString();

        base.RabbitMqChannel.BasicPublish(exchange: ExchangeName,
                                     routingKey: $"{QueueName}.*",
                                     mandatory: false,
                                     basicProperties: properties,
                                     body: System.Text.Encoding.UTF8.GetBytes(body));
    }
}
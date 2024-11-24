using Microsoft.Extensions.Logging;
using RabbitMq.QueueLibrary;
using System.Threading;

namespace TesseractOcr
{
    public class QueueService: IQueueService
    {
        private readonly IQueueProducer _queueProducer;
        private readonly IQueueConsumer _queueConsumer;
        private readonly ILogger<QueueConsumer> _logger;

        public QueueService(IQueueProducer queueProducer, IQueueConsumer queueConsumer, ILogger<QueueConsumer> logger)
        {
            _queueProducer = queueProducer;
            _queueConsumer = queueConsumer;
            _logger = logger;
        }

        public void Start() {
            _queueConsumer.OnReceived += (sender, eventArgs) =>
            {
                _logger.LogInformation($"Received message {eventArgs.MessageId}");
                _queueProducer.Send(eventArgs.Content, eventArgs.MessageId);
            };
            _queueConsumer.StartReceive();

            var cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                cancellationTokenSource.Cancel();
            };

            while (true)
            {
                Thread.Sleep(1000); 
            }

        }
        private void RespondToQueue(string message)
        {
            ;
        }
    }
}

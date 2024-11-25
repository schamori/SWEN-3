using Microsoft.Extensions.Logging;
using RabbitMq.QueueLibrary;
using System;
using System.Threading;
using FileStorageService.Controllers;

namespace TesseractOcr
{
    public class QueueService: IQueueService
    {
        private readonly IQueueProducer _queueProducer;
        private readonly IQueueConsumer _queueConsumer;
        private readonly ILogger<QueueConsumer> _logger;
        private readonly IOcrClient _ocrClient;
        private readonly IFilesApi _filesApi;

        public QueueService(IQueueProducer queueProducer, IQueueConsumer queueConsumer, ILogger<QueueConsumer> logger, IOcrClient ocrClient, IFilesApi filesApi)
        {
            _queueProducer = queueProducer;
            _queueConsumer = queueConsumer;
            _logger = logger;
            _ocrClient = ocrClient;
            _filesApi = filesApi;
        }

        public void Start() {
            _queueConsumer.OnReceived += async (sender, eventArgs) =>
            {
                var fileName = eventArgs.Content; // URL vom Consumer
                var messageId = eventArgs.MessageId;

                _logger.LogInformation($"QueueService with url {fileName}");

                var fileStream = await _filesApi.DownloadFromMinioAsync("documents", fileName);
                _logger.LogInformation($"Downloaded stream length: {fileStream.Length}");
                _logger.LogInformation($"File {fileName} successfully downloaded from MinIO.");

                var extractedText = await _ocrClient.OcrPdf(fileStream);
                _logger.LogInformation($"OCR completed for file: {fileName}");

                // Ergebnis verarbeiten oder speichern
                _queueProducer.Send(extractedText, messageId);
                _logger.LogInformation($"Extracted text {extractedText} sent to producer with MessageId: {messageId}");
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

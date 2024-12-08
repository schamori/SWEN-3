
using RabbitMq.QueueLibrary;

namespace TesseractOcr;

public interface IOcrClient
{
    Task<string> OcrPdf(byte[] pdfStream);
}

namespace TesseractOcr;

public interface IOcrClient
{
    string OcrPdf(Stream pdfStream);
}

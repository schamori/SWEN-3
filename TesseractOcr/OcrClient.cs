using System.Text;
using ImageMagick;
using Microsoft.Extensions.Options;
using RabbitMq.QueueLibrary;
using Tesseract;

namespace TesseractOcr;

public class OcrClient : IOcrClient
{
    private readonly string tessDataPath;
    private readonly string language;

    public OcrClient(OcrOptions options)
    {
        tessDataPath = options.TessDataPath;
        language = options.Language;

    }

    public async Task<string> OcrPdf(byte[] pdfStream)
    {
        var stringBuilder = new StringBuilder();

        using (var magickImages = new MagickImageCollection())
        {
            magickImages.Read(pdfStream);
            foreach (var magickImage in magickImages)
            {
                // Set the resolution and format of the image (adjust as needed)
                magickImage.Density = new Density(300, 300);
                //magickImage.ColorType = ColorType.Grayscale;
                magickImage.Format = MagickFormat.Png;

                // Perform OCR on the image
                using (var memoryStream = new MemoryStream())
                {
                    magickImage.Write(memoryStream);
                    memoryStream.Position = 0;

                    using (var pix = Pix.LoadFromMemory(memoryStream.ToArray()))
                    {
                        using (var tesseractEngine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
                        {
                            using (var page = tesseractEngine.Process(pix))
                            {
                                // Extrahiere den Text aus der Seite
                                var extractedText = page.GetText();
                                stringBuilder.Append(extractedText);
                            }
                        }
                    }
                }
            }
        }


        return stringBuilder.ToString();
    }
}

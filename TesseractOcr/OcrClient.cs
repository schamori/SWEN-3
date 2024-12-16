using System.Text;
using ImageMagick;
using Microsoft.Extensions.Options;
using RabbitMq.QueueLibrary;
using Tesseract;
using Microsoft.Extensions.Logging;

namespace TesseractOcr
{
    public class OcrClient : IOcrClient
    {
        private readonly string tessDataPath;
        private readonly string language;
        private readonly ILogger<OcrClient> _logger;

        public OcrClient(OcrOptions options, ILogger<OcrClient> logger)
        {
            tessDataPath = options.TessDataPath;
            language = options.Language;
            _logger = logger;
        }

        public async Task<string> OcrPdf(byte[] pdfStream)
        {
            var stringBuilder = new StringBuilder();

            try
            {
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
            }
            catch (MagickException magickEx)
            {
                _logger.LogError(magickEx, "Error while processing image with ImageMagick.");
                throw;
            }
            catch (TesseractException tessEx)
            {
                _logger.LogError(tessEx, "Error while ocr processing with Tesseract.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error while processing OCR.");
                throw;
            }

            return stringBuilder.ToString();
        }
    }
}

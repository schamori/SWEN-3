﻿using System.Text;
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

    public async Task<string> OcrPdf(Stream pdfStream)
    {
        var stringBuilder = new StringBuilder();

        using (var magickImages = new MagickImageCollection())
        {
            magickImages.Read(pdfStream);
            foreach (var magickImage in magickImages)
            {
                // Set the resolution and format of the image (adjust as needed)
                magickImage.Density = new Density(300, 300);
                magickImage.Format = MagickFormat.Png;

                // Perform OCR on the image
                using (var tesseractEngine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
                {
                    using (var page = tesseractEngine.Process(Pix.LoadFromMemory(magickImage.ToByteArray())))
                    {
                        var extractedText = page.GetText();
                        stringBuilder.Append(extractedText);
                    }
                }
            }
        }


        return stringBuilder.ToString();
    }
}

using System;

namespace TesseractOcr;

public class OcrOptions
{
    public const string OCR = "OCR";

    public string Language { get; set; } = "eng";
    public string TessDataPath { get; set; } = "/usr/share/tesseract-ocr/5/tessdata/";
}
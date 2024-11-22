﻿// See https://aka.ms/new-console-template for more information
using TesseractOcr;

Console.WriteLine("OCR with Tesseract Demo!");

string filePath = "./docs/HelloWorld.pdf";

try
{
    using FileStream fileStream = new FileStream(filePath, FileMode.Open);
    using StreamReader reader = new StreamReader(fileStream);
    OcrClient ocrClient = new OcrClient(new OcrOptions());

    var ocrContentText = ocrClient.OcrPdf(fileStream);
    Console.WriteLine( ocrContentText );
}
catch (IOException e)
{
    Console.WriteLine("An error occurred: " + e.Message);
}

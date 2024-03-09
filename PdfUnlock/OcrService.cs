// using Microsoft.Extensions.Logging;
// using System;
// using System.IO;
// using System.Text;
// using Tesseract;

// namespace PdfUnlock
// {
    // public class OcrService
    // {
        // private static ILogger _logger;

        // public OcrService(ILogger logger) : base()
        // {
            // _logger = logger;
        // }

        // public const string folderName = "C:/Temp/Images/";
        // public const string trainedDataFolderName = "tessdata";

        // public string DoOCR(OcrModel request)
        // {

            // //string name = request.Image.FileName;
            // //var image = request.Image;

            // //if (image.Length > 0)
            // //{
            // //    using (var fileStream = new FileStream(folderName + image.FileName, FileMode.Create))
            // //    {
            // //        image.CopyTo(fileStream);
            // //    }
            // //}

            // string tessPath = Path.Combine(trainedDataFolderName, "");
            // string result = "";

            // using (var engine = new TesseractEngine(tessPath, request.DestinationLanguage, EngineMode.Default))
            // {
                // string filePath = Path.Combine(folderName, request.FileName);
                // using (var img = Pix.LoadFromFile(filePath))
                // {
                    // var page = engine.Process(img);
                    // result = page.GetText();
                    // if (!String.IsNullOrWhiteSpace(result))
                        // _logger.LogDebug(result);
                    // else
                        // _logger.LogDebug("OCR finished, no text found.");
                // }
            // }

            // return result;
        // }

        // public void GetTextFromImage(params string[] imagePath)
        // {
            // var testImagePath = "./test.jpeg";
            // if (imagePath.Length > 0)
            // {
                // testImagePath = imagePath[0];
            // }

            // try
            // {
                // string filePath = Path.Combine(folderName, testImagePath);
                // using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                // {
                    // using (var img = Pix.LoadFromFile(testImagePath))
                    // {
                        // using (var page = engine.Process(img))
                        // {
                            // var text = page.GetText();

                            // if (String.IsNullOrWhiteSpace(text))
                                // _logger.LogDebug("No OCR text found.");

                            // _logger.LogDebug("Mean confidence: {0}", page.GetMeanConfidence());

                            // _logger.LogDebug("Text (GetText): \r\n{0}", text);
                            // _logger.LogDebug("Text (iterator):");

                            // StringBuilder stringBuilder = new();
                            // using (var iter = page.GetIterator())
                            // {
                                // iter.Begin();

                                // do
                                // {
                                    // do
                                    // {
                                        // do
                                        // {
                                            // do
                                            // {
                                                // if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                                // {
                                                    // _logger.LogDebug("<BLOCK>");
                                                // }

                                                // stringBuilder.Append(iter.GetText(PageIteratorLevel.Word));
                                                // stringBuilder.Append(" ");

                                                // if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                                // {
                                                    // stringBuilder.AppendLine();
                                                // }
                                            // } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                            // if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                            // {
                                                // stringBuilder.AppendLine();
                                            // }
                                        // } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                    // } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                                // } while (iter.Next(PageIteratorLevel.Block));
                            // }

                            // _logger.LogDebug(stringBuilder.ToString());
                        // }
                    // }
                // }
            // }
            // catch (Exception e)
            // {
                // _logger.LogError(e, "OCR failed.");
            // }
        // }

    // }

    // public class OcrModel
    // {
        // public string DestinationLanguage { get; set; }
        // //public IFormFile Image { get; set; }
        // public string FileName { get; set; }
    // }
// }
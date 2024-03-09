namespace PdfUnlock;

using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.IO.enums;
using PdfSharpCore.Pdf.Security;
using Microsoft.Extensions.Logging;
//using iTextSharp.text.pdf;
//iTextSharp.LGPLv2.Core
//PaddleOCR
//Tesseract

public static class PdfDocumentHandler
{
    public static bool IsFileSecured(this PdfDocument document)
    {
        var security = document.SecuritySettings;
        bool isSecured = security.DocumentSecurityLevel != PdfDocumentSecurityLevel.None;
        bool hasPermission = security.PermitPrint &&
            security.PermitExtractContent;
        return isSecured || !hasPermission;
    }

    public static bool IsFileSecured(string filePath, ILogger logger, string password = "")
    {
        bool isSecured = false;
        try
        {
            using var document = PdfReader.Open(filePath, password,
                PdfDocumentOpenMode.InformationOnly, PdfReadAccuracy.Moderate);
            isSecured = document.IsFileSecured();
            document.Close();
        }
        catch (PdfReaderException)
        {
            logger.LogDebug("PdfSharpCore reader failed to view file info for '{0}'.", filePath);
        }
        catch (Exception ex)
        {
            logger.LogInformation("PdfSharpCore reader failed. {0}", ex.Message);
            System.Diagnostics.Debugger.Break();
        }
        return isSecured;
    }

    public static bool? IsFileSecured(Stream inputStream, ILogger logger, string password = "")
    {
        bool? isSecured = null;
        try
        {
            inputStream.Position = 0;
            using var document = PdfReader.Open(inputStream, password,
                PdfDocumentOpenMode.InformationOnly, PdfReadAccuracy.Moderate);
            isSecured = document.IsFileSecured();
            document.Close();
        }
        catch (PdfReaderException)
        {
            logger.LogDebug("PdfSharpCore reader failed to view file info.");
        }
        catch (FormatException ex)
        {
            logger.LogDebug("FormatException in PdfSharpCore reader: {0}", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogDebug("InvalidOperationException in PdfSharpCore reader: {0}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogInformation("{0} in PdfSharpCore reader: {0}", ex.GetType().Name, ex.Message);
            System.Diagnostics.Debugger.Break();
        }
        return isSecured;
    }
}

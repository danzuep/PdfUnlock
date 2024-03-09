namespace PdfUnlock;

using Docnet.Core.Readers;

public static class PdfTextExtensions
{
    public static string GetPdfText(this MemoryStream pdfStream, string? password = null) =>
        pdfStream.ToArray().GetPdfText(password);

    internal static string GetPdfText(this byte[] bytes, string? password = null, string delimiter = "\r\n") =>
        string.Join(delimiter, bytes.GetText(password));

    internal static IEnumerable<string> GetText(this byte[] bytes, string? password = null)
    {
        IEnumerable<string> results = Array.Empty<string>();

        if (bytes?.Length > 0)
        {
            using var docReader = PdfHandler.GetDocReader(bytes, password);
            results = docReader.GetText();
        }

        return results;
    }

    internal static IEnumerable<string> GetText(this IDocReader docReader)
    {
        if (docReader != null)
        {
            for (int i = 0; i < docReader.GetPageCount(); i++)
            {
                using var pageReader = docReader.GetPageReader(i);
                yield return pageReader.GetText();
            }
        }
    }
}

namespace PdfUnlock;

using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using System.Runtime.Versioning;
using System.IO;

public class PdfHandler
{
    private readonly ILogger _logger = NullLogger.Instance;

    public PdfHandler(ILogger<PdfHandler>? logger = null) : base()
    {
        if (logger != null)
            _logger = logger;
    }

    public void GetUnlockedPdf(string? filePath, string? password, string? output = null)
    {
        _logger.LogDebug("FilePath: {0}; HasPassword: {1}; Output: {2}", filePath, !string.IsNullOrEmpty(password), output);

        var fileName = Path.GetFileName(filePath);
        try
        {
            if (!string.IsNullOrWhiteSpace(filePath) && filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        _logger.LogDebug("Checking security on '{0}'.", fileName);
                        string filePathOutput = string.IsNullOrEmpty(output) ?
                            GetOutputFileName(filePath, "", " (Unlocked)") : output;
                        var bytes = DocLib.Instance.Unlock(filePath, password);
                        File.WriteAllBytes(filePathOutput, bytes);
                        _logger.LogInformation("PDF contents have been written to '{0}'.", filePathOutput);
                    }
                    //using var memoryStream = new MemoryStream();
                    //using var inputStream = File.OpenRead(filePath);
                    //inputStream.CopyTo(memoryStream);
                    //if (GetUnlockedStream(memoryStream, out MemoryStream outputStream))
                    //{
                    //    _logger.LogDebug("Unlocked '{0}'.", fileName);
                    //    string filePathOutput = string.IsNullOrEmpty(output) ?
                    //        GetOutputFileName(fileName, "", " (Unlocked)") : output;
                    //    using var fileStream = File.Create(filePathOutput);
                    //    outputStream.WriteTo(fileStream);
                    //    _logger.LogInformation("PDF contents have been written to '{0}'.", filePathOutput);
                    //}
                }
                catch (Exception ex)
                {
                    if (ex is Docnet.Core.Exceptions.DocnetException && ex.Message.Contains("incorrect password"))
                    {
                        _logger.LogInformation("Attachment is password protected, '{0}'. {1}", fileName, ex.Message);
                    }
                }
            }
            else
                _logger.LogWarning("'{0}' is not in a valid PDF MimePart format.", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to unlock '{0}'.", fileName);
        }
    }

    internal static IDocReader GetDocReader(string filePath, string? password = null, int imageSize = 0)
    {
        var dimensions = imageSize > 0 ?
            new PageDimensions(imageSize, imageSize) : new PageDimensions();
        return string.IsNullOrEmpty(password) ?
            DocLib.Instance.GetDocReader(filePath, dimensions) :
            DocLib.Instance.GetDocReader(filePath, password, dimensions);
    }

    internal static IDocReader GetDocReader(byte[] bytes, string? password = null, int imageSize = 0)
    {
        var dimensions = imageSize > 0 ?
            new PageDimensions(imageSize, imageSize) : new PageDimensions();
        return string.IsNullOrEmpty(password) ?
            DocLib.Instance.GetDocReader(bytes, dimensions) :
            DocLib.Instance.GetDocReader(bytes, password, dimensions);
    }

    public MemoryStream Unlock(MemoryStream inputStream, string? password = null)
    {
        MemoryStream outputStream = new MemoryStream();
        if (inputStream != null)
        {
            inputStream.Position = 0;
            var bytes = DocLib.Instance.Unlock(inputStream.ToArray(), password);
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Position = 0;
        }
        return outputStream;
    }

    public bool GetUnlockedStream(MemoryStream inputStream, out MemoryStream outputStream)
    {
        // Check PDF doc info for access rights before unlocking
        bool? isSecured = PdfDocumentHandler.IsFileSecured(inputStream, _logger);
        bool unlockRequired = isSecured == null || isSecured.Value;
        outputStream = unlockRequired ? new MemoryStream() : inputStream;
        bool isUnlocked = false;
        if (unlockRequired)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            outputStream = Unlock(inputStream);
            isUnlocked = true;
            if (isSecured == null)
                _logger.LogDebug("File recovery with Docnet.Core was successful.");
        }
        return isUnlocked;
    }

    public string SaveUnlocked(string filePathInput, string? password = null)
    {
        string filePathOutput = "";
        if (File.Exists(filePathInput))
        {
            filePathOutput = GetOutputFileName(filePathInput, "Unlocked");
            var bytes = DocLib.Instance.Unlock(filePathInput, password);
            File.WriteAllBytes(filePathOutput, bytes);
        }
        return filePathOutput;
    }

    public string SaveFirstPage(string filePathInput)
    {
        string filePathOutput = "";
        if (File.Exists(filePathInput))
        {
            var bytes = DocLib.Instance.Split(filePathInput, 0, 0);
            filePathOutput = GetOutputFileName(filePathInput, "FirstPage");
            File.WriteAllBytes(filePathOutput, bytes);
        }
        return filePathOutput;
    }

    [SupportedOSPlatform("windows")]
    public string SavePdfThumbnail(string filePathInput, int imageSize = 1024)
    {
        string filePathOutput = "";
        if (File.Exists(filePathInput))
        {
            using var pageReader = GetDocReader(filePathInput, null, imageSize).GetPageReader(0);
            var bytes = PdfImageHandler.PdfToPng(pageReader);
            //string fileName = Path.GetFileName(filePathInput);
            filePathOutput = GetOutputFileName(filePathInput, "PdfThumbnails")
                .Replace(".pdf", ".png", StringComparison.OrdinalIgnoreCase);
            File.WriteAllBytes(filePathOutput, bytes);
        }
        return filePathOutput;
    }

    private const int MaxFilePathLength = 260;

    private string GetOutputFileName(string? filePath, string subFolder = "Temp", string suffix = "", bool create = true) //" (Copy)"
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
        var outputPath = filePath;
        var folder = Path.GetDirectoryName(filePath);
        //var fileName = Path.GetFileName(filePath);
        if (folder != null && (File.Exists(outputPath) || !string.IsNullOrEmpty(suffix)))
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            bool hasSubFolder = !string.IsNullOrEmpty(subFolder);
            var fileName = $"{name}{suffix}{extension}";
            if (hasSubFolder && !folder.EndsWith(subFolder, StringComparison.OrdinalIgnoreCase))
                folder = Path.Combine(folder, subFolder);
            if (create && hasSubFolder)
                Directory.CreateDirectory(folder);
            outputPath = Path.Combine(folder, fileName);
            if (File.Exists(outputPath))
                outputPath = Path.Combine(folder, $"{name}_{DateTime.Now.Ticks}{extension}");
            if (outputPath.Length > MaxFilePathLength)
                outputPath = Path.Combine(folder, $"{DateTime.Now.Ticks}{extension}");
            if (outputPath.Length > MaxFilePathLength)
                outputPath = Path.Combine(folder, $"{DateTime.Now.Millisecond}{extension}");
        }
        return outputPath;
    }

    private static string ConvertToFileExtensionFilter(string fileExtension)
    {
        var sb = new StringBuilder();
        if (string.IsNullOrEmpty(fileExtension))
            fileExtension = "*";
        else if (fileExtension.StartsWith("."))
            sb.Append("*");
        else if (!fileExtension.StartsWith("*."))
            sb.Append("*.");
        sb.Append(fileExtension);
        return sb.ToString();
    }

    private void CreateDirectory(string uncPath)
    {
        try
        {
            //var security = new DirectorySecurity("ReadFolder", AccessControlSections.Access);
            // If the directory already exists, this method does not create a new directory.
            Directory.CreateDirectory(uncPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "CreateDirectory access is denied, folder not created: '{0}'", uncPath);
        }
    }

    private bool CheckDirectory(string uncPath, bool createDirectory = false)
    {
        bool exists = Directory.Exists(uncPath);
        if (createDirectory)
            CreateDirectory(uncPath);
        else if (!exists)
            _logger.LogWarning("Folder not found: '{0}'.", uncPath);
        return exists;
    }

    private IEnumerable<string> EnumerateFilesFromFolder(string uncPath, string extension = "*", bool searchAll = false, bool createDirectory = false)
    {
        extension = ConvertToFileExtensionFilter(extension);
        var option = searchAll ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return CheckDirectory(uncPath, createDirectory) ? Directory.EnumerateFiles(uncPath, extension, option) : Array.Empty<string>();
    }

    public IEnumerable<string> GetFilesFromFolder(string uncFolderPath, string extension = "*.pdf", bool searchAll = false, bool createDirectory = false) =>
        EnumerateFilesFromFolder(uncFolderPath, extension, searchAll, createDirectory);

    public int GetPdfPageCount(string uncFilePath)
    {
        var files = GetFilesFromFolder(uncFilePath);
        int pageCount = GetTotalPageCount(files);
        _logger.LogDebug("{0} pages in {1} PDF files in '{2}'", pageCount, files.Count(), uncFilePath);
        return pageCount;
    }

    public int GetPdfWordCount(string uncFolderPath)
    {
        var files = GetFilesFromFolder(uncFolderPath);
        int wordCount = GetTotalWordCount(files);
        _logger.LogDebug("{0} words total in {1} PDF files in '{2}'", wordCount, files.Count(), uncFolderPath);
        return wordCount;
    }

    public int GetTotalPageCount(IEnumerable<string> files) =>
        files.Select(f => GetPageCount(f)).Sum();

    public int GetPageCount(string filePath, string password = "")
    {
        int pageCount = -1;
        try
        {
            using var docReader = GetDocReader(filePath, password);
            pageCount = docReader.GetPageCount();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get page count for '{0}'.", filePath);
        }
        return pageCount;
    }

    public int GetTotalWordCount(IEnumerable<string> files) =>
        files.Select(f => GetWordCount(f)).Sum();

    public int GetWordCount(string uncFilePath, string? password = null)
    {
        int wordCount = 0;

        using var docReader = GetDocReader(uncFilePath, password);
        var pages = docReader.GetText();

        foreach (var text in pages)
            wordCount += text.Length;

        _logger.LogDebug("'{0}' word count is {1}.", uncFilePath, wordCount);

        if (wordCount < 100)
            _logger.LogDebug("Suspected handwritten invoice found.");

        return wordCount;
    }

    public string ReadAllText(string filePath, string? password = null)
    {
        string pdfText = "";

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var allText = new StringBuilder();
            using var docReader = GetDocReader(filePath, password);
            var pages = docReader.GetText();

            foreach (var text in pages)
            {
                allText.Append(text);
                //allText.AppendJoin(", ", text);
                _logger.LogDebug(text);
            }

            pdfText = allText.ToString();
        }

        return pdfText;
    }
}

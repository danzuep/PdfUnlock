namespace PdfUnlock;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Editors;
using Docnet.Core.Models;
using Docnet.Core.Readers;

//https://github.com/GowenGit/docnet/blob/master/examples/nuget-usage/NugetUsageX86/PdfToImageExamples.cs
public static class PdfImageHandler
{
    public static void JpegToPdf(string filePathInput, string filePathOutput)
    {
        if (filePathOutput.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
            (filePathInput.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            filePathInput.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
        {
            var file = new JpegImage
            {
                Bytes = File.ReadAllBytes(filePathInput),
                Width = 1024,
                Height = 1024
            };
            var bytes = DocLib.Instance.JpegToPdf([file]);
            File.WriteAllBytes(filePathOutput, bytes);
        }
    }

    [SupportedOSPlatform("windows")]
    public static byte[] PdfToPng(IPageReader pageReader)
    {
        byte[] result = Array.Empty<byte>();

        if (pageReader != null)
        {
            byte[] rawBytes = pageReader.GetImage();
            int width = pageReader.GetPageWidth();
            int height = pageReader.GetPageHeight();

            using (var thumbnailBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                var rect = new Rectangle(0, 0, thumbnailBitmap.Width, thumbnailBitmap.Height);

                var bitmapData = thumbnailBitmap.LockBits(rect, ImageLockMode.WriteOnly, thumbnailBitmap.PixelFormat);
                var nativePointer = bitmapData.Scan0;

                Marshal.Copy(rawBytes, 0, nativePointer, rawBytes.Length);
                thumbnailBitmap.UnlockBits(bitmapData);

                using (var stream = new MemoryStream())
                {
                    thumbnailBitmap.Save(stream, ImageFormat.Png);
                    result = stream.ToArray();
                }
            }
        }

        return result;
    }

    [SupportedOSPlatform("windows")]
    public static void ConvertPageToSimpleImageWithLetterOutlines(
        string filePathInput, string filePathOutput)
    {
        using var docReader = DocLib.Instance.GetDocReader(
            filePathInput, new PageDimensions(1080, 1920));

        using var pageReader = docReader.GetPageReader(0);

        var rawBytes = pageReader.GetImage();

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        var characters = pageReader.GetCharacters();

        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        AddBytes(bmp, rawBytes);
        DrawRectangles(bmp, characters);

        using var stream = new MemoryStream();

        bmp.Save(stream, ImageFormat.Png);

        File.WriteAllBytes(filePathOutput, stream.ToArray());
    }

    [SupportedOSPlatform("windows")]
    public static void ConvertPageToSimpleImageWithLetterOutlinesUsingScaling(
        string filePathInput, string filePathOutput)
    {
        using var docReader = DocLib.Instance.GetDocReader(
            filePathInput, new PageDimensions(1080, 1920));

        using var pageReader = docReader.GetPageReader(0);

        var rawBytes = pageReader.GetImage();

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        var characters = pageReader.GetCharacters();

        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        AddBytes(bmp, rawBytes);
        DrawRectangles(bmp, characters);

        using var stream = new MemoryStream();

        bmp.Save(stream, ImageFormat.Png);

        File.WriteAllBytes(filePathOutput, stream.ToArray());
    }

    [SupportedOSPlatform("windows")]
    public static void ConvertPageToSimpleImageWithoutTransparency(
        string filePathInput, string filePathOutput)
    {
        using var docReader = DocLib.Instance.GetDocReader(
            filePathInput, new PageDimensions(1080, 1920));

        using var pageReader = docReader.GetPageReader(0);

        var rawBytes = pageReader.GetImage(new NaiveTransparencyRemover(120, 120, 0));

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        var characters = pageReader.GetCharacters();

        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        AddBytes(bmp, rawBytes);
        DrawRectangles(bmp, characters);

        using var stream = new MemoryStream();

        bmp.Save(stream, ImageFormat.Png);

        File.WriteAllBytes(filePathOutput, stream.ToArray());
    }

    [SupportedOSPlatform("windows")]
    public static void ConvertPageToGreyscaleImageIncludeAnnotations(
        string filePathInput, string filePathOutput)
    {
        using var docReader = DocLib.Instance.GetDocReader(
            filePathInput, new PageDimensions(1080, 1920));

        using var pageReader = docReader.GetPageReader(0);

        var rawBytes = pageReader.GetImage(RenderFlags.RenderAnnotations | RenderFlags.Grayscale);

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        AddBytes(bmp, rawBytes);

        using var stream = new MemoryStream();

        bmp.Save(stream, ImageFormat.Png);

        File.WriteAllBytes(filePathOutput, stream.ToArray());
    }

    [SupportedOSPlatform("windows")]
    private static void AddBytes(Bitmap bmp, byte[] rawBytes)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

        var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
        var pNative = bmpData.Scan0;

        Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
        bmp.UnlockBits(bmpData);
    }

    [SupportedOSPlatform("windows")]
    private static void DrawRectangles(Bitmap bmp, IEnumerable<Character> characters)
    {
        var pen = new Pen(Color.Red);

        using var graphics = Graphics.FromImage(bmp);

        foreach (var c in characters)
        {
            var rect = new Rectangle(c.Box.Left, c.Box.Top, c.Box.Right - c.Box.Left, c.Box.Bottom - c.Box.Top);
            graphics.DrawRectangle(pen, rect);
        }
    }
}

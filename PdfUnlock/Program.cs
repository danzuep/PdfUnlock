namespace PdfUnlock;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

public sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(o =>
            o.SetMinimumLevel(LogLevel.Debug).AddDebug().AddConsole());

        //var app = InitialiseOptions<PdfConfig>(args);
        var config = InitialiseConfig(args).GetSection("Pdf");

        var pdfHandler = new PdfHandler(loggerFactory.CreateLogger<PdfHandler>());
        pdfHandler.GetUnlockedPdf(config["File"], config["Password"], config["Output"]);

        if (!string.IsNullOrWhiteSpace(config["Wait"]))
        {
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("Closing in 3 seconds, use the -w option to wait for a key press instead.");
            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        return 0;
    }

    //public record PdfConfig(string File, string Password, string Output, int WaitSeconds = 3);

    //[RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Get<T>()")]
    //[RequiresDynamicCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Get<T>()")]
    //private static T? InitialiseOptions<T>(string[] args) where T : new()
    //{
    //    var config = InitialiseConfig(args);
    //    return config.Get<T>();
    //}

    private static IConfigurationRoot InitialiseConfig(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            // The file to read and unlock.
            { "--file", "Pdf:File" },
            { "-f", "Pdf:File" },
            // The password to unlock it with.
            { "--password", "Pdf:Password" },
            { "-p", "Pdf:Password" },
            // The filename to write to.
            { "--output", "Pdf:Output" },
            { "-o", "Pdf:Output" },
            // Delay closing the terminal window.
            { "--wait", "Pdf:Wait" },
            { "-w", "Pdf:Wait" },
        };
        var builder = new ConfigurationBuilder();
        builder.AddCommandLine(args, switchMappings);
        return builder.Build();
    }
}
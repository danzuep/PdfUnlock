using Microsoft.Extensions.Logging;

namespace PdfUnlock.Wpf.Services
{
    public sealed class PdfUnlockService : IPdfUnlockService
    {
        private readonly PdfHandler _pdfHandler;

        public PdfUnlockService(ILogger<PdfHandler> logger)
        {
            _pdfHandler = new PdfHandler(logger);
        }

        public Task UnlockAsync(UnlockRequest request, CancellationToken cancellationToken = default)
        {
            _pdfHandler.GetUnlockedPdf(request.FilePath, request.Password, request.OutputPath);

            return Task.CompletedTask;
        }
    }
}
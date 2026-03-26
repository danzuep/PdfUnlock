namespace PdfUnlock.Wpf.Services
{
    public interface IPdfUnlockService
    {
        Task UnlockAsync(UnlockRequest request, CancellationToken cancellationToken = default);
    }
}
namespace PdfUnlock
{
    public sealed class UnlockRequest
    {
        public string? FilePath { get; set; }
        public string? Password { get; set; }
        public string? OutputPath { get; set; }
        public bool WaitForClose { get; set; }
    }
}
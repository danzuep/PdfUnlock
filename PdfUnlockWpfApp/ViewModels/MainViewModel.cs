using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfUnlock.Wpf.Services;

namespace PdfUnlock.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IPdfUnlockService _pdfUnlockService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string? filePath;

    [ObservableProperty]
    private string? password;

    [ObservableProperty]
    private string? outputPath;

    [ObservableProperty]
    private bool waitForClose;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Ready.";

    public MainViewModel(IPdfUnlockService pdfUnlockService, IDialogService dialogService)
    {
        _pdfUnlockService = pdfUnlockService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private void BrowseInput()
    {
        var selected = _dialogService.OpenPdfFile();
        if (!string.IsNullOrWhiteSpace(selected))
            FilePath = selected;
    }

    [RelayCommand]
    private void BrowseOutput()
    {
        var selected = _dialogService.SavePdfFile();
        if (!string.IsNullOrWhiteSpace(selected))
            OutputPath = selected;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task UnlockAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Unlocking PDF...";

            var request = new UnlockRequest
            {
                FilePath = FilePath,
                Password = Password,
                OutputPath = OutputPath,
                WaitForClose = WaitForClose
            };

            await _pdfUnlockService.UnlockAsync(request);

            StatusMessage = "Unlock completed successfully.";
            _dialogService.ShowMessage("Success", "PDF unlocked successfully.");
        }
        catch (Exception ex)
        {
            StatusMessage = "Unlock failed.";
            _dialogService.ShowError("Error", ex.Message);
        }
        finally
        {
            IsBusy = false;

            if (WaitForClose)
            {
                MessageBox.Show(
                    "Press OK to continue.",
                    "Wait",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
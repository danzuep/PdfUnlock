using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PdfUnlock.Wpf.Services;
using PdfUnlock.Wpf.ViewModels;

namespace PdfUnlockWpfApp
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddDebug();
                builder.AddConsole();
            });

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IPdfUnlockService, PdfUnlockService>();
            services.AddSingleton<MainViewModel>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };

            MainWindow = window;
            window.Show();
        }
    }
}
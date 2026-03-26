using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using PdfUnlock.Wpf.ViewModels;

namespace PdfUnlock.Wpf.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        RevealPasswordToggle.Checked += RevealPasswordToggle_OnChecked;
        RevealPasswordToggle.Unchecked += RevealPasswordToggle_OnUnchecked;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
            if (PasswordRevealBox.Visibility == Visibility.Visible)
                PasswordRevealBox.Text = box.Password;
        }
    }

    private void RevealPasswordToggle_OnChecked(object sender, RoutedEventArgs e)
    {
        PasswordRevealBox.Visibility = Visibility.Visible;
        PasswordBox.Visibility = Visibility.Collapsed;
        PasswordRevealBox.Text = PasswordBox.Password;
    }

    private void RevealPasswordToggle_OnUnchecked(object sender, RoutedEventArgs e)
    {
        PasswordRevealBox.Visibility = Visibility.Collapsed;
        PasswordBox.Visibility = Visibility.Visible;
        PasswordBox.Password = PasswordRevealBox.Text;
    }
}
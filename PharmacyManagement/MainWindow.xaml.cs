using System.Windows;
using PharmacyManagement.ViewModels;

namespace PharmacyManagement;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}

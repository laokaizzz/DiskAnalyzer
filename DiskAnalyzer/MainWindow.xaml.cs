using System.Windows;
using DiskAnalyzer.ViewModels;

namespace DiskAnalyzer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}

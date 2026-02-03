using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DiskAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            var viewModel = mainWindow.DataContext as ViewModels.MainViewModel;

            if (viewModel != null && e.Args.Length > 0)
            {
                // Use the first argument as the path
                viewModel.SetInitialPath(e.Args[0]);
            }

            mainWindow.Show();
        }
    }
}

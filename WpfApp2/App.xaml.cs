using System.Windows;
using WpfApp2.Services;
using WpfApp2.ViewModels;
using WpfApp2.Views; // Добавлено!

namespace WpfApp2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Внедрение зависимостей
            IDialogService dialogService = new DialogService();
            IDataService dataService = new FileDataService();

            var mainViewModel = new MainViewModel(dataService, dialogService);

            // Создаем окно из папки Views
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Closing += (s, args) => mainViewModel.Dispose();

            mainWindow.Show();
        }
    }
}
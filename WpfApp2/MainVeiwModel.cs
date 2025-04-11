using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class MainViewModel
    {
        // Свойства для транзакций
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<string> Categories { get; set; }
        public string SelectedCategory { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; }

        // Команда для добавления транзакции
        public ICommand AddTransactionCommand { get; }

        // Данные для графика
        public SeriesCollection SpendingByCategory { get; set; }
        public List<string> CategoryLabels { get; set; }

        public MainViewModel()
        {
            // Инициализация списка транзакций и категорий
            Transactions = new ObservableCollection<Transaction>();
            Categories = new ObservableCollection<string> { "Еда", "Транспорт", "Развлечения", "Зарплата" };

            // Инициализация команды
            AddTransactionCommand = new RelayCommand(AddTransaction);

            // Инициализация данных для графика
            SpendingByCategory = new SeriesCollection();
            CategoryLabels = new List<string> { "Еда", "Транспорт", "Развлечения" };

            // Пример данных для графика
            SpendingByCategory.Add(new ColumnSeries
            {
                Title = "Расходы",
                Values = new ChartValues<decimal> { 500, 300, 200 }
            });
        }

        private void AddTransaction()
        {
            var transaction = new Transaction
            {
                Category = SelectedCategory,
                Amount = Amount,
                Date = Date,
                Description = Description
            };
            Transactions.Add(transaction);

            // Обновление графика
            UpdateChart();
        }

        private void UpdateChart()
        {
            // Логика обновления графика на основе новых данных
        }
    }

    public class Transaction
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

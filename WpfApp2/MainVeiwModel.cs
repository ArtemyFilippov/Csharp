using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.IO; 
using Microsoft.Win32; 

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
        //Работа с бд (txt файл)
        public ICommand SaveToFileCommand { get; }
        public ICommand LoadFromFileCommand { get; }

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

            SaveToFileCommand = new RelayCommand(SaveToFile);
            LoadFromFileCommand = new RelayCommand(LoadFromFile);

            // Пример данных для графика
            SpendingByCategory.Add(new ColumnSeries
            {
                Title = "Расходы",
                Values = new ChartValues<decimal> { 0, 0, 0 }
            });
        }

        private void SaveToFile()
        {
            try
            {
                string filePath = "transactions.txt";
                var lines = Transactions.Select(t => $"{t.Category}|{t.Amount}|{t.Date}|{t.Description}");
                File.WriteAllLines(filePath, lines);
                MessageBox.Show("Данные сохранены в файл!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void LoadFromFile()
        {
            try
            {
                string filePath = "transactions.txt";
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден!");
                    return;
                }

                var lines = File.ReadAllLines(filePath);
                Transactions.Clear();

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 4)
                    {
                        Transactions.Add(new Transaction
                        {
                            Category = parts[0],
                            Amount = decimal.Parse(parts[1]),
                            Date = DateTime.Parse(parts[2]),
                            Description = parts[3]
                        });
                    }
                }

                MessageBox.Show("Данные загружены из файла!");
                UpdateChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
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

            UpdateChart();
        }

        private void UpdateChart()
        {
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

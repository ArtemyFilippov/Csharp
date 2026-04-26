using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfApp2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private const string FilePath = "transactions.txt";

        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<string> Categories { get; set; }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction { get => _selectedTransaction; set { _selectedTransaction = value; OnPropertyChanged(); } }

        private string _selectedCategory;
        public string SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        private decimal _amount;
        public decimal Amount { get => _amount; set { _amount = value; OnPropertyChanged(); } }

        private DateTime _date = DateTime.Now;
        public DateTime Date { get => _date; set { _date = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditing)); } }
        public bool IsNotEditing => !IsEditing;
        public string FormButtonText => IsEditing ? "Сохранить" : "Добавить";

        private decimal _totalIncome;
        public decimal TotalIncome { get => _totalIncome; set { _totalIncome = value; OnPropertyChanged(); } }

        private decimal _totalExpense;
        public decimal TotalExpense { get => _totalExpense; set { _totalExpense = value; OnPropertyChanged(); } }

        private decimal _totalBalance;
        public decimal TotalBalance { get => _totalBalance; set { _totalBalance = value; OnPropertyChanged(); } }

        public ICommand AddTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand CancelEditCommand { get; }

        public SeriesCollection PieSeries { get; set; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<Transaction>();
            // ИЗМЕНЕНИЕ 1: Замена слова в списке категорий
            Categories = new ObservableCollection<string> { "Еда", "Транспорт", "Развлечения", "Жильё", "Пополнение" };

            AddTransactionCommand = new RelayCommand(AddOrUpdateTransaction);
            EditTransactionCommand = new RelayCommand(EditTransaction, () => SelectedTransaction != null);
            DeleteTransactionCommand = new RelayCommand(DeleteTransaction, () => SelectedTransaction != null);
            CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditing);

            PieSeries = new SeriesCollection();
            LoadData();
        }

        private void LoadData()
        {
            if (!File.Exists(FilePath)) { AddSampleData(); SaveData(); return; }
            try
            {
                var lines = File.ReadAllLines(FilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { '|' }, 4);

                    if (parts.Length == 4 && DateTime.TryParse(parts[0], out DateTime date) && decimal.TryParse(parts[2], out decimal amount))
                        Transactions.Add(new Transaction { Date = date, Category = parts[1], Amount = amount, Description = parts[3] });
                }
            }
            catch { MessageBox.Show("Ошибка чтения файла.", "Внимание"); }
            UpdateChartDataAndSummary();
        }

        private void SaveData()
        {
            try { using (StreamWriter writer = new StreamWriter(FilePath)) { foreach (var t in Transactions) writer.WriteLine($"{t.Date:yyyy-MM-dd}|{t.Category}|{t.Amount}|{t.Description}"); } }
            catch { MessageBox.Show("Ошибка сохранения файла.", "Ошибка"); }
        }

        private void AddOrUpdateTransaction()
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory) || Amount <= 0) return;
            if (IsEditing && SelectedTransaction != null) { SelectedTransaction.Category = SelectedCategory; SelectedTransaction.Amount = Amount; SelectedTransaction.Date = Date; SelectedTransaction.Description = Description; }
            else { Transactions.Add(new Transaction { Category = SelectedCategory, Amount = Amount, Date = Date, Description = Description }); }
            ResetForm(); UpdateChartDataAndSummary(); SaveData();
        }

        private void EditTransaction() { if (SelectedTransaction == null) return; IsEditing = true; SelectedCategory = SelectedTransaction.Category; Amount = SelectedTransaction.Amount; Date = SelectedTransaction.Date; Description = SelectedTransaction.Description; }

        private void DeleteTransaction()
        {
            if (SelectedTransaction == null) return;
            if (MessageBox.Show($"Удалить транзакцию?\n{SelectedTransaction.Category} | {SelectedTransaction.Amount:N0} ₽", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            { Transactions.Remove(SelectedTransaction); SelectedTransaction = null; if (IsEditing) ResetForm(); UpdateChartDataAndSummary(); SaveData(); }
        }

        private void CancelEdit() => ResetForm();
        private void ResetForm() { IsEditing = false; SelectedCategory = null; Amount = 0; Date = DateTime.Now; Description = string.Empty; }

        private void AddSampleData()
        {
            // ИЗМЕНЕНИЕ 2: Демо-данные
            Transactions.Add(new Transaction { Category = "Еда", Amount = 5200, Date = DateTime.Today.AddDays(-5), Description = "Продукты" });
            Transactions.Add(new Transaction { Category = "Транспорт", Amount = 2400, Date = DateTime.Today.AddDays(-4), Description = "Метро" });
            Transactions.Add(new Transaction { Category = "Жильё", Amount = 15000, Date = DateTime.Today.AddDays(-6), Description = "Аренда" });
            Transactions.Add(new Transaction { Category = "Пополнение", Amount = 80000, Date = DateTime.Today.AddDays(-7), Description = "Аванс" });
        }

        private void UpdateChartDataAndSummary() { UpdateChart(); UpdateSummary(); }

        private void UpdateChart()
        {
            PieSeries.Clear();
            // ИЗМЕНЕНИЕ 3: Исключение из графика расходов
            foreach (var category in Categories.Where(c => c != "Пополнение"))
            {
                var total = Transactions.Where(t => t.Category == category).Sum(t => t.Amount);
                if (total > 0) PieSeries.Add(new PieSeries { Title = category, Values = new ChartValues<decimal> { total }, DataLabels = true, LabelPoint = chartPoint => string.Format("{0:N0} ₽ ({1:P})", chartPoint.Y, chartPoint.Participation), PushOut = 4 });
            }
        }

        // ИЗМЕНЕНИЕ 4: Подсчет доходов и расходов
        private void UpdateSummary()
        {
            TotalIncome = Transactions.Where(t => t.Category == "Пополнение").Sum(t => t.Amount);
            TotalExpense = Transactions.Where(t => t.Category != "Пополнение").Sum(t => t.Amount);
            TotalBalance = TotalIncome - TotalExpense;
        }
    }

    public class Transaction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private string _category; public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }
        private decimal _amount; public decimal Amount { get => _amount; set { _amount = value; OnPropertyChanged(); } }
        private DateTime _date; public DateTime Date { get => _date; set { _date = value; OnPropertyChanged(); } }
        private string _description; public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute; private readonly Func<bool> _canExecute;
        public RelayCommand(Action execute, Func<bool> canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility v && v == Visibility.Visible;
    }

    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ИЗМЕНЕНИЕ 5: Цвет суммы (зеленый для Пополнения)
            if (value is string category) return category == "Пополнение" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3BE8C")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A"));
            return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { if (value is decimal balance) { if (balance > 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3BE8C")); if (balance < 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A")); } return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C566A")); }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class AmountValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) { if (value is string s && decimal.TryParse(s, out decimal amount)) { if (amount < 0) return new ValidationResult(false, "Сумма не может быть отрицательной"); if (amount == 0) return new ValidationResult(false, "Введите сумму больше 0"); return ValidationResult.ValidResult; } return new ValidationResult(false, "Введите корректное число"); }
    }
}
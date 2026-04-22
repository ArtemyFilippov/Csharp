using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LiveCharts;

namespace WpfApp2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ========== Транзакции ==========
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<string> Categories { get; set; }

        // Выбранная транзакция в DataGrid
        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set { _selectedTransaction = value; OnPropertyChanged(); }
        }

        // Поля формы
        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // Режим редактирования
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditing)); }
        }
        public bool IsNotEditing => !IsEditing;

        // Текст кнопки
        public string FormButtonText => IsEditing ? "Сохранить" : "Добавить";

        // ========== Команды ==========
        public ICommand AddTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand CancelEditCommand { get; }

        // ========== График ==========
        public ChartValues<decimal> SpendingValues { get; set; }
        public List<string> CategoryLabels { get; set; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<Transaction>();
            Categories = new ObservableCollection<string>
            {
                "Еда", "Транспорт", "Развлечения", "Жильё", "Зарплата"
            };

            AddTransactionCommand = new RelayCommand(AddOrUpdateTransaction);
            EditTransactionCommand = new RelayCommand(EditTransaction, () => SelectedTransaction != null);
            DeleteTransactionCommand = new RelayCommand(DeleteTransaction, () => SelectedTransaction != null);
            CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditing);

            CategoryLabels = Categories.Where(c => c != "Зарплата").ToList();
            SpendingValues = new ChartValues<decimal>(new decimal[CategoryLabels.Count]);

            AddSampleData();
        }

        // ========== Добавление / Обновление ==========
        private void AddOrUpdateTransaction()
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory) || Amount <= 0)
                return;

            if (IsEditing && SelectedTransaction != null)
            {
                // Обновляем существующую
                SelectedTransaction.Category = SelectedCategory;
                SelectedTransaction.Amount = Amount;
                SelectedTransaction.Date = Date;
                SelectedTransaction.Description = Description;
            }
            else
            {
                // Создаём новую
                var transaction = new Transaction
                {
                    Category = SelectedCategory,
                    Amount = Amount,
                    Date = Date,
                    Description = Description
                };
                Transactions.Add(transaction);
            }

            ResetForm();
            UpdateChart();
        }

        // ========== Редактирование ==========
        private void EditTransaction()
        {
            if (SelectedTransaction == null) return;

            IsEditing = true;
            SelectedCategory = SelectedTransaction.Category;
            Amount = SelectedTransaction.Amount;
            Date = SelectedTransaction.Date;
            Description = SelectedTransaction.Description;
        }

        // ========== Удаление с подтверждением ==========
        private void DeleteTransaction()
        {
            if (SelectedTransaction == null) return;

            var result = MessageBox.Show(
                $"Удалить транзакцию?\n\n" +
                $"{SelectedTransaction.Date:d}  |  {SelectedTransaction.Category}  |  {SelectedTransaction.Amount:N0} ₽\n" +
                $"{SelectedTransaction.Description}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Transactions.Remove(SelectedTransaction);
                SelectedTransaction = null;

                if (IsEditing) ResetForm();

                UpdateChart();
            }
        }

        // ========== Отмена редактирования ==========
        private void CancelEdit()
        {
            ResetForm();
        }

        // ========== Сброс формы ==========
        private void ResetForm()
        {
            IsEditing = false;
            SelectedCategory = null;
            Amount = 0;
            Date = DateTime.Now;
            Description = string.Empty;
        }

        // ========== Данные графика ==========
        private void AddSampleData()
        {
            Transactions.Add(new Transaction { Category = "Еда", Amount = 5200, Date = DateTime.Today.AddDays(-5), Description = "Продукты на неделю" });
            Transactions.Add(new Transaction { Category = "Еда", Amount = 1800, Date = DateTime.Today.AddDays(-3), Description = "Ресторан" });
            Transactions.Add(new Transaction { Category = "Транспорт", Amount = 2400, Date = DateTime.Today.AddDays(-4), Description = "Метро и такси" });
            Transactions.Add(new Transaction { Category = "Транспорт", Amount = 800, Date = DateTime.Today.AddDays(-1), Description = "Бензин" });
            Transactions.Add(new Transaction { Category = "Развлечения", Amount = 3500, Date = DateTime.Today.AddDays(-2), Description = "Кино и концерты" });
            Transactions.Add(new Transaction { Category = "Жильё", Amount = 15000, Date = DateTime.Today.AddDays(-6), Description = "Аренда" });
            Transactions.Add(new Transaction { Category = "Зарплата", Amount = 80000, Date = DateTime.Today.AddDays(-7), Description = "Зарплата" });

            UpdateChart();
        }

        private void UpdateChart()
        {
            SpendingValues.Clear();
            foreach (var category in CategoryLabels)
            {
                var total = Transactions
                    .Where(t => t.Category == category)
                    .Sum(t => t.Amount);
                SpendingValues.Add(total);
            }
        }
    }

    // ========== Модель ==========
    public class Transaction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string _category;
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
    }

    // ========== RelayCommand ==========
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
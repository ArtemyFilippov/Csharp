using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.Services;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfApp2.ViewModels
{
    public class MainViewModel : IDisposable
    {
        private const string FilePath = "transactions.txt";

        private readonly IDialogService _dialogService;
        private readonly IDataService _dataService;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction { get => _selectedTransaction; set { _selectedTransaction = value; OnPropertyChanged(); } }

        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

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

        // Внедрение зависимостей (Dependency Inversion)
        public MainViewModel(IDataService dataService, IDialogService dialogService)
        {
            _dataService = dataService;
            _dialogService = dialogService;

            Transactions = new ObservableCollection<Transaction>();

            // Избавление от хардкода строк - используем Модели
            Categories = new ObservableCollection<Category>
            {
                new Category { Name = "Еда", Type = CategoryType.Expense },
                new Category { Name = "Транспорт", Type = CategoryType.Expense },
                new Category { Name = "Развлечения", Type = CategoryType.Expense },
                new Category { Name = "Жильё", Type = CategoryType.Expense },
                new Category { Name = "Пополнение", Type = CategoryType.Income }
            };

            AddTransactionCommand = new RelayCommand(AddOrUpdateTransaction);
            EditTransactionCommand = new RelayCommand(EditTransaction, () => SelectedTransaction != null);
            DeleteTransactionCommand = new RelayCommand(DeleteTransaction, () => SelectedTransaction != null);
            CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditing);

            PieSeries = new SeriesCollection();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var loadedTransactions = _dataService.LoadData(FilePath);
                foreach (var t in loadedTransactions)
                    Transactions.Add(t);

                if (Transactions.Count == 0) AddSampleData();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Не удалось загрузить данные: {ex.Message}", "Ошибка");
            }
            finally
            {
                UpdateChartDataAndSummary();
                SaveData(); // Сохраняем, если файла не было (демо-данные)
            }
        }

        private void SaveData()
        {
            try
            {
                _dataService.SaveData(FilePath, Transactions);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Ошибка сохранения");
            }
        }

        private void AddOrUpdateTransaction()
        {
            if (SelectedCategory == null || Amount <= 0)
            {
                _dialogService.ShowMessage("Пожалуйста, выберите категорию и введите сумму больше нуля!", "Ошибка ввода");
                return;
            }

            if (IsEditing && SelectedTransaction != null)
            {
                SelectedTransaction.Category = SelectedCategory.Name;
                SelectedTransaction.Type = SelectedCategory.Type;
                SelectedTransaction.Amount = Amount;
                SelectedTransaction.Date = Date;
                SelectedTransaction.Description = Description;
            }
            else
            {
                Transactions.Add(new Transaction
                {
                    Category = SelectedCategory.Name,
                    Type = SelectedCategory.Type,
                    Amount = Amount,
                    Date = Date,
                    Description = Description
                });
            }
            ResetForm();
            UpdateChartDataAndSummary();
            SaveData();
        }

        private void EditTransaction()
        {
            if (SelectedTransaction == null) return;
            IsEditing = true;
            SelectedCategory = Categories.FirstOrDefault(c => c.Name == SelectedTransaction.Category);
            Amount = SelectedTransaction.Amount;
            Date = SelectedTransaction.Date;
            Description = SelectedTransaction.Description;
        }

        private void DeleteTransaction()
        {
            if (SelectedTransaction == null) return;
            if (_dialogService.ShowConfirmation($"Удалить транзакцию?\n{SelectedTransaction.Category} | {SelectedTransaction.Amount:N0} ₽", "Удаление"))
            {
                Transactions.Remove(SelectedTransaction);
                SelectedTransaction = null;
                if (IsEditing) ResetForm();
                UpdateChartDataAndSummary();
                SaveData();
            }
        }

        private void CancelEdit() => ResetForm();
        private void ResetForm() { IsEditing = false; SelectedCategory = null; Amount = 0; Date = DateTime.Now; Description = string.Empty; }

        private void AddSampleData()
        {
            Transactions.Add(new Transaction { Category = "Еда", Type = CategoryType.Expense, Amount = 5200, Date = DateTime.Today.AddDays(-5), Description = "Продукты" });
            Transactions.Add(new Transaction { Category = "Транспорт", Type = CategoryType.Expense, Amount = 2400, Date = DateTime.Today.AddDays(-4), Description = "Метро" });
            Transactions.Add(new Transaction { Category = "Жильё", Type = CategoryType.Expense, Amount = 15000, Date = DateTime.Today.AddDays(-6), Description = "Аренда" });
            Transactions.Add(new Transaction { Category = "Пополнение", Type = CategoryType.Income, Amount = 80000, Date = DateTime.Today.AddDays(-7), Description = "Аванс" });
        }

        private void UpdateChartDataAndSummary() { UpdateChart(); UpdateSummary(); }

        private void UpdateChart()
        {
            PieSeries.Clear();
            // Используем Type вместо хардкода строки
            foreach (var category in Categories.Where(c => c.Type == CategoryType.Expense))
            {
                var total = Transactions.Where(t => t.Category == category.Name).Sum(t => t.Amount);
                if (total > 0) PieSeries.Add(new PieSeries { Title = category.Name, Values = new ChartValues<decimal> { total }, DataLabels = true, LabelPoint = chartPoint => string.Format("{0:N0} ₽ ({1:P})", chartPoint.Y, chartPoint.Participation), PushOut = 4 });
            }
        }

        private void UpdateSummary()
        {
            TotalIncome = Transactions.Where(t => t.Type == CategoryType.Income).Sum(t => t.Amount);
            TotalExpense = Transactions.Where(t => t.Type == CategoryType.Expense).Sum(t => t.Amount);
            TotalBalance = TotalIncome - TotalExpense;
        }

        public void Dispose()
        {
            // При закрытии приложения сохраняем данные
            SaveData();
        }
    }
}
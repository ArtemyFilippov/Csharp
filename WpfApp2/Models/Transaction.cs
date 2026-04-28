using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp2.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _category;
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }

        private CategoryType _type;
        public CategoryType Type { get => _type; set { _type = value; OnPropertyChanged(); } }

        private decimal _amount;
        public decimal Amount { get => _amount; set { _amount = value; OnPropertyChanged(); } }

        private System.DateTime _date;
        public System.DateTime Date { get => _date; set { _date = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
    }
}
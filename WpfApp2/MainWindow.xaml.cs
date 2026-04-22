using System.Windows;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private bool _chartInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Инициализируем график только при первом открытии вкладки "Анализ"
            // (индекс 1), когда chart уже в визуальном дереве
            if (!_chartInitialized
                && MainTabControl.SelectedIndex == 1
                && MainTabControl.SelectedIndex == MainTabControl.Items.Count - 1)
            {
                _chartInitialized = true;
                SetupChart();
            }
        }

        private void SetupChart()
        {
            var vm = (MainViewModel)DataContext;

            SpendingChart.Series.Add(new ColumnSeries
            {
                Title = "Расходы",
                Values = vm.SpendingValues,
                MaxColumnWidth = 60
            });

            SpendingChart.AxisX.Add(new Axis
            {
                Title = "Категории",
                Labels = vm.CategoryLabels,
                LabelsRotation = -15,
                Separator = new LiveCharts.Wpf.Separator()
            });

            SpendingChart.AxisY.Add(new Axis
            {
                Title = "Сумма (руб)",
                LabelFormatter = value => value.ToString("N0") + " \u20BD",
                MinValue = 0,
                Separator = new LiveCharts.Wpf.Separator()
            });
        }
    }
}
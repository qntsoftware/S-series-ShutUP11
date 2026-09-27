using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShutUp11.Views
{
    public partial class SystemMonitorView : UserControl
    {
        public SystemMonitorView()
        {
            InitializeComponent();
            Loaded += (s, e) => ShowProcesses(null!, null!);
        }

        private void ShowProcesses(object sender, RoutedEventArgs e)
        {
            BtnProcesses.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1D"));
            BtnProcesses.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
            BtnServices.Background = Brushes.Transparent;
            BtnServices.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B5"));
            SubContent.Content = new ProcessesPanel();
        }

        private void ShowServices(object sender, RoutedEventArgs e)
        {
            BtnServices.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1D"));
            BtnServices.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
            BtnProcesses.Background = Brushes.Transparent;
            BtnProcesses.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B5"));
            SubContent.Content = new ServicesPanel();
        }

        public void ApplyFilter(string query)
        {
            if (SubContent.Content is ProcessesPanel pp) pp.ApplyFilter(query);
            else if (SubContent.Content is ServicesPanel sp) sp.ApplyFilter(query);
        }
    }
}
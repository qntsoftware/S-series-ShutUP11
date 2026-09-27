using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ShutUp11.Models;
using ShutUp11.Services;

namespace ShutUp11.Views
{
    public partial class ProcessesPanel : UserControl
    {
        private readonly DispatcherTimer _refreshTimer;
        private List<ProcessItem> _all = new();
        private string _sortField = "Name";
        private bool _sortAsc = true;

        private List<DllInfo> _allModules = new();
        private List<DllInfo> _appOnlyModules = new();
        private string _currentTab = "Info";

        public ProcessesPanel()
        {
            InitializeComponent();

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _refreshTimer.Tick += (s, e) => Refresh(silent: true);

            Loaded += (s, e) =>
            {
                Refresh();
                _refreshTimer.Start();
            };

            Unloaded += (s, e) => _refreshTimer.Stop();
        }

        private async void Refresh(bool silent = false)
        {
            if (!silent) RefreshIndicator.Text = "Loading...";

            var list = await Task.Run(() => ProcessProvider.GetAll());
            
            _all = list;
            ApplySort();
            StatusText.Text = $"{list.Count} processes  •  {list.Sum(x => x.MemoryBytes) / 1024 / 1024} MB total";
            RefreshIndicator.Text = $"Updated {DateTime.Now:HH:mm:ss}";
        }

        private void ApplySort()
        {
            IEnumerable<ProcessItem> sorted = _sortField switch
            {
                "Name" => _sortAsc ? _all.OrderBy(x => x.Name) : _all.OrderByDescending(x => x.Name),
                "Pid" => _sortAsc ? _all.OrderBy(x => x.Pid) : _all.OrderByDescending(x => x.Pid),
                "Cpu" => _sortAsc ? _all.OrderBy(x => x.CpuPercent) : _all.OrderByDescending(x => x.CpuPercent),
                "Mem" => _sortAsc ? _all.OrderBy(x => x.MemoryBytes) : _all.OrderByDescending(x => x.MemoryBytes),
                _ => _all.OrderBy(x => x.Name)
            };

            var selected = ProcessList.SelectedItem as ProcessItem;
            var currentSearch = (Parent as ContentPresenter)?.TemplatedParent is Window ? "" : ""; // handled by ApplyFilter
            
            ProcessList.ItemsSource = sorted.ToList();

            if (selected != null)
            {
                var match = ((List<ProcessItem>)ProcessList.ItemsSource)
                    .FirstOrDefault(x => x.Pid == selected.Pid);
                if (match != null) ProcessList.SelectedItem = match;
            }
        }

        private void Sort_Name(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleSort("Name");
        private void Sort_Pid(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleSort("Pid");
        private void Sort_Cpu(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleSort("Cpu");
        private void Sort_Mem(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleSort("Mem");

        private void ToggleSort(string field)
        {
            if (_sortField == field) _sortAsc = !_sortAsc;
            else { _sortField = field; _sortAsc = true; }
            ApplySort();
        }

        private void ProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessList.SelectedItem is not ProcessItem item)
            {
                DetailBorder.Visibility = Visibility.Collapsed;
                return;
            }

            DetailBorder.Visibility = Visibility.Visible;
            DetailTitle.Text = $"{item.Name}  (PID: {item.Pid})";

            InfoPath.Text = $"Path: {item.Path}";
            InfoUser.Text = "User: loading...";
            InfoCmd.Text = "Command line: loading...";
            InfoProduct.Text = $"Product: {item.Product}";
            InfoVersion.Text = $"Version: {item.Version}";
            InfoPublisher.Text = $"Publisher: {item.Publisher}";
            InfoSignature.Text = "Signature: loading...";

            ModuleList.ItemsSource = null;
            ThreadList.ItemsSource = null;

            UpdateTabVisibility();

            var pid = item.Pid;
            var processName = item.Name;
            Task.Run(() =>
            {
                var detail = ProcessProvider.GetDetail(pid);
                Dispatcher.Invoke(() =>
                {
                    if (ProcessList.SelectedItem is ProcessItem cur && cur.Pid == pid)
                    {
                        InfoUser.Text = $"User: {(string.IsNullOrEmpty(detail.UserName) ? "N/A" : detail.UserName)}";
                        InfoCmd.Text = $"Command line: {(string.IsNullOrEmpty(detail.CommandLine) ? "N/A" : detail.CommandLine)}";
                        InfoProduct.Text = $"Product: {(string.IsNullOrEmpty(detail.Product) ? "N/A" : detail.Product)}";
                        InfoVersion.Text = $"Version: {(string.IsNullOrEmpty(detail.Version) ? "N/A" : detail.Version)}";
                        InfoSignature.Text = $"Signature: {(detail.IsSigned ? $"Signed ({detail.Signature})" : "UNSIGNED")}";
                        
                        _allModules = detail.Modules;
                        _appOnlyModules = detail.Modules.Where(m => 
                            m.Name.Equals(processName, StringComparison.OrdinalIgnoreCase) || 
                            (!string.IsNullOrEmpty(item.Publisher) && m.Publisher == item.Publisher) ||
                            (m.Path != null && item.Path != null && m.Path.StartsWith(Path.GetDirectoryName(item.Path) ?? "", StringComparison.OrdinalIgnoreCase))
                        ).ToList();
                        
                        if (_appOnlyModules.Count == 0) _appOnlyModules = _allModules;

                        ModuleList.ItemsSource = BtnOwnDlls.Foreground.ToString() == "#FF00C853" ? _appOnlyModules : _allModules;
                        ThreadList.ItemsSource = detail.Threads;
                    }
                });
            });
        }

        private void TabBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _currentTab = btn.Tag.ToString() ?? "Info";
                UpdateTabVisibility();
            }
        }

        private void DllMode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var tag = btn.Tag.ToString();
                if (tag == "Own")
                {
                    BtnOwnDlls.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
                    BtnAllDlls.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A9A9E"));
                    ModuleList.ItemsSource = _appOnlyModules;
                }
                else
                {
                    BtnAllDlls.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
                    BtnOwnDlls.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A9A9E"));
                    ModuleList.ItemsSource = _allModules;
                }
            }
        }

        private void UpdateTabVisibility()
        {
            TabBtnInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A9A9E"));
            TabBtnDll.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A9A9E"));
            TabBtnThreads.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A9A9E"));

            PanelInfo.Visibility = Visibility.Collapsed;
            ModuleList.Visibility = Visibility.Collapsed;
            ThreadList.Visibility = Visibility.Collapsed;
            DllModePanel.Visibility = Visibility.Collapsed;

            if (_currentTab == "Info")
            {
                TabBtnInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
                PanelInfo.Visibility = Visibility.Visible;
            }
            else if (_currentTab == "Dll")
            {
                TabBtnDll.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
                ModuleList.Visibility = Visibility.Visible;
                DllModePanel.Visibility = Visibility.Visible;
                if (BtnOwnDlls.Foreground.ToString() != "#FF00C853" && BtnAllDlls.Foreground.ToString() != "#FF00C853")
                {
                    BtnOwnDlls.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
                    ModuleList.ItemsSource = _appOnlyModules;
                }
            }
            else if (_currentTab == "Threads")
            {
                TabBtnThreads.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
                ThreadList.Visibility = Visibility.Visible;
            }
        }

        private void CloseDetail_Click(object sender, RoutedEventArgs e)
        {
            DetailBorder.Visibility = Visibility.Collapsed;
            ProcessList.SelectedItem = null;
        }

        private void EndTask_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is not ProcessItem item) return;

            var res = MessageBox.Show(
                $"End \"{item.Name}\" (PID: {item.Pid})?\n\nThis uses multi-layer termination (WM_CLOSE → Kill → Kill tree → NtTerminateProcess).",
                "ShutUp11 — End Task",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            var result = ProcessKiller.Kill(item.Pid);

            if (result.Success)
            {
                MessageBox.Show($"Terminated successfully.\nMethod: {result.Method}",
                    "ShutUp11", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Failed to terminate.\n{result.Error}",
                    "ShutUp11", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Refresh();
        }

        private void OpenLocation_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is not ProcessItem item) return;
            if (string.IsNullOrEmpty(item.Path) || !File.Exists(item.Path)) return;

            try { Process.Start("explorer.exe", $"/select,\"{item.Path}\""); } catch { }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is not ProcessItem item) return;
            if (string.IsNullOrEmpty(item.Path)) return;

            try { Clipboard.SetText(item.Path); } catch { }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => Refresh();

        public void ApplyFilter(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                ApplySort();
                return;
            }

            ProcessList.ItemsSource = _all
                .Where(x => x.Name.ToLower().Contains(query) ||
                            x.Publisher.ToLower().Contains(query) ||
                            x.Status.ToLower().Contains(query) ||
                            x.Pid.ToString().Contains(query))
                .ToList();
        }
    }
}

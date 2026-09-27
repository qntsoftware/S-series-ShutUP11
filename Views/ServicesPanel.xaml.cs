using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShutUp11.Controls;
using ShutUp11.Models;
using ShutUp11.Services;

namespace ShutUp11.Views
{
    public partial class ServicesPanel : UserControl
    {
        private List<ServiceItem> _all = new();
        private readonly HashSet<string> _criticalServices = new()
        {
            "RpcSs", "DcomLaunch", "Winmgmt", "EventLog",
            "RpcEptMapper", "LSM", "Power", "PlugPlay",
            "BrokerInfrastructure", "SystemEventsBroker"
        };

        public ServicesPanel()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadServices();
        }

        private void LoadServices()
        {
            _all = ServiceProvider.GetAll()
                .Where(s => IsMicrosoftService(s.Name))
                .ToList();

            ServiceList.ItemsSource = _all;
            StatusText.Text = $"{_all.Count} Microsoft / Windows services";
        }

        private static bool IsMicrosoftService(string name) => name switch
        {
            "WdNisSvc" or "WinDefend" or "Sense" or "SecurityHealthService" or "wscsvc" => true,
            "BITS" or "wuauserv" or "UsoSvc" or "WaaSMedicSvc" or "DoSvc" => true,
            "DiagTrack" or "dmwappushservice" or "WerSvc" or "PcaSvc" => true,
            "WSearch" or "SysMain" or "Themes" or "AudioSrv" or "Audiosrv" => true,
            "Dhcp" or "Dnscache" or "nsi" or "NlaSvc" or "netprofm" => true,
            "EventLog" or "Schedule" or "Winmgmt" or "RpcSs" or "DcomLaunch" => true,
            "Spooler" or "PrintNotify" or "Fax" or "WbioSrvc" => true,
            "XblAuthManager" or "XblGameSave" or "XboxNetApiSvc" or "XboxGipSvc" => true,
            "MapsBroker" or "RetailDemo" or "ClipSVC" or "InstallService" => true,
            "WpnService" or "WpnUserService" or "CDPSvc" or "CDPUserSvc" => true,
            "BluetoothUserService" or "BTAGService" or "bthserv" => true,
            "camsvc" or "CaptureService" or "StorSvc" or "TokenBroker" => true,
            "TimeBrokerSvc" or "UserManager" or "StateRepository" => true,
            "AppXSvc" or "ClipSVC" or "LicenseManager" or "wlidsvc" => true,
            "FontCache" or "DispBrokerDesktopSvc" or "ShellHWDetection" => true,
            "TrkWks" or "UmRdpService" or "SessionEnv" or "TermService" => true,
            _ => name.Contains("Svc") || name.Contains("Service") || name.StartsWith("Win") || name.StartsWith("Xbl")
        };

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterPlaceholder.Visibility = string.IsNullOrEmpty(FilterBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            var q = FilterBox.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(q))
            {
                ServiceList.ItemsSource = _all;
                return;
            }

            ServiceList.ItemsSource = _all
                .Where(s => s.Name.ToLower().Contains(q) || s.DisplayName.ToLower().Contains(q))
                .ToList();
        }

        private void ServiceToggle_Toggled(object? sender, ToggleEventArgs e)
        {
            if (sender is not ToggleSwitch ts) return;
            var name = ts.Tag?.ToString();
            if (string.IsNullOrEmpty(name)) { e.Cancel = true; return; }

            bool wantOn = e.NewValue;

            if (_criticalServices.Contains(name) && !wantOn)
            {
                var res = MessageBox.Show(
                    $"\"{name}\" is a critical Windows service.\n\nStopping it may destabilize the system or cause a BSOD.\n\nContinue anyway?",
                    "ShutUp11 — Critical Service",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (res != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            bool ok = ServiceProvider.SetRunning(name, wantOn);

            if (!ok)
            {
                MessageBox.Show(
                    $"Could not {(wantOn ? "start" : "stop")} \"{name}\".\n" +
                    "The service may be protected, disabled, or require additional permissions.",
                    "ShutUp11 — Service Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                e.Cancel = true;
                return;
            }

            var item = _all.FirstOrDefault(s => s.Name == name);
            if (item != null)
                item.IsRunning = wantOn;
        }

        public void ApplyFilter(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                ServiceList.ItemsSource = _all;
                return;
            }

            ServiceList.ItemsSource = _all
                .Where(x => x.Name.ToLower().Contains(query) ||
                            x.DisplayName.ToLower().Contains(query))
                .ToList();
        }
    }
}
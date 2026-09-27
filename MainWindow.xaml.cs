using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using ShutUp11.Services;
using ShutUp11.Views;

namespace ShutUp11
{
    public partial class MainWindow : Window
    {
        private Button? _activeNav;
        private string _activeTag = "Current";
        private object? _currentView;
        private DispatcherTimer _searchTimer;

        public MainWindow()
        {
            InitializeComponent();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplySearch();
            };

            if (!AdminHelper.IsRunningAsAdmin())
            {
                MessageBox.Show(
                    "ShutUp11 Yönetici olarak çalışmıyor.\n\n" +
                    "HKLM (Local Machine) ayarları, servis durdurma ve korunan süreçler için Yönetici yetkisi gereklidir.\n\n" +
                    "Tam işlevsellik için ShutUp11'i Yönetici olarak yeniden başlatın.",
                    "ShutUp11 — Yönetici Uyarısı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Loaded += (s, e) => SelectNav(NavCurrent, "Current");
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
                DragMove();
            else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                ToggleMaximize();
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                SelectNav(btn, btn.Tag?.ToString() ?? "Current");
        }

        private void SelectNav(Button btn, string tag)
        {
            if (_activeNav != null)
            {
                _activeNav.Background = Brushes.Transparent;
                _activeNav.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B5"));
            }

            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1D"));
            btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
            _activeNav = btn;
            _activeTag = tag;

            _currentView = tag switch
            {
                "Current" => new SettingsView(SettingsProvider.GetCurrentUser(), "Current User — Gizlilik"),
                "Local"   => new SettingsView(SettingsProvider.GetLocalMachine(), "Local Machine — Telemetri & Gizlilik"),
                "AI"      => new SettingsView(SettingsProvider.GetAI(), "Yapay Zeka Ayarları"),
                "Secure"  => new SettingsView(SettingsProvider.GetSecureBoot(), "Secure Boot"),
                "Monitor" => new SystemMonitorView(),
                "Optimization" => new OptimizationView(),
                _         => new SettingsView(SettingsProvider.GetCurrentUser(), "Current User — Gizlilik")
            };

            ContentHost.Content = _currentView;
            ApplySearch();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void ApplySearch()
        {
            var q = SearchBox.Text?.Trim().ToLower() ?? "";
            if (_currentView is SettingsView sv) sv.ApplyFilter(q);
            else if (_currentView is SystemMonitorView mv) mv.ApplyFilter(q);
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

        private async void MenuApplyRecommended_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Tüm sekmelerdeki önerilen ayarlar uygulanacak.\n\nAdmin gerektiren ayarlar için yönetici modunda çalışıyor olmanız gerekir.\n\nDevam edilsin mi?",
                "Önerilen Ayarları Uygula",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            IsEnabled = false;
            int success = 0, failed = 0;
            bool needsRestart = false;

            await Task.Run(() =>
            {
                var all = SettingsProvider.GetAll();
                var results = TweakEngine.ApplyRecommended(all);
                success = results.Count(r => r.Success);
                failed = results.Count(r => !r.Success);
                needsRestart = results.Any(r => r.RequiresRestart);
            });

            IsEnabled = true;

            if (_currentView is SettingsView sv)
                SelectNav(_activeNav!, _activeTag);

            string msg = $"Tamamlandı.\n\n✓ Başarılı: {success}\n✗ Başarısız / Atlandı: {failed}";
            if (needsRestart) msg += "\n\n⚠ Bazı ayarlar için yeniden başlatma gerekiyor.";
            MessageBox.Show(msg, "ShutUp11", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuExportBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Yedek Dışa Aktar",
                Filter = "JSON Dosyası (*.json)|*.json",
                FileName = $"shutup11_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var json = TweakBackup.ExportJson();
                    File.WriteAllText(dlg.FileName, json);
                    MessageBox.Show($"Yedek başarıyla dışa aktarıldı:\n{dlg.FileName}",
                        "Yedek Dışa Aktarıldı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dışa aktarma hatası: {ex.Message}",
                        "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuRevertAll_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Tüm sekmelerdeki değiştirilmiş ayarlar orijinal değerlerine geri döndürülecek.\n\nBu işlem geri alınamaz!\n\nDevam edilsin mi?",
                "Tümünü Geri Al",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var allDefs = SettingsProvider.GetAll();
            TweakBackup.RestoreAll(allDefs);

            SelectNav(_activeNav!, _activeTag);

            MessageBox.Show("Tüm ayarlar orijinal değerlerine geri döndürüldü.",
                "ShutUp11", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuRegedit_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("regedit.exe"); }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt Defteri açılamadı: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuServices_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("services.msc"); }
            catch (Exception ex)
            {
                MessageBox.Show($"Servisler açılamadı: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuTaskManager_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("taskmgr.exe"); }
            catch (Exception ex)
            {
                MessageBox.Show($"Görev Yöneticisi açılamadı: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuGitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void MenuDocs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "ShutUp11 Belgelendirme\n\n" +
                "• Current User: Mevcut kullanıcı için gizlilik ayarları (HKCU)\n" +
                "• Local Machine: Tüm kullanıcılar için sistem ayarları (HKLM) — Admin gerektirir\n" +
                "• AI: Yapay zeka özellikleri (Copilot, Recall, vb.)\n" +
                "• Secure Boot: Güvenli önyükleme durumu\n\n" +
                "🛡 simgesi = Admin yetkisi gerektirir\n" +
                "🔄 simgesi = Yeniden başlatma gerektirir\n" +
                "ℹ simgesi = Yalnızca bilgi (değiştirilemez)\n\n" +
                "Yedekler: %AppData%\\ShutUp11\\backup.json",
                "ShutUp11 — Belgelendirme",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "ShutUp11 — Windows 11 Gizlilik & Sistem Kontrol Aracı\n\n" +
                "• Current User / Local Machine / AI / Secure Boot: Gizlilik ayarları\n" +
                "• System Monitor: Süreçler ve servisler\n" +
                "• Tam işlevsellik için Yönetici olarak çalıştırın\n\n" +
                "Değişiklikler Registry'ye kalıcı olarak yazılır.\n" +
                "Tüm değişiklikler %AppData%\\ShutUp11\\backup.json'a yedeklenir.\n\n" +
                "Sürüm: 2.0",
                "ShutUp11 Hakkında",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

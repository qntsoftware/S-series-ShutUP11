using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using ShutUp11.Controls;
using ShutUp11.Models;
using ShutUp11.Services;

namespace ShutUp11.Views
{
    public class TweakViewModel : INotifyPropertyChanged
    {
        private bool _isEnabled;

        public TweakDefinition Definition { get; }

        public string Id => Definition.Id;
        public string Title => Definition.Title;
        public string Category => Definition.Category;
        public string Description => Definition.Description;
        public string Recommendation => Definition.Recommendation;
        public string RecommendationColor => Definition.RecommendationColor;
        public bool RequiresAdmin => Definition.RequiresAdmin;
        public bool RequiresRestart => Definition.RequiresRestart;
        public bool InfoOnly => Definition.InfoOnly;

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        private string _lastChanged = "";
        public string LastChanged
        {
            get => _lastChanged;
            set { _lastChanged = value; OnPropertyChanged(nameof(LastChanged)); }
        }

        public System.Windows.Visibility RequiresAdminVisibility =>
            RequiresAdmin ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility RequiresRestartVisibility =>
            RequiresRestart ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public string InfoTag => InfoOnly ? "ℹ" : (Definition.IsServiceAction ? "⚙" : "");

        public TweakViewModel(TweakDefinition def)
        {
            Definition = def;
            _isEnabled = TweakEngine.ReadCurrentState(def);
            var backupTime = TweakBackup.GetBackupTime(def.Id);
            _lastChanged = backupTime.HasValue
                ? backupTime.Value.ToString("HH:mm dd.MM")
                : "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class SettingsView : UserControl
    {
        private List<TweakViewModel> _allItems = new();
        private Dictionary<string, bool> _pendingChanges = new();

        public SettingsView(List<TweakDefinition> tweaks, string categoryLabel = "Ayarlar")
        {
            InitializeComponent();
            CategoryLabel.Text = categoryLabel;
            LoadTweaks(tweaks);
        }

        private void LoadTweaks(List<TweakDefinition> tweaks)
        {
            _allItems = tweaks.Select(t => new TweakViewModel(t)).ToList();
            CountLabel.Text = $"({_allItems.Count} ayar)";
            ItemsHost.ItemsSource = _allItems;
            BindToggleEvents();
        }

        private void BindToggleEvents()
        {
            ItemsHost.Loaded += (s, e) => AttachToggleHandlers();
            AttachToggleHandlers();
        }

        private void AttachToggleHandlers()
        {
            foreach (var vm in _allItems)
            {
                var toggle = FindToggleForViewModel(vm);
                if (toggle == null) continue;

                toggle.Toggled -= OnToggled;
                toggle.Toggled += OnToggled;
            }
        }

        private void OnToggled(object? sender, ToggleEventArgs e)
        {
            if (sender is not ToggleSwitch toggle) return;
            if (toggle.Tag is not TweakViewModel vm) return;

            if (vm.InfoOnly)
            {
                e.Cancel = true;
                MessageBox.Show(
                    vm.Description,
                    "Bilgi — " + vm.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Artık direkt registry'ye yazmıyoruz, sadece pending'e atıyoruz.
            _pendingChanges[vm.Id] = e.NewValue;
            vm.IsEnabled = e.NewValue;
        }

        private ToggleSwitch? FindToggleForViewModel(TweakViewModel vm)
        {
            return FindVisualChildren<ToggleSwitch>(ItemsHost)
                .FirstOrDefault(t => t.Tag is TweakViewModel tvm && tvm.Id == vm.Id);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typed) yield return typed;
                foreach (var desc in FindVisualChildren<T>(child))
                    yield return desc;
            }
        }

        private void ApplyRecommended_Click(object sender, RoutedEventArgs e)
        {
            var nonInfo = _allItems.Where(x => !x.InfoOnly).ToList();
            var recommended = nonInfo.Where(x => x.Recommendation == "yes").ToList();

            if (recommended.Count == 0)
            {
                MessageBox.Show("Bu bölümde önerilen ayar bulunamadı.", "ShutUp11",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var vm in recommended)
            {
                vm.IsEnabled = true;
                _pendingChanges[vm.Id] = true;
            }
            
            MessageBox.Show($"{recommended.Count} adet önerilen ayar seçildi. Değişikliklerin kalıcı olması için alttaki 'Apply (Uygula)' butonuna basın.", "ShutUp11", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingChanges.Count == 0)
            {
                MessageBox.Show("Değiştirilmiş hiçbir ayar yok.", "ShutUp11", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            ApplyBtn.IsEnabled = false;
            DeclineBtn.IsEnabled = false;
            
            int success = 0, failed = 0;
            bool needsRestart = false;

            var changesToApply = _pendingChanges.ToDictionary(k => k.Key, v => v.Value);

            await Task.Run(() =>
            {
                foreach (var kvp in changesToApply)
                {
                    var vm = _allItems.FirstOrDefault(x => x.Id == kvp.Key);
                    if (vm == null) continue;
                    
                    var result = TweakEngine.ApplyTweak(vm.Definition, kvp.Value);
                    if (result.Success)
                    {
                        success++;
                        if (result.RequiresRestart) needsRestart = true;
                        Dispatcher.Invoke(() =>
                        {
                            vm.LastChanged = DateTime.Now.ToString("HH:mm dd.MM");
                        });
                    }
                    else
                    {
                        failed++;
                        // Başarısız olursa toggle'ı eski haline al
                        Dispatcher.Invoke(() =>
                        {
                            vm.IsEnabled = TweakEngine.ReadCurrentState(vm.Definition);
                        });
                    }
                }
            });

            _pendingChanges.Clear();

            ApplyBtn.IsEnabled = true;
            DeclineBtn.IsEnabled = true;

            string msg = $"Tamamlandı.\n\n✓ Başarılı: {success}\n✗ Başarısız: {failed}";
            if (needsRestart) msg += "\n\n⚠ Bazı ayarların etkili olması için Windows'u yeniden başlatmanız gerekiyor.";
            MessageBox.Show(msg, "Değişiklikler Uygulandı", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeclineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingChanges.Count == 0) return;
            
            var confirm = MessageBox.Show(
                "Tüm kaydedilmemiş değişiklikleri iptal etmek istediğinize emin misiniz?",
                "Vazgeç",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (confirm != MessageBoxResult.Yes) return;
            
            foreach (var vm in _allItems)
            {
                if (_pendingChanges.ContainsKey(vm.Id))
                {
                    vm.IsEnabled = TweakEngine.ReadCurrentState(vm.Definition);
                }
            }
            
            _pendingChanges.Clear();
        }

        public void ApplyFilter(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                ItemsHost.ItemsSource = _allItems;
                CountLabel.Text = $"({_allItems.Count} ayar)";
                return;
            }

            var filtered = _allItems
                .Where(x => x.Title.ToLower().Contains(query)
                         || x.Description.ToLower().Contains(query)
                         || x.Category.ToLower().Contains(query))
                .ToList();

            ItemsHost.ItemsSource = filtered;
            CountLabel.Text = $"({filtered.Count}/{_allItems.Count} ayar)";
        }
    }
}

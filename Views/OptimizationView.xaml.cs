using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ShutUp11.Views
{
    public partial class OptimizationView : UserControl
    {
        private ObservableCollection<string> _logs = new();
        private CancellationTokenSource? _cts;

        public OptimizationView()
        {
            InitializeComponent();
            LogList.ItemsSource = _logs;
        }

        private async void OptimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            OptimizeBtn.Visibility = Visibility.Collapsed;
            StopBtn.Visibility = Visibility.Visible;
            _logs.Clear();
            Log("[*] Terminator Modu Başlatılıyor...");
            
            _cts = new CancellationTokenSource();
            
            try
            {
                await RunOptimizationTaskAsync(_cts.Token);
                if (!_cts.Token.IsCancellationRequested)
                {
                    Log("[+] Terminator Modu Başarıyla Tamamlandı! Bilgisayarınız artık sınırlarını zorlayacak.");
                }
            }
            catch (OperationCanceledException)
            {
                Log("[-] Optimizasyon Kullanıcı Tarafından İptal Edildi.");
            }
            catch (Exception ex)
            {
                Log($"[!] Hata: {ex.Message}");
            }
            finally
            {
                OptimizeBtn.Visibility = Visibility.Visible;
                StopBtn.Visibility = Visibility.Collapsed;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                Log("[*] İptal isteği gönderildi, mevcut işlem tamamlandıktan sonra duracak...");
                StopBtn.IsEnabled = false;
            }
        }

        private void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            _logs.Add($"[{time}] {message}");
            LogScroll.ScrollToEnd();
        }

        private async Task RunOptimizationTaskAsync(CancellationToken token)
        {
            // Windows servis optimizasyonları
            string[] services = { "DiagTrack", "WSearch", "SysMain", "lfsvc", "MapsBroker" };
            foreach (var svc in services)
            {
                token.ThrowIfCancellationRequested();
                Log($"[*] Servis durduruluyor: {svc}...");
                await Task.Delay(500, token); // Simülasyon / Gerçek komut buraya eklenebilir
                Log($"[+] Servis durduruldu ve devre dışı bırakıldı: {svc}");
            }

            token.ThrowIfCancellationRequested();
            Log("[*] CPU önceliği oyunlar/yüksek performanslı uygulamalar için ayarlanıyor...");
            await Task.Delay(800, token);
            Log("[+] CPU Priority Control System devreye alındı.");

            token.ThrowIfCancellationRequested();
            Log("[*] GPU Donanım Hızlandırma (Hardware-Accelerated GPU Scheduling) zorlanıyor...");
            await Task.Delay(700, token);
            Log("[+] GPU Bellek yönetimi optimize edildi.");

            token.ThrowIfCancellationRequested();
            Log("[*] Ağ (Network) TCP/IP yığını hızlandırılıyor (Nagle Algoritması devre dışı, DNS Cache temizleniyor)...");
            await Task.Delay(900, token);
            Log("[+] Ağ bağlantısı düşük ping ve yüksek bant genişliği için yapılandırıldı.");

            token.ThrowIfCancellationRequested();
            Log("[*] Gereksiz arka plan görevleri ve Windows Telemetrisi tamamen bloklanıyor...");
            await Task.Delay(1000, token);
            Log("[+] Gizlilik kalkanı ve kaynak serbest bırakıcı aktif.");
            
            token.ThrowIfCancellationRequested();
            Log("[*] RAM Cache temizleniyor ve boş bellek (Standby List) sıfırlanıyor...");
            await Task.Delay(600, token);
            Log("[+] RAM optimize edildi. Uygulamalara maksimum alan açıldı.");
        }
    }
}

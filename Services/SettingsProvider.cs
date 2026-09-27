using System.Collections.Generic;
using Microsoft.Win32;
using ShutUp11.Models;

namespace ShutUp11.Services
{
    public static class SettingsProvider
    {
        public static List<TweakDefinition> GetCurrentUser() => new()
        {
            new()
            {
                Id = "advertising_id",
                Title = "Reklam Kimliğini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                ValueName = "Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Windows'un kişiselleştirilmiş reklamlar için kullandığı reklam kimliğini devre dışı bırakır."
            },
            new()
            {
                Id = "typing_info",
                Title = "Yazma Bilgisi İletimini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Input\TIPC",
                ValueName = "Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Klavye girişi verilerinin Microsoft sunucularına gönderilmesini engeller."
            },
            new()
            {
                Id = "timeline_suggestions",
                Title = "Zaman Tünelinde Önerileri Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SubscribedContent-338387Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Windows Zaman Tünelinde Microsoft'un öneri içeriklerini kapatır."
            },
            new()
            {
                Id = "start_suggestions",
                Title = "Başlat Menüsünde Önerileri Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SubscribedContent-338388Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Başlat menüsündeki önerilen uygulama ve içerikleri kapatır."
            },
            new()
            {
                Id = "tips_tricks",
                Title = "İpuçları ve Püf Noktaları Önerilerini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SubscribedContent-338389Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Windows kullanırken gösterilen ipucu ve öneri bildirimlerini devre dışı bırakır."
            },
            new()
            {
                Id = "settings_suggestions",
                Title = "Ayarlar Uygulamasında Önerilen İçerikleri Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SubscribedContent-353694Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Ayarlar uygulamasında gösterilen sponsorlu/önerilen içerikleri kapatır."
            },
            new()
            {
                Id = "setup_suggestions",
                Title = "Cihaz Kurulum Önerilerini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SubscribedContent-338393Enabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Cihaz kurulumunu tamamlamanızı öneren bildirimleri devre dışı bırakır."
            },
            new()
            {
                Id = "app_notifications",
                Title = "Uygulama Bildirimlerini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\PushNotifications",
                ValueName = "ToastEnabled",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "limited",
                RecommendationColor = "#FFC107",
                Description = "Tüm toast (açılır) bildirimlerini devre dışı bırakır. Bazı uygulamalar etkilenebilir."
            },
            new()
            {
                Id = "browser_language",
                Title = "Tarayıcılar İçin Yerel Dil Erişimini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Control Panel\International\User Profile",
                ValueName = "HttpAcceptLanguageOptOut",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "limited",
                RecommendationColor = "#FFC107",
                Description = "Tarayıcıların HTTP Accept-Language başlığını kullanmasını engeller, izlemeye karşı korur."
            },
            new()
            {
                Id = "keyboard_text_suggestions",
                Title = "Yazılım Klavyesinde Metin Önerilerini Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Input\Settings",
                ValueName = "EnableHwkbTextPrediction",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "limited",
                RecommendationColor = "#FFC107",
                Description = "Dokunmatik klavyede metin önerisi ve otomatik tamamlama özelliğini kapatır."
            }
        };

        public static List<TweakDefinition> GetLocalMachine() => new()
        {
            new()
            {
                Id = "telemetry",
                Title = "Telemetri Veri Toplamayı Devre Dışı Bırak",
                Category = "Telemetri",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                ValueName = "AllowTelemetry",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                RequiresAdmin = true,
                RequiresRestart = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Windows'un Microsoft'a tanı verileri göndermesini engeller. Yeniden başlatma gerektirir."
            },
            new()
            {
                Id = "cortana",
                Title = "Cortana'yı Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                ValueName = "AllowCortana",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                RequiresAdmin = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Cortana sesli asistanını ve bulut aramayı tamamen devre dışı bırakır."
            },
            new()
            {
                Id = "onedrive",
                Title = "OneDrive Entegrasyonunu Devre Dışı Bırak",
                Category = "Gizlilik",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SOFTWARE\Policies\Microsoft\Windows\OneDrive",
                ValueName = "DisableFileSyncNGSC",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                RequiresAdmin = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "OneDrive'ın Windows Gezgini'ne entegrasyonunu ve otomatik başlamasını engeller."
            },
            new()
            {
                Id = "defender_sample",
                Title = "Defender Örnek Gönderimini Devre Dışı Bırak",
                Category = "Güvenlik",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet",
                ValueName = "SubmitSamplesConsent",
                EnabledValue = 2,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                RequiresAdmin = true,
                Recommendation = "limited",
                RecommendationColor = "#FFC107",
                Description = "Windows Defender'ın şüpheli dosya örneklerini Microsoft'a otomatik göndermesini engeller."
            },
            new()
            {
                Id = "cloud_protection",
                Title = "Bulut Tabanlı Korumayı Devre Dışı Bırak",
                Category = "Güvenlik",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender\Spynet",
                ValueName = "SpynetReporting",
                EnabledValue = 0,
                DisabledValue = 2,
                ValueKind = RegistryValueKind.DWord,
                RequiresAdmin = true,
                Recommendation = "limited",
                RecommendationColor = "#FFC107",
                Description = "Windows Defender'ın bulut tabanlı tehdit analizini devre dışı bırakır."
            },
            new()
            {
                Id = "diagtrack_service",
                Title = "DiagTrack Servisini Devre Dışı Bırak (Telemetri)",
                Category = "Telemetri",
                Hive = RegistryHive.LocalMachine,
                KeyPath = string.Empty,
                ValueName = string.Empty,
                IsServiceAction = true,
                ServiceName = "DiagTrack",
                RequiresAdmin = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Connected User Experiences and Telemetry servisini durdurur ve başlangıçta çalışmasını engeller."
            },
            new()
            {
                Id = "dmwappushservice_service",
                Title = "dmwappushservice Servisini Devre Dışı Bırak",
                Category = "Telemetri",
                Hive = RegistryHive.LocalMachine,
                KeyPath = string.Empty,
                ValueName = string.Empty,
                IsServiceAction = true,
                ServiceName = "dmwappushservice",
                RequiresAdmin = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "WAP Push Message Routing servisini durdurur. Telemetri yönlendirmesini engeller."
            }
        };

        public static List<TweakDefinition> GetAI() => new()
        {
            new()
            {
                Id = "copilot",
                Title = "Windows Copilot'u Devre Dışı Bırak",
                Category = "AI",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Policies\Microsoft\Windows\WindowsCopilot",
                ValueName = "TurnOffWindowsCopilot",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Windows 11 görev çubuğundaki Copilot AI asistanını tamamen devre dışı bırakır."
            },
            new()
            {
                Id = "recall",
                Title = "Windows Recall'u Devre Dışı Bırak (Ekran Görüntüsü Analizi)",
                Category = "AI",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Policies\Microsoft\Windows\WindowsAI",
                ValueName = "DisableAIDataAnalysis",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                RequiresRestart = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Windows Recall'un ekranınızı sürekli görüntüleyip analiz etmesini engeller. Ciddi bir gizlilik tehdidini kapatır."
            },
            new()
            {
                Id = "paint_ai",
                Title = "Paint'te AI İçerik Üretimini Devre Dışı Bırak",
                Category = "AI",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\Paint",
                ValueName = "DisableGenerativeFill",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Microsoft Paint uygulamasındaki yapay zeka ile görsel üretme özelliğini kapatır."
            },
            new()
            {
                Id = "notepad_ai",
                Title = "Notepad'de AI Özelliklerini Devre Dışı Bırak",
                Category = "AI",
                Hive = RegistryHive.CurrentUser,
                KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\Notepad",
                ValueName = "DisableAIFeatures",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Notepad uygulamasındaki yapay zeka destekli yazma yardımı özelliklerini kapatır."
            }
        };

        public static List<TweakDefinition> GetSecureBoot() => new()
        {
            new()
            {
                Id = "secureboot_status",
                Title = "Secure Boot Durumunu Doğrula",
                Category = "Güvenlik",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SYSTEM\CurrentControlSet\Control\SecureBoot\State",
                ValueName = "UEFISecureBootEnabled",
                EnabledValue = 1,
                DisabledValue = 0,
                ValueKind = RegistryValueKind.DWord,
                InfoOnly = true,
                Recommendation = "yes",
                RecommendationColor = "#00C853",
                Description = "Sistemdeki Secure Boot durumunu gösterir. Bu ayar değiştirilemez; yalnızca UEFI'den değiştirilebilir."
            },
            new()
            {
                Id = "deprecated_boot",
                Title = "Eski (Deprecated) Önyükleme Girdilerini Devre Dışı Bırak",
                Category = "Güvenlik",
                Hive = RegistryHive.LocalMachine,
                KeyPath = @"SYSTEM\CurrentControlSet\Control\SecureBoot",
                ValueName = "AvailableUpdates",
                EnabledValue = 0,
                DisabledValue = 1,
                ValueKind = RegistryValueKind.DWord,
                RequiresAdmin = true,
                RequiresRestart = true,
                Recommendation = "limited",
                RecommendationColor = "#FFC107",
                Description = "Güvenlik açığı içerebilecek eski Secure Boot önyükleme girdilerini kısıtlar. Yeniden başlatma gerektirir."
            }
        };

        public static List<TweakDefinition> GetAll()
        {
            var all = new List<TweakDefinition>();
            all.AddRange(GetCurrentUser());
            all.AddRange(GetLocalMachine());
            all.AddRange(GetAI());
            all.AddRange(GetSecureBoot());
            return all;
        }
    }
}
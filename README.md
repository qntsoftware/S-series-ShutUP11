<div align="center">

<img src="https://img.shields.io/badge/Windows-11-0078D4?style=for-the-badge&logo=windows11&logoColor=white"/>
<img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/WPF-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge"/>

<img src="https://r.resimlink.com/UnQmMuvFgT.png"/>

# 🛡 ShutUp11

**Windows 11 için Gizlilik & Sistem Kontrol Aracı**

*Telemetriyi kapat. Takibi engelle. Kontrolü geri al.*

[İndir](#kurulum) · [Özellikler](#özellikler) · [Katkıda Bulun](#katkıda-bulunma)

</div>

---

## Nedir?

ShutUp11, Windows 11'in gizliliğinizi ihlal eden özelliklerini tek tıkla devre dışı bırakmanızı sağlayan açık kaynaklı bir masaüstü uygulamasıdır. Her toggle gerçek Windows Registry değerlerini değiştirir — görsel numara değil.

- **Registry tabanlı** — Değişiklikler kalıcıdır, uygulama kapalıyken de geçerlidir
- **Yedek sistemi** — Her değişiklikten önce eski değer `%AppData%\ShutUp11\backup.json` dosyasına kaydedilir
- **Tek tıkla geri alma** — "Tümünü Geri Al" ile fabrika ayarlarına dönün
- **Admin koruması** — Yetki gerektiren ayarlar, admin olmadan çalıştırıldığında kilitlenir

---

## Özellikler

### 🔒 Current User — Gizlilik (HKCU)

| Ayar | Registry Yolu | Değer |
|------|--------------|-------|
| Reklam Kimliğini Devre Dışı Bırak | `HKCU\...\AdvertisingInfo` | `Enabled = 0` |
| Yazma Bilgisi İletimini Devre Dışı Bırak | `HKCU\...\Input\TIPC` | `Enabled = 0` |
| Başlat Menüsü Önerilerini Kapat | `HKCU\...\ContentDeliveryManager` | `...338388Enabled = 0` |
| Zaman Tüneli Önerilerini Kapat | `HKCU\...\ContentDeliveryManager` | `...338387Enabled = 0` |
| İpuçları & Püf Noktaları Bildirimlerini Kapat | `HKCU\...\ContentDeliveryManager` | `...338389Enabled = 0` |
| Ayarlar Uygulaması Önerilerini Kapat | `HKCU\...\ContentDeliveryManager` | `...353694Enabled = 0` |
| Cihaz Kurulum Önerilerini Kapat | `HKCU\...\ContentDeliveryManager` | `...338393Enabled = 0` |
| Uygulama Bildirimlerini Devre Dışı Bırak | `HKCU\...\PushNotifications` | `ToastEnabled = 0` |
| Tarayıcı Dil Erişimini Kapat | `HKCU\Control Panel\International\User Profile` | `HttpAcceptLanguageOptOut = 1` |
| Klavye Metin Önerilerini Kapat | `HKCU\...\Input\Settings` | `EnableHwkbTextPrediction = 0` |

### 🖥 Local Machine — Telemetri & Sistem (HKLM) `🛡 Admin`

| Ayar | Açıklama |
|------|---------|
| Telemetri Veri Toplamayı Devre Dışı Bırak | `AllowTelemetry = 0` — Veri gönderimi tamamen durur |
| Cortana'yı Devre Dışı Bırak | `AllowCortana = 0` — Bulut arama kapatılır |
| OneDrive Entegrasyonunu Devre Dışı Bırak | `DisableFileSyncNGSC = 1` — Gezgin entegrasyonu kaldırılır |
| Defender Örnek Gönderimini Durdur | `SubmitSamplesConsent = 2` — Otomatik numune gönderimi engellenir |
| Bulut Tabanlı Korumayı Kapat | `SpynetReporting = 0` |
| DiagTrack Servisini Durdur | Servis: `Stopped` + `StartType = Disabled` |
| dmwappushservice Servisini Durdur | WAP yönlendirme servisi durdurulur |

### 🤖 Yapay Zeka (AI)

| Ayar | Açıklama |
|------|---------|
| Windows Copilot'u Devre Dışı Bırak | Görev çubuğundan tamamen kaldırılır |
| Windows Recall'u Devre Dışı Bırak | Ekran analizi ve anlık görüntü sistemi kapatılır |
| Paint'te AI İçerik Üretimini Kapat | Generative Fill devre dışı |
| Notepad'de AI Özelliklerini Kapat | AI yazma yardımı kapatılır |

### 🔐 Secure Boot

- Secure Boot durumu görüntülenir (salt okunur)
- Eski önyükleme girdileri kısıtlanabilir

---

## Mimari

```
ShutUp11/
├── Models/
│   ├── TweakDefinition.cs     # Her tweakin tam tanımı
│   ├── TweakResult.cs         # İşlem sonucu (Success/Message/RequiresRestart)
│   └── TweakItem.cs           # (Eski — Obsolete)
│
├── Services/
│   ├── TweakEngine.cs         # Okuma/yazma motoru (thread-safe)
│   ├── TweakBackup.cs         # JSON yedek sistemi
│   ├── RegistryHelper.cs      # Düşük seviye registry erişimi
│   ├── SettingsProvider.cs    # 21 gerçek tweak tanımı
│   ├── ServiceProvider.cs     # Windows servis kontrolü (WMI)
│   └── AdminHelper.cs         # UAC / admin kontrolü
│
├── Views/
│   ├── SettingsView.xaml(.cs) # Toggle listesi + TweakEngine entegrasyonu
│   ├── SystemMonitorView      # Süreç & servis izleyici
│   └── ...
│
└── Controls/
    └── ToggleSwitch.xaml(.cs) # Animasyonlu toggle (cancel destekli)
```

### Veri Akışı

```
Kullanıcı toggle tıklar
        ↓
ToggleSwitch.Toggled event
        ↓
TweakEngine.ApplyTweak(tweak, enable)
        ↓
TweakBackup.RecordBackup()   <-- Önce yedek al
        ↓
RegistryHelper.WriteValue()  <-- Registrye yaz
        ↓
Başarısız: e.Cancel = true   <-- Toggle eski konuma döner
Başarılı:  VM güncellenir    <-- UI yansır
```

---

## Kurulum

### Gereksinimler

- Windows 11 (Windows 10 kısmen desteklenir)
- .NET 8.0 Runtime
- Yönetici (Administrator) yetkisi — HKLM ayarları için

### Kaynaktan Derleme

```bash
git clone https://github.com/qntsoftware/ShutUp11.git
cd ShutUp11
dotnet build --configuration Release
```

> **Not:** HKLM (Local Machine) ayarları ve servis durdurma işlemleri için uygulamayı
> **Yönetici olarak çalıştırın** (Sağ tık → Yönetici olarak çalıştır)

---

## Kullanım

### Toggle Renkleri

- **Yeşil (açık)** → Ayar etkin (gizlilik koruması aktif)
- **Kırmızı (kapalı)** → Ayar devre dışı (Windows varsayılanı)

### İkonlar

| İkon | Anlam |
|------|-------|
| 🛡 | Yönetici yetkisi gerektirir |
| 🔄 | Değişiklik için yeniden başlatma gerektirir |
| ℹ | Yalnızca bilgi — değiştirilemez |
| ⚙ | Servis bazlı işlem |

### Menüler

| Menü | İşlev |
|------|-------|
| **Dosya → Çıkış** | Uygulamayı kapat |
| **İşlemler → Önerilen Ayarları Uygula** | Tüm sekmelerde "yes" etiketli ayarları uygular |
| **İşlemler → Yedek Dışa Aktar** | backup.json dosyasını istenen konuma kaydeder |
| **İşlemler → Tümünü Geri Al** | Tüm değişiklikleri fabrika değerlerine döndürür |
| **Araçlar → Kayıt Defteri Düzenleyicisi** | regedit.exe açar |
| **Araçlar → Servisler** | services.msc açar |
| **Araçlar → Görev Yöneticisi** | taskmgr.exe açar |

### Yedek Sistemi

Her değişiklik öncesi eski değer otomatik yedeklenir:

```json
{
  "backups": [
    {
      "tweakId": "advertising_id",
      "timestamp": "2026-09-27T15:30:00Z",
      "hive": "CurrentUser",
      "keyPath": "Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo",
      "valueName": "Enabled",
      "previousValue": 1,
      "valueKind": "DWord",
      "valueExisted": true
    }
  ]
}
```

**Yedek konumu:** `%AppData%\ShutUp11\backup.json`

---

## Güvenlik

- Uygulama **hiçbir veriyi internete göndermez**
- Tüm işlemler yerel Windows API üzerinden gerçekleşir
- Kaynak kodu açıktır, her satır incelenebilir
- Registry değişiklikleri geri alınabilir

---

## Katkıda Bulunma

Pull requestler memnuniyetle karşılanır!

1. Forklayın
2. Feature branch oluşturun: `git checkout -b feature/yeni-tweak`
3. Değişikliklerinizi commit edin
4. Branci push edin
5. Pull Request açın

### Yeni Tweak Eklemek

`Services/SettingsProvider.cs` dosyasına `TweakDefinition` ekleyin:

```csharp
new()
{
    Id = "benzersiz_id",
    Title = "Tweak Başlığı",
    Category = "Kategori",
    Hive = RegistryHive.CurrentUser,
    KeyPath = @"Software\...",
    ValueName = "ValueName",
    EnabledValue = 0,
    DisabledValue = 1,
    ValueKind = RegistryValueKind.DWord,
    RequiresAdmin = false,
    Recommendation = "yes",
    RecommendationColor = "#00C853",
    Description = "Bu tweak ne işe yarar."
}
```

---

## Lisans

[MIT License](LICENSE) © 2026 [qntsoftware](https://github.com/qntsoftware)

---

<div align="center">

*Gizliliğiniz bir hak, lütuf değil.*

</div>

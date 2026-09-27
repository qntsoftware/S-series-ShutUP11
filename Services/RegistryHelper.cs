using System;
using System.Security;
using Microsoft.Win32;
using ShutUp11.Models;

namespace ShutUp11.Services
{
    public static class RegistryHelper
    {
        public static RegistryKey? OpenBaseKey(RegistryHive hive, bool writable = false)
        {
            return hive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                RegistryHive.ClassesRoot => Registry.ClassesRoot,
                RegistryHive.Users => Registry.Users,
                RegistryHive.CurrentConfig => Registry.CurrentConfig,
                _ => null
            };
        }

        public static object? ReadValue(TweakDefinition tweak)
        {
            try
            {
                using var baseKey = OpenBaseKey(tweak.Hive);
                if (baseKey == null) return null;
                using var key = baseKey.OpenSubKey(tweak.KeyPath, false);
                return key?.GetValue(tweak.ValueName);
            }
            catch (SecurityException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch { return null; }
        }

        public static TweakResult WriteValue(TweakDefinition tweak, object value)
        {
            try
            {
                using var baseKey = OpenBaseKey(tweak.Hive, true);
                if (baseKey == null) return TweakResult.Fail("Registry ana anahtarı açılamadı.");

                using var key = baseKey.CreateSubKey(tweak.KeyPath, true);
                if (key == null) return TweakResult.Fail($"Registry anahtarı oluşturulamadı: {tweak.KeyPath}");

                key.SetValue(tweak.ValueName, value, tweak.ValueKind);
                key.Close();
                return TweakResult.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return TweakResult.Fail("Erişim reddedildi. Bu ayar için Yönetici yetkisi gerekiyor.");
            }
            catch (SecurityException)
            {
                return TweakResult.Fail("Güvenlik hatası. Bu ayar için Yönetici yetkisi gerekiyor.");
            }
            catch (Exception ex)
            {
                return TweakResult.Fail($"Registry yazma hatası: {ex.Message}");
            }
        }

        public static bool ValueExists(TweakDefinition tweak)
        {
            try
            {
                using var baseKey = OpenBaseKey(tweak.Hive);
                if (baseKey == null) return false;
                using var key = baseKey.OpenSubKey(tweak.KeyPath, false);
                return key?.GetValue(tweak.ValueName) != null;
            }
            catch { return false; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using ShutUp11.Models;

namespace ShutUp11.Services
{
    public static class TweakEngine
    {
        private static readonly SemaphoreSlim WriteLock = new(1, 1);

        public static bool ReadCurrentState(TweakDefinition tweak)
        {
            if (tweak.InfoOnly) return false;

            if (tweak.IsServiceAction && !string.IsNullOrEmpty(tweak.ServiceName))
            {
                try
                {
                    using var sc = new System.ServiceProcess.ServiceController(tweak.ServiceName);
                    return sc.Status == System.ServiceProcess.ServiceControllerStatus.Running;
                }
                catch { return false; }
            }

            var raw = RegistryHelper.ReadValue(tweak);
            if (raw == null) return false;

            return ValuesEqual(raw, tweak.EnabledValue);
        }

        public static TweakResult ApplyTweak(TweakDefinition tweak, bool enable)
        {
            if (tweak.InfoOnly)
                return TweakResult.Fail("Bu ayar yalnızca bilgi amaçlıdır ve değiştirilemez.");

            if (tweak.RequiresAdmin && !AdminHelper.IsRunningAsAdmin())
                return TweakResult.Fail("Bu ayar Yönetici yetkisi gerektiriyor.\n\nLütfen ShutUp11'i Yönetici olarak yeniden başlatın.");

            WriteLock.Wait();
            try
            {
                if (tweak.IsServiceAction && !string.IsNullOrEmpty(tweak.ServiceName))
                    return ApplyServiceTweak(tweak, enable);

                return ApplyRegistryTweak(tweak, enable);
            }
            finally
            {
                WriteLock.Release();
            }
        }

        private static TweakResult ApplyRegistryTweak(TweakDefinition tweak, bool enable)
        {
            var existed = RegistryHelper.ValueExists(tweak);
            var previousValue = RegistryHelper.ReadValue(tweak);

            TweakBackup.RecordBackup(tweak, previousValue, existed);

            var valueToWrite = enable ? tweak.EnabledValue : tweak.DisabledValue;
            var result = RegistryHelper.WriteValue(tweak, valueToWrite);

            if (!result.Success)
            {
                return result;
            }

            return TweakResult.Ok(previousValue, existed, tweak.RequiresRestart);
        }

        private static TweakResult ApplyServiceTweak(TweakDefinition tweak, bool enable)
        {
            try
            {
                TweakBackup.RecordBackup(tweak, null, false);

                bool ok = ServiceProvider.SetRunning(tweak.ServiceName!, enable);
                if (!ok && !enable)
                {
                }

                ServiceProvider.SetStartType(tweak.ServiceName!,
                    enable
                        ? System.ServiceProcess.ServiceStartMode.Automatic
                        : System.ServiceProcess.ServiceStartMode.Disabled);

                return TweakResult.Ok(null, false, false);
            }
            catch (Exception ex)
            {
                return TweakResult.Fail($"Servis işlemi başarısız: {ex.Message}");
            }
        }

        public static TweakResult RevertTweak(TweakDefinition tweak)
        {
            if (!TweakBackup.HasBackup(tweak.Id))
                return TweakResult.Fail("Bu tweak için yedek bulunamadı.");

            var previousValue = TweakBackup.GetBackup(tweak.Id);

            WriteLock.Wait();
            try
            {
                if (previousValue == null)
                {
                    try
                    {
                        using var baseKey = RegistryHelper.OpenBaseKey(tweak.Hive, true);
                        using var key = baseKey?.OpenSubKey(tweak.KeyPath, true);
                        key?.DeleteValue(tweak.ValueName, false);
                        return TweakResult.Ok();
                    }
                    catch (Exception ex)
                    {
                        return TweakResult.Fail($"Değer silinirken hata: {ex.Message}");
                    }
                }

                return RegistryHelper.WriteValue(tweak, previousValue);
            }
            finally
            {
                WriteLock.Release();
            }
        }

        public static List<TweakResult> ApplyRecommended(List<TweakDefinition> tweaks)
        {
            var results = new List<TweakResult>();
            foreach (var tweak in tweaks)
            {
                if (tweak.InfoOnly) continue;
                if (tweak.Recommendation == "yes")
                    results.Add(ApplyTweak(tweak, true));
            }
            return results;
        }

        private static bool ValuesEqual(object a, object b)
        {
            if (a == null || b == null) return false;
            try
            {
                return Convert.ToInt64(a) == Convert.ToInt64(b);
            }
            catch
            {
                return a.ToString() == b.ToString();
            }
        }
    }
}

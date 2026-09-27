using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ShutUp11.Models;

namespace ShutUp11.Services
{
    public static class TweakBackup
    {
        private static readonly string BackupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShutUp11");
        private static readonly string BackupPath = Path.Combine(BackupDir, "backup.json");
        private static readonly object FileLock = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void RecordBackup(TweakDefinition tweak, object? previousValue, bool existed)
        {
            lock (FileLock)
            {
                var store = LoadStore();
                store.Backups.RemoveAll(b => b.TweakId == tweak.Id);
                store.Backups.Add(new BackupEntry
                {
                    TweakId = tweak.Id,
                    Timestamp = DateTime.UtcNow.ToString("O"),
                    Hive = tweak.Hive.ToString(),
                    KeyPath = tweak.KeyPath,
                    ValueName = tweak.ValueName,
                    PreviousValue = previousValue is null ? null : JsonSerializer.SerializeToElement(previousValue),
                    ValueKind = tweak.ValueKind.ToString(),
                    ValueExisted = existed
                });
                SaveStore(store);
            }
        }

        public static object? GetBackup(string tweakId)
        {
            lock (FileLock)
            {
                var entry = LoadStore().Backups.FirstOrDefault(b => b.TweakId == tweakId);
                if (entry == null) return null;
                if (!entry.ValueExisted) return null;
                if (entry.PreviousValue is JsonElement el) return ExtractJsonValue(el);
                return null;
            }
        }

        public static bool HasBackup(string tweakId)
        {
            lock (FileLock)
            {
                return LoadStore().Backups.Any(b => b.TweakId == tweakId);
            }
        }

        public static DateTime? GetBackupTime(string tweakId)
        {
            lock (FileLock)
            {
                var entry = LoadStore().Backups.FirstOrDefault(b => b.TweakId == tweakId);
                if (entry == null) return null;
                if (DateTime.TryParse(entry.Timestamp, out var dt)) return dt.ToLocalTime();
                return null;
            }
        }

        public static void RestoreAll(List<TweakDefinition> allTweaks)
        {
            lock (FileLock)
            {
                var store = LoadStore();
                foreach (var entry in store.Backups)
                {
                    var tweak = allTweaks.FirstOrDefault(t => t.Id == entry.TweakId);
                    if (tweak == null) continue;

                    if (!entry.ValueExisted)
                    {
                        try
                        {
                            using var baseKey = RegistryHelper.OpenBaseKey(tweak.Hive, true);
                            using var key = baseKey?.OpenSubKey(tweak.KeyPath, true);
                            key?.DeleteValue(tweak.ValueName, false);
                        }
                        catch { }
                    }
                    else
                    {
                        object? val = entry.PreviousValue is JsonElement el ? ExtractJsonValue(el) : null;
                        if (val != null) RegistryHelper.WriteValue(tweak, val);
                    }
                }
                store.Backups.Clear();
                SaveStore(store);
            }
        }

        public static void ClearBackups()
        {
            lock (FileLock)
            {
                SaveStore(new BackupStore());
            }
        }

        public static string ExportJson()
        {
            lock (FileLock)
            {
                var store = LoadStore();
                return JsonSerializer.Serialize(store, JsonOptions);
            }
        }

        private static BackupStore LoadStore()
        {
            try
            {
                if (!File.Exists(BackupPath)) return new BackupStore();
                var json = File.ReadAllText(BackupPath);
                return JsonSerializer.Deserialize<BackupStore>(json) ?? new BackupStore();
            }
            catch { return new BackupStore(); }
        }

        private static void SaveStore(BackupStore store)
        {
            try
            {
                Directory.CreateDirectory(BackupDir);
                File.WriteAllText(BackupPath, JsonSerializer.Serialize(store, JsonOptions));
            }
            catch { }
        }

        private static object? ExtractJsonValue(JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.Number when el.TryGetInt64(out long l) => (int)l,
                JsonValueKind.String => el.GetString(),
                JsonValueKind.True => 1,
                JsonValueKind.False => 0,
                _ => null
            };
        }

        private class BackupStore
        {
            [JsonPropertyName("backups")]
            public List<BackupEntry> Backups { get; set; } = new();
        }

        private class BackupEntry
        {
            [JsonPropertyName("tweakId")] public string TweakId { get; set; } = string.Empty;
            [JsonPropertyName("timestamp")] public string Timestamp { get; set; } = string.Empty;
            [JsonPropertyName("hive")] public string Hive { get; set; } = string.Empty;
            [JsonPropertyName("keyPath")] public string KeyPath { get; set; } = string.Empty;
            [JsonPropertyName("valueName")] public string ValueName { get; set; } = string.Empty;
            [JsonPropertyName("previousValue")] public JsonElement? PreviousValue { get; set; }
            [JsonPropertyName("valueKind")] public string ValueKind { get; set; } = string.Empty;
            [JsonPropertyName("valueExisted")] public bool ValueExisted { get; set; }
        }
    }
}

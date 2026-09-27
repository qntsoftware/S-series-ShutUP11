using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using ShutUp11.Models;

namespace ShutUp11.Services
{
    public static class ProcessProvider
    {
        private static readonly Dictionary<int, (TimeSpan Cpu, DateTime Ts)> _cpuCache = new();
        private static readonly object _cpuLock = new();

        public static List<ProcessItem> GetAll()
        {
            var list = new List<ProcessItem>();
            var now = DateTime.UtcNow;
            int cpuCount = Environment.ProcessorCount;
            var seenPids = new HashSet<int>();

            foreach (var p in Process.GetProcesses())
            {
                seenPids.Add(p.Id);

                try
                {
                    var item = new ProcessItem
                    {
                        Pid = p.Id,
                        Name = p.ProcessName + ".exe",
                        MemoryBytes = SafeGet(() => p.WorkingSet64, 0L),
                        ThreadCount = SafeGet(() => p.Threads.Count, 0),
                        HandleCount = SafeGet(() => p.HandleCount, 0),
                        StartTime = SafeGet(() => p.StartTime, DateTime.MinValue)
                    };

                    try { item.Path = p.MainModule?.FileName ?? ""; } catch { }

                    if (!string.IsNullOrEmpty(item.Path) && File.Exists(item.Path))
                    {
                        try
                        {
                            var fvi = FileVersionInfo.GetVersionInfo(item.Path);
                            item.Publisher = fvi.CompanyName ?? "Unknown";
                            item.Description = fvi.FileDescription ?? "";
                            item.Product = fvi.ProductName ?? "";
                            item.Version = fvi.FileVersion ?? "";
                        }
                        catch { }
                    }

                    item.CpuPercent = ComputeCpu(p, item.Pid, now, cpuCount);
                    ClassifyProcess(item);
                    list.Add(item);
                }
                catch { }
            }

            lock (_cpuLock)
            {
                var dead = _cpuCache.Keys.Where(k => !seenPids.Contains(k)).ToList();
                foreach (var k in dead) _cpuCache.Remove(k);
            }

            return list;
        }

        private static T SafeGet<T>(Func<T> f, T def)
        {
            try { return f(); } catch { return def; }
        }

        private static double ComputeCpu(Process p, int pid, DateTime now, int cpuCount)
        {
            try
            {
                var total = p.TotalProcessorTime;

                lock (_cpuLock)
                {
                    if (_cpuCache.TryGetValue(pid, out var prev))
                    {
                        var cpuDelta = (total - prev.Cpu).TotalMilliseconds;
                        var timeDelta = (now - prev.Ts).TotalMilliseconds;
                        _cpuCache[pid] = (total, now);

                        if (timeDelta <= 0) return 0;
                        double pct = cpuDelta / (timeDelta * cpuCount) * 100;
                        if (pct < 0) pct = 0;
                        if (pct > 100) pct = 100;
                        return Math.Round(pct, 1);
                    }

                    _cpuCache[pid] = (total, now);
                }
            }
            catch { }
            return 0;
        }

        private static void ClassifyProcess(ProcessItem item)
        {
            if (item.Pid == 0 || item.Pid == 4)
            {
                item.Status = "System";
                item.StatusColor = "#00C853";
                return;
            }

            if (string.IsNullOrEmpty(item.Path))
            {
                item.Status = "Protected";
                item.StatusColor = "#9A9A9E";
                return;
            }

            bool systemPath =
                item.Path.StartsWith(@"C:\Windows\", StringComparison.OrdinalIgnoreCase) ||
                item.Path.StartsWith(@"C:\Program Files\", StringComparison.OrdinalIgnoreCase) ||
                item.Path.StartsWith(@"C:\Program Files (x86)\", StringComparison.OrdinalIgnoreCase);

            bool signed = !string.IsNullOrEmpty(item.Publisher) && item.Publisher != "Unknown";

            if (systemPath && signed)
            {
                item.Status = "System";
                item.StatusColor = "#00C853";
            }
            else if (signed)
            {
                item.Status = "3rd-Party";
                item.StatusColor = "#B84DFF";
            }
            else if (!systemPath && !signed)
            {
                item.Status = "Suspicious";
                item.StatusColor = "#FFD93D";
            }
            else
            {
                item.Status = "Unknown";
                item.StatusColor = "#9A9A9E";
            }
        }

        public static ProcessDetail GetDetail(int pid)
        {
            var d = new ProcessDetail();

            Process p;
            try { p = Process.GetProcessById(pid); }
            catch { return d; }

            try
            {
                string exePath = "";
                try { exePath = p.MainModule?.FileName ?? ""; } catch { }

                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    try
                    {
                        var fvi = FileVersionInfo.GetVersionInfo(exePath);
                        d.Description = fvi.FileDescription ?? "";
                        d.Product = fvi.ProductName ?? "";
                        d.Version = fvi.FileVersion ?? "";
                        d.Signature = fvi.CompanyName ?? "Unknown";
                        d.IsSigned = !string.IsNullOrEmpty(fvi.CompanyName);
                    }
                    catch { }
                }

                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        d.CommandLine = obj["CommandLine"]?.ToString() ?? "";
                        break;
                    }
                }
                catch { }

                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        try
                        {
                            using var inParams = obj.GetMethodParameters("GetOwner");
                            using var outParams = obj.InvokeMethod("GetOwner", inParams, null);

                            string user = outParams?["User"] as string ?? "";
                            string domain = outParams?["Domain"] as string ?? "";

                            if (!string.IsNullOrEmpty(user))
                            {
                                d.UserName = string.IsNullOrEmpty(domain)
                                    ? user
                                    : $"{domain}\\{user}";
                            }
                        }
                        catch { }
                        break;
                    }
                }
                catch { }

                try
                {
                    foreach (ProcessModule m in p.Modules)
                    {
                        try
                        {
                            var info = new DllInfo
                            {
                                Name = m.ModuleName,
                                Path = m.FileName
                            };

                            try
                            {
                                var fvi = FileVersionInfo.GetVersionInfo(m.FileName);
                                info.Publisher = fvi.CompanyName ?? "";
                                info.IsSigned = !string.IsNullOrEmpty(fvi.CompanyName);
                            }
                            catch { }

                            d.Modules.Add(info);
                        }
                        catch { }
                    }
                }
                catch { }

                try
                {
                    foreach (ProcessThread t in p.Threads)
                    {
                        try
                        {
                            d.Threads.Add(new ThreadInfo
                            {
                                Id = t.Id,
                                State = t.ThreadState.ToString(),
                                WaitReason = t.WaitReason.ToString(),
                                Priority = t.CurrentPriority
                            });
                        }
                        catch { }
                    }
                }
                catch { }
            }
            catch { }

            return d;
        }
    }
}
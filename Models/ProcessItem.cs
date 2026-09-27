using System;

namespace ShutUp11.Models
{
    public class ProcessItem
    {
        public int Pid { get; set; }
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public long MemoryBytes { get; set; }
        public string MemoryText => $"{MemoryBytes / 1024 / 1024} MB";
        public string Publisher { get; set; } = "Unknown";
        public string Description { get; set; } = "";
        public string Product { get; set; } = "";
        public string Version { get; set; } = "";
        public string Status { get; set; } = "Normal";
        public string StatusColor { get; set; } = "#00C853";
        public double CpuPercent { get; set; }
        public string CpuText => $"{CpuPercent:0.0}%";
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public DateTime StartTime { get; set; }
    }
}
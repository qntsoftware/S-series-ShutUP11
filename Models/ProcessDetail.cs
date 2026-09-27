using System.Collections.Generic;

namespace ShutUp11.Models
{
    public class ProcessDetail
    {
        public List<DllInfo> Modules { get; set; } = new();
        public List<ThreadInfo> Threads { get; set; } = new();
        public string CommandLine { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Product { get; set; } = "";
        public string Version { get; set; } = "";
        public string Signature { get; set; } = "Unknown";
        public bool IsSigned { get; set; }
    }

    public class DllInfo
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public bool IsSigned { get; set; }
        public string Publisher { get; set; } = "";
    }

    public class ThreadInfo
    {
        public int Id { get; set; }
        public string State { get; set; } = "";
        public string WaitReason { get; set; } = "";
        public int Priority { get; set; }
    }
}
using Microsoft.Win32;

namespace ShutUp11.Models
{
    public class TweakDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public RegistryHive Hive { get; set; } = RegistryHive.CurrentUser;
        public string KeyPath { get; set; } = string.Empty;
        public string ValueName { get; set; } = string.Empty;
        public object EnabledValue { get; set; } = 0;
        public object DisabledValue { get; set; } = 1;
        public RegistryValueKind ValueKind { get; set; } = RegistryValueKind.DWord;
        public bool RequiresAdmin { get; set; }
        public bool RequiresRestart { get; set; }
        public bool IsServiceAction { get; set; }
        public string? ServiceName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = "yes";
        public string RecommendationColor { get; set; } = "#00C853";
        public bool InfoOnly { get; set; }
    }
}

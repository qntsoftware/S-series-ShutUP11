namespace ShutUp11.Models
{
    public class ServiceItem
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool IsRunning { get; set; }
        public string StartType { get; set; } = "";
        public bool SendsData { get; set; }
        public string Tag { get; set; } = "";
    }
}
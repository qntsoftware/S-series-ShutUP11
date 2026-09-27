namespace ShutUp11.Models
{
    [System.Obsolete("TweakItem yerine TweakDefinition kullanın.")]
    public class TweakItem
    {
        public string Title { get; set; } = "";
        public bool IsEnabled { get; set; }
        public string Recommendation { get; set; } = "yes";
        public string RecommendationColor { get; set; } = "#00C853";
    }
}

namespace ShutUp11.Models
{
    public class TweakResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresRestart { get; set; }
        public bool RequiresLogoff { get; set; }
        public object? PreviousValue { get; set; }
        public bool ValueExisted { get; set; }

        public static TweakResult Ok(object? previousValue = null, bool existed = false, bool requiresRestart = false)
            => new() { Success = true, Message = "Başarıyla uygulandı.", PreviousValue = previousValue, ValueExisted = existed, RequiresRestart = requiresRestart };

        public static TweakResult Fail(string message)
            => new() { Success = false, Message = message };
    }
}

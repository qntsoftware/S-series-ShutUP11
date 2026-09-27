using System;

namespace ShutUp11.Controls
{
    public class ToggleEventArgs : EventArgs
    {
        public bool NewValue { get; }
        public bool Cancel { get; set; }

        public ToggleEventArgs(bool newValue)
        {
            NewValue = newValue;
        }
    }
}
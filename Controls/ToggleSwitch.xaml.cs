using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace ShutUp11.Controls
{
    public partial class ToggleSwitch : UserControl
    {
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(ToggleSwitch),
                new PropertyMetadata(false, OnIsOnChanged));

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        public event EventHandler<ToggleEventArgs>? Toggled;

        private bool _suppressEvent;

        public ToggleSwitch()
        {
            InitializeComponent();
            Loaded += (s, e) => Animate(IsOn, false);
            MouseLeftButtonDown += OnToggleClicked;
        }

        private void OnToggleClicked(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            bool newValue = !IsOn;
            var args = new ToggleEventArgs(newValue);

            if (!_suppressEvent)
                Toggled?.Invoke(this, args);

            if (args.Cancel)
            {
                Animate(IsOn, true);
                return;
            }

            _suppressEvent = true;
            IsOn = newValue;
            _suppressEvent = false;

            Animate(newValue, true);
        }

        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ToggleSwitch ts) return;
            if (ts._suppressEvent) return;
            ts.Animate((bool)e.NewValue, true);
        }

        public void SetStateSilently(bool value)
        {
            _suppressEvent = true;
            IsOn = value;
            _suppressEvent = false;
            Animate(value, true);
        }

        private void Animate(bool on, bool animated)
        {
            var duration = animated ? TimeSpan.FromMilliseconds(220) : TimeSpan.Zero;
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var colorAnim = new ColorAnimation
            {
                To = on ? Color.FromRgb(0, 200, 83) : Color.FromRgb(230, 74, 25),
                Duration = duration
            };
            TrackBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            var thumbAnim = new ThicknessAnimation
            {
                To = on ? new Thickness(25, 0, 0, 0) : new Thickness(3, 0, 0, 0),
                Duration = duration,
                EasingFunction = ease
            };
            Thumb.BeginAnimation(MarginProperty, thumbAnim);

            var glowAnim = new DoubleAnimation
            {
                To = on ? 14 : 0,
                Duration = duration
            };
            Glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnim);
        }
    }
}
using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace EMS_PJT_Hamburger.Behaviors
{
    public static class TouchKeyboardService
    {
        public static readonly DependencyProperty ShowOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "ShowOnFocus",
                typeof(bool),
                typeof(TouchKeyboardService),
                new PropertyMetadata(false, OnShowOnFocusChanged));

        private static TouchKeypadWindow _keypadWindow;
        private static DateTime _lastOpenAttemptUtc = DateTime.MinValue;
        private static DateTime _suppressOpenUntilUtc = DateTime.MinValue;

        public static bool IsEnabled { get; private set; } = true;

        public static void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            if (!isEnabled && _keypadWindow != null)
            {
                SuppressOpenFor(TimeSpan.FromMilliseconds(300));
                _keypadWindow.Hide();
            }
        }

        internal static void SuppressOpenFor(TimeSpan duration)
        {
            _suppressOpenUntilUtc = DateTime.UtcNow.Add(duration);
        }

        public static bool GetShowOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowOnFocusProperty);
        }

        public static void SetShowOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowOnFocusProperty, value);
        }

        private static void OnShowOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element == null) return;

            if ((bool)e.NewValue)
            {
                element.PreviewMouseDown += OnInputPreviewMouseDown;
                element.GotKeyboardFocus += OnInputGotKeyboardFocus;
                element.TouchDown += OnInputTouchDown;
            }
            else
            {
                element.PreviewMouseDown -= OnInputPreviewMouseDown;
                element.GotKeyboardFocus -= OnInputGotKeyboardFocus;
                element.TouchDown -= OnInputTouchDown;
            }
        }

        private static void OnInputPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowKeypadAfterFocus(sender as FrameworkElement);
        }

        private static void OnInputTouchDown(object sender, TouchEventArgs e)
        {
            ShowKeypadAfterFocus(sender as FrameworkElement);
        }

        private static void OnInputGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ShowKeypadAfterFocus(sender as FrameworkElement);
        }

        private static void ShowKeypadAfterFocus(FrameworkElement element)
        {
            if (!IsEnabled) return;
            
            if (DateTime.UtcNow < _suppressOpenUntilUtc) return;
            if (element == null) return;
            if (!IsEditableElement(element)) return;

            element.Dispatcher.BeginInvoke(
                new Action(() => ShowKeypad(element)),
                DispatcherPriority.ApplicationIdle);
        }

        private static bool IsEditableElement(FrameworkElement element)
        {
            if (!element.IsEnabled || !element.IsVisible) return false;

            var readOnlyProperty = element.GetType().GetProperty("IsReadOnly");
            if (readOnlyProperty != null && readOnlyProperty.PropertyType == typeof(bool))
            {
                var isReadOnly = (bool)readOnlyProperty.GetValue(element, null);
                if (isReadOnly) return false;
            }

            return true;
        }

        private static void ShowKeypad(FrameworkElement target)
        {
            if (!IsEnabled) return;
            if (DateTime.UtcNow < _suppressOpenUntilUtc) return;

            var now = DateTime.UtcNow;
            if ((now - _lastOpenAttemptUtc).TotalMilliseconds < 100)
                return;

            _lastOpenAttemptUtc = now;

            if (_keypadWindow == null)
            {
                _keypadWindow = new TouchKeypadWindow();
                _keypadWindow.Closed += (s, e) => _keypadWindow = null;
            }

            _keypadWindow.AttachTarget(target);
        }
    }

    internal sealed class TouchKeypadWindow : Window
    {
        private readonly TextBox _display;
        private FrameworkElement _target;
        private DependencyProperty _targetEditValueProperty;

        public TouchKeypadWindow()
        {
            Width = 340;
            Height = 390;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            Topmost = true;
            Background = new SolidColorBrush(Color.FromRgb(26, 31, 40));
            BorderBrush = new SolidColorBrush(Color.FromRgb(95, 110, 130));
            BorderThickness = new Thickness(1);
            Padding = new Thickness(10);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            _display = new TextBox
            {
                Height = 48,
                IsReadOnly = true,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 24,
                TextAlignment = TextAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(12, 16, 23)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(95, 110, 130))
            };
            root.Children.Add(_display);

            var keypad = new Grid();
            Grid.SetRow(keypad, 1);
            for (int i = 0; i < 5; i++)
                keypad.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 4; i++)
                keypad.ColumnDefinitions.Add(new ColumnDefinition());

            AddButton(keypad, "7", 0, 0);
            AddButton(keypad, "8", 0, 1);
            AddButton(keypad, "9", 0, 2);
            AddButton(keypad, "Back", 0, 3);
            AddButton(keypad, "4", 1, 0);
            AddButton(keypad, "5", 1, 1);
            AddButton(keypad, "6", 1, 2);
            AddButton(keypad, "Clear", 1, 3);
            AddButton(keypad, "1", 2, 0);
            AddButton(keypad, "2", 2, 1);
            AddButton(keypad, "3", 2, 2);
            AddButton(keypad, "-", 2, 3);
            AddButton(keypad, "0", 3, 0, 1, 2);
            AddButton(keypad, ".", 3, 2);
            AddButton(keypad, "OK", 3, 3, 2, 1);
            AddButton(keypad, "Close", 4, 0, 1, 3);

            root.Children.Add(keypad);
            Content = root;
        }

        public void AttachTarget(FrameworkElement target)
        {
            _target = target;
            _targetEditValueProperty = FindDependencyProperty(target.GetType(), "EditValueProperty");
            //_display.Text = GetTargetText();
            _display.Text = string.Empty;
            PositionAtApplicationCenter();
            ClearTargetFocus(target);

            if (!IsVisible)
                Show();

            Activate();
            _display.Focus();
        }

        private void AddButton(Grid grid, string text, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            var button = new Button
            {
                Content = text,
                Margin = new Thickness(4),
                FontSize = text.Length > 1 ? 18 : 24,
                FontWeight = FontWeights.SemiBold,
                Background = new SolidColorBrush(Color.FromRgb(43, 52, 65)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(90, 108, 130))
            };

            button.Click += (s, e) => HandleKey(text);
            Grid.SetRow(button, row);
            Grid.SetColumn(button, column);
            Grid.SetRowSpan(button, rowSpan);
            Grid.SetColumnSpan(button, columnSpan);
            grid.Children.Add(button);
        }

        private void HandleKey(string key)
        {
            var text = _display.Text ?? string.Empty;

            if (key == "OK" || key == "Close")
            {
                TouchKeyboardService.SuppressOpenFor(TimeSpan.FromMilliseconds(300));
                Keyboard.ClearFocus();
                Hide();
                return;
            }

            if (key == "Clear")
            {
                SetText(string.Empty);
                return;
            }

            if (key == "Back")
            {
                if (text.Length > 0)
                    SetText(text.Substring(0, text.Length - 1));
                return;
            }

            if (key == "-")
            {
                if (text.StartsWith("-", StringComparison.Ordinal))
                    SetText(text.Substring(1));
                else
                    SetText("-" + text);
                return;
            }

            if (key == ".")
            {
                if (!text.Contains("."))
                    SetText(string.IsNullOrEmpty(text) ? "0." : text + ".");
                return;
            }

            SetText(text + key);
        }

        private void SetText(string value)
        {
            _display.Text = value;
            SetTargetValue(value);
        }

        private string GetTargetText()
        {
            if (_target == null) return string.Empty;

            var value = GetTargetValue();
            if (value == null) return string.Empty;

            var formattable = value as IFormattable;
            return formattable != null
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
        }

        private object GetTargetValue()
        {
            if (_target == null) return null;

            if (_targetEditValueProperty != null)
                return _target.GetValue(_targetEditValueProperty);

            var property = _target.GetType().GetProperty("EditValue");
            return property != null ? property.GetValue(_target, null) : null;
        }

        private void SetTargetValue(string value)
        {
            if (_target == null) return;

            if (_targetEditValueProperty != null)
            {
                _target.SetValue(_targetEditValueProperty, value);
                var binding = BindingOperations.GetBindingExpression(_target, _targetEditValueProperty);
                if (binding != null)
                    binding.UpdateSource();
                return;
            }

            var property = _target.GetType().GetProperty("EditValue");
            if (property != null && property.CanWrite)
                property.SetValue(_target, value, null);
        }

        private static DependencyProperty FindDependencyProperty(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (field != null)
                    return field.GetValue(null) as DependencyProperty;

                type = type.BaseType;
            }

            return null;
        }

        private static void ClearTargetFocus(FrameworkElement target)
        {
            if (target == null) return;

            var focusScope = FocusManager.GetFocusScope(target);
            if (focusScope != null)
                FocusManager.SetFocusedElement(focusScope, null);

            Keyboard.ClearFocus();
        }

        private void PositionAtApplicationCenter()
        {
            var owner = Application.Current != null ? Application.Current.MainWindow : null;
            if (owner != null && Owner == null && owner != this)
                Owner = owner;

            Rect targetArea;
            if (owner != null && owner.IsVisible)
            {
                var topLeft = owner.PointToScreen(new Point(0, 0));
                var source = PresentationSource.FromVisual(owner);
                if (source != null && source.CompositionTarget != null)
                    topLeft = source.CompositionTarget.TransformFromDevice.Transform(topLeft);

                targetArea = new Rect(topLeft.X, topLeft.Y, owner.ActualWidth, owner.ActualHeight);
            }
            else
            {
                targetArea = SystemParameters.WorkArea;
            }

            var workArea = SystemParameters.WorkArea;
            var left = targetArea.Left + ((targetArea.Width - Width) / 2);
            var top = targetArea.Top + ((targetArea.Height - Height) / 2);

            Left = Math.Max(workArea.Left, Math.Min(left, workArea.Right - Width));
            Top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - Height));
        }
    }
}

using ControlzEx.Native;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ControlzExWindow.Controls
{
    [TemplatePart(Name = "PART_Min", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Max", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Button))]
    public class WindowCommandsControl : Control
    {
        public event EventHandler<WindowClosingEventArgs> WindowClosing;

        public static readonly DependencyProperty MinimizeToolTipProperty =
            DependencyProperty.Register("MinimizeToolTip", typeof(string), typeof(WindowCommandsControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the minimize button tooltip.
        /// </summary>
        public string MinimizeToolTip
        {
            get => (string)GetValue(MinimizeToolTipProperty);
            set => SetValue(MinimizeToolTipProperty, value);
        }

        public static readonly DependencyProperty MaximizeToolTipProperty =
            DependencyProperty.Register("MaximizeToolTip", typeof(string), typeof(WindowCommandsControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the maximize button tooltip.
        /// </summary>
        public string MaximizeToolTip
        {
            get => (string)GetValue(MaximizeToolTipProperty);
            set => SetValue(MaximizeToolTipProperty, value);
        }

        public static readonly DependencyProperty CloseToolTipProperty =
            DependencyProperty.Register("CloseToolTip", typeof(string), typeof(WindowCommandsControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the close button tooltip.
        /// </summary>
        public string CloseToolTip
        {
            get => (string)GetValue(CloseToolTipProperty);
            set => SetValue(CloseToolTipProperty, value);
        }

        public static readonly DependencyProperty RestoreToolTipProperty =
            DependencyProperty.Register("RestoreToolTip", typeof(string), typeof(WindowCommandsControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the restore button tooltip.
        /// </summary>
        public string RestoreToolTip
        {
            get => (string)GetValue(RestoreToolTipProperty);
            set => SetValue(RestoreToolTipProperty, value);
        }

        private Button min;
        private Button max;
        private Button close;
#pragma warning disable 618
        private SafeLibraryHandle user32;
#pragma warning restore 618

        static WindowCommandsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowCommandsControl),
                new FrameworkPropertyMetadata(typeof(WindowCommandsControl)));
        }

        public WindowCommandsControl()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() =>
                {
                    if (string.IsNullOrWhiteSpace(this.MinimizeToolTip))
                    {
                        this.SetCurrentValue(MinimizeToolTipProperty, GetCaption(900));
                    }

                    if (string.IsNullOrWhiteSpace(this.MaximizeToolTip))
                    {
                        this.SetCurrentValue(MaximizeToolTipProperty, GetCaption(901));
                    }

                    if (string.IsNullOrWhiteSpace(this.CloseToolTip))
                    {
                        this.SetCurrentValue(CloseToolTipProperty, GetCaption(905));
                    }

                    if (string.IsNullOrWhiteSpace(this.RestoreToolTip))
                    {
                        this.SetCurrentValue(RestoreToolTipProperty, GetCaption(903));
                    }
                }));
        }

#pragma warning disable 618
        private string GetCaption(int id)
        {
            if (user32 == null)
            {
                user32 = UnsafeNativeMethods.LoadLibrary(Environment.SystemDirectory + "\\User32.dll");
            }

            var sb = new StringBuilder(256);
            UnsafeNativeMethods.LoadString(user32, (uint)id, sb, sb.Capacity);
            return sb.ToString().Replace("&", "");
        }
#pragma warning restore 618

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            close = Template.FindName("PART_Close", this) as Button;
            if (close != null)
            {
                close.Click += CloseClick;
            }

            max = Template.FindName("PART_Max", this) as Button;
            if (max != null)
            {
                max.Click += MaximizeClick;
            }

            min = Template.FindName("PART_Min", this) as Button;
            if (min != null)
            {
                min.Click += MinimizeClick;
            }

            _parentWindow = VisualTreeLocator.FindAncestor<CustomWindow>(this);
        }

        protected void OnWindowClosing(WindowClosingEventArgs args)
        {
            WindowClosing?.Invoke(this, args);
        }

#pragma warning disable 618

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            if (null == _parentWindow) return;
            ControlzEx.Windows.Shell.SystemCommands.MinimizeWindow(_parentWindow);
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (null == _parentWindow) return;
            if (_parentWindow.WindowState == WindowState.Maximized)
            {
                ControlzEx.Windows.Shell.SystemCommands.RestoreWindow(_parentWindow);
            }
            else
            {
                ControlzEx.Windows.Shell.SystemCommands.MaximizeWindow(_parentWindow);
            }
        }
#pragma warning restore 618

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            var closingWindowEventHandlerArgs = new WindowClosingEventArgs();
            OnWindowClosing(closingWindowEventHandlerArgs);

            if (closingWindowEventHandlerArgs.Cancel)
            {
                return;
            }

            _parentWindow?.Close();
        }

        private CustomWindow _parentWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
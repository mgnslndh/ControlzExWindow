using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlzEx.Behaviors;
using ControlzEx.Native;
using ControlzEx.Standard;
using JetBrains.Annotations;
using Microsoft.Xaml.Behaviors;

namespace ControlzExWindow.Controls
{
    [TemplatePart(Name = PART_WindowTitleThumb, Type = typeof(Thumb))]
    [TemplatePart(Name = PART_WindowTitleBorder, Type = typeof(Border))]
    [TemplatePart(Name = PART_WindowTitleBackground, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_TitleBar, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_TitleBarIcon, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_TitleBarWindowCommands, Type = typeof(WindowCommandsControl))]
    public class CustomWindow : Window
    {
        private const string PART_WindowTitleThumb = "PART_WindowTitleThumb";
        private const string PART_WindowTitleBorder = "PART_WindowTitleBorder";
        private const string PART_WindowTitleBackground = "PART_WindowTitleBackground";
        private const string PART_TitleBar = "PART_TitleBar";
        private const string PART_TitleBarIcon = "PART_TitleBarIcon";
        private const string PART_TitleBarWindowCommands = "PART_TitleBarWindowCommands";

        private Thumb windowTitleThumb;
        private Border windowTitleBorder;
        private UIElement titleBar;
        private UIElement titleBarBackground;
        private FrameworkElement titleBarIcon;

        public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register("TitleBarHeight", typeof(int), typeof(CustomWindow), new PropertyMetadata(30, TitleBarHeightPropertyChangedCallback));
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register("TitleTemplate", typeof(DataTemplate), typeof(CustomWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(CustomWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowTitleBarProperty = DependencyProperty.Register("ShowTitleBar", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true, OnShowTitleBarPropertyChangedCallback, OnShowTitleBarCoerceValueCallback));
        public static readonly DependencyProperty UseNoneWindowStyleProperty = DependencyProperty.Register("UseNoneWindowStyle", typeof(bool), typeof(CustomWindow), new PropertyMetadata(false, OnUseNoneWindowStylePropertyChangedCallback));
        public static readonly DependencyProperty ShowSystemMenuOnRightClickProperty = DependencyProperty.Register("ShowSystemMenuOnRightClick", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowIconOnTitleBarProperty = DependencyProperty.Register("ShowIconOnTitleBar", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true, OnShowIconOnTitleBarPropertyChangedCallback));

        public static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.Register("ShowMinButton", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowMaxRestoreButtonProperty = DependencyProperty.Register("ShowMaxRestoreButton", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty IsMinButtonEnabledProperty = DependencyProperty.Register("IsMinButtonEnabled", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty IsMaxRestoreButtonEnabledProperty = DependencyProperty.Register("IsMaxRestoreButtonEnabled", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty IsCloseButtonEnabledProperty = DependencyProperty.Register("IsCloseButtonEnabled", typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty TitleBarWindowCommandsProperty = DependencyProperty.Register("TitleBarWindowCommands", typeof(WindowCommandsControl), typeof(CustomWindow), new PropertyMetadata(null, UpdateLogicalChildren));

        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        public bool IsWindowDraggable { get; set; } = true;

        public CustomWindow()
        {
            InitializeWindowChromeBehavior();
        }

        /// <summary>
        /// Initializes the WindowChromeBehavior which is needed to render the custom WindowChrome.
        /// </summary>
        private void InitializeWindowChromeBehavior()
        {
            var behavior = new WindowChromeBehavior();

            Interaction.GetBehaviors(this).Add(behavior);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            titleBarIcon = GetPart<FrameworkElement>(PART_TitleBarIcon);
            windowTitleThumb = GetPart<Thumb>(PART_WindowTitleThumb);
            windowTitleBorder = GetPart<Border>(PART_WindowTitleBorder);
            titleBar = GetPart<UIElement>(PART_TitleBar);
            titleBarBackground = GetPart<UIElement>(PART_WindowTitleBackground);

            SetVisibilityForAllTitleElements();
        }

        /// <summary>
        /// Gets the template child with the given name.
        /// </summary>
        /// <typeparam name="T">The interface type inherited from DependencyObject.</typeparam>
        /// <param name="name">The name of the template child.</param>
        internal T GetPart<T>(string name) where T : DependencyObject
        {
            return GetTemplateChild(name) as T;
        }

        private void AddWindowEventHandlers()
        {
            RemoveWindowEventHandlers();

            if (windowTitleThumb != null)
            {
                windowTitleThumb.PreviewMouseLeftButtonUp += WindowTitleThumbOnPreviewMouseLeftButtonUp;
                windowTitleThumb.DragDelta += this.WindowTitleThumbMoveOnDragDelta;
                windowTitleThumb.MouseDoubleClick += this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                windowTitleThumb.MouseRightButtonUp += this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }

            if (titleBar is ICustomThumb thumbContentControl)
            {
                thumbContentControl.PreviewMouseLeftButtonUp += WindowTitleThumbOnPreviewMouseLeftButtonUp;
                thumbContentControl.DragDelta += this.WindowTitleThumbMoveOnDragDelta;
                thumbContentControl.MouseDoubleClick += this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                thumbContentControl.MouseRightButtonUp += this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }
        }

        private void RemoveWindowEventHandlers()
        {
            // clear all event handlers first:
            if (windowTitleThumb != null)
            {
                windowTitleThumb.PreviewMouseLeftButtonUp -= this.WindowTitleThumbOnPreviewMouseLeftButtonUp;
                windowTitleThumb.DragDelta -= this.WindowTitleThumbMoveOnDragDelta;
                windowTitleThumb.MouseDoubleClick -= this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                windowTitleThumb.MouseRightButtonUp -= this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }

            if (titleBar is ICustomThumb thumbContentControl)
            {
                thumbContentControl.PreviewMouseLeftButtonUp -= this.WindowTitleThumbOnPreviewMouseLeftButtonUp;
                thumbContentControl.DragDelta -= this.WindowTitleThumbMoveOnDragDelta;
                thumbContentControl.MouseDoubleClick -= this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                thumbContentControl.MouseRightButtonUp -= this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }
        }

        private static void TitleBarHeightPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = (CustomWindow)dependencyObject;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibilityForAllTitleElements();
            }
        }

        private void SetVisibilityForTitleBarIcon()
        {
            if (this.titleBarIcon != null)
            {
                var isVisible = (this.ShowIconOnTitleBar && this.ShowTitleBar);
                var iconVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                this.titleBarIcon.Visibility = iconVisibility;
            }
        }

        private void SetVisibilityForAllTitleElements()
        {
            this.SetVisibilityForTitleBarIcon();
            var newVisibility = this.TitleBarHeight > 0 && this.ShowTitleBar && !this.UseNoneWindowStyle ? Visibility.Visible : Visibility.Collapsed;

            titleBar?.SetCurrentValue(VisibilityProperty, newVisibility);
            titleBarBackground?.SetCurrentValue(VisibilityProperty, newVisibility);

            this.AddWindowEventHandlers();
        }

        /// <summary>
        /// Gets or sets whether if the minimize button is visible and the minimize system menu is enabled.
        /// </summary>
        public bool ShowMinButton
        {
            get => (bool)GetValue(ShowMinButtonProperty);
            set => SetValue(ShowMinButtonProperty, value);
        }

        /// <summary>
        /// Gets or sets whether if the maximize/restore button is visible and the maximize/restore system menu is enabled.
        /// </summary>
        public bool ShowMaxRestoreButton
        {
            get => (bool)GetValue(ShowMaxRestoreButtonProperty);
            set => SetValue(ShowMaxRestoreButtonProperty, value);
        }

        /// <summary>
        /// Gets or sets whether if the close button is visible.
        /// </summary>
        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        /// <summary>
        /// Gets/sets if the min button is enabled.
        /// </summary>
        public bool IsMinButtonEnabled
        {
            get => (bool)GetValue(IsMinButtonEnabledProperty);
            set => SetValue(IsMinButtonEnabledProperty, value);
        }

        /// <summary>
        /// Gets/sets if the max/restore button is enabled.
        /// </summary>
        public bool IsMaxRestoreButtonEnabled
        {
            get => (bool)GetValue(IsMaxRestoreButtonEnabledProperty);
            set => SetValue(IsMaxRestoreButtonEnabledProperty, value);
        }

        /// <summary>
        /// Gets/sets if the close button is enabled.
        /// </summary>
        public bool IsCloseButtonEnabled
        {
            get => (bool)GetValue(IsCloseButtonEnabledProperty);
            set => SetValue(IsCloseButtonEnabledProperty, value);
        }

        /// <summary>
        /// Get/sets whether the titlebar icon is visible or not.
        /// </summary>
        public bool ShowIconOnTitleBar
        {
            get => (bool)GetValue(ShowIconOnTitleBarProperty);
            set => SetValue(ShowIconOnTitleBarProperty, value);
        }

        private static void OnShowIconOnTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (CustomWindow)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibilityForTitleBarIcon();
            }
        }

        /// <summary>
        /// Gets or sets the icon content template to show a custom icon.
        /// </summary>
        public DataTemplate IconTemplate
        {
            get => (DataTemplate)GetValue(IconTemplateProperty);
            set => SetValue(IconTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the title content template to show a custom title.
        /// </summary>
        public DataTemplate TitleTemplate
        {
            get => (DataTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }


        /// <summary>
        /// Gets or sets whether the WindowStyle is None or not.
        /// </summary>
        public bool UseNoneWindowStyle
        {
            get => (bool)GetValue(UseNoneWindowStyleProperty);
            set => SetValue(UseNoneWindowStyleProperty, value);
        }

        private static void OnUseNoneWindowStylePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                // if UseNoneWindowStyle = true no title bar should be shown
                var useNoneWindowStyle = (bool)e.NewValue;

                // UseNoneWindowStyle means no title bar, window commands or min, max, close buttons
                if (useNoneWindowStyle)
                {
                    ((CustomWindow)d).SetCurrentValue(ShowTitleBarProperty, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the system menu should popup with right mouse click if the mouse position is on title bar or on the entire window if it has no title bar (and no title bar height).
        /// </summary>
        public bool ShowSystemMenuOnRightClick
        {
            get => (bool)GetValue(ShowSystemMenuOnRightClickProperty);
            set => SetValue(ShowSystemMenuOnRightClickProperty, value);
        }

        /// <summary>
        /// Gets or sets the title bar height.
        /// </summary>
        public int TitleBarHeight
        {
            get => (int)GetValue(TitleBarHeightProperty);
            set => SetValue(TitleBarHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the title bar is visible or not.
        /// </summary>
        public bool ShowTitleBar
        {
            get => (bool)GetValue(ShowTitleBarProperty);
            set => SetValue(ShowTitleBarProperty, value);
        }

        private static void OnShowTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (CustomWindow)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibilityForAllTitleElements();
            }
        }

        private static object OnShowTitleBarCoerceValueCallback(DependencyObject d, object value)
        {
            // if UseNoneWindowStyle = true no title bar should be shown
            if (((CustomWindow)d).UseNoneWindowStyle)
            {
                return false;
            }
            return value;
        }


        private void WindowTitleThumbOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoWindowTitleThumbOnPreviewMouseLeftButtonUp(this, e);
        }

        private void WindowTitleThumbMoveOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
        {
            DoWindowTitleThumbMoveOnDragDelta(sender as ICustomThumb, this, dragDeltaEventArgs);
        }

        private void WindowTitleThumbChangeWindowStateOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(this, mouseButtonEventArgs);
        }

        private void WindowTitleThumbSystemMenuOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(this, e);
        }

        internal static void DoWindowTitleThumbOnPreviewMouseLeftButtonUp(CustomWindow window, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.Source == mouseButtonEventArgs.OriginalSource)
            {
                Mouse.Capture(null);
            }
        }

        internal static void DoWindowTitleThumbMoveOnDragDelta(ICustomThumb thumb, [NotNull] CustomWindow window, DragDeltaEventArgs dragDeltaEventArgs)
        {
            if (thumb == null)
            {
                throw new ArgumentNullException(nameof(thumb));
            }
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            // drag only if IsWindowDraggable is set to true
            if (!window.IsWindowDraggable ||
                (!(Math.Abs(dragDeltaEventArgs.HorizontalChange) > 2) && !(Math.Abs(dragDeltaEventArgs.VerticalChange) > 2)))
            {
                return;
            }

            // tage from DragMove internal code
            window.VerifyAccess();

            //var cursorPos = WinApiHelper.GetPhysicalCursorPos();

            // if the window is maximized dragging is only allowed on title bar (also if not visible)
            var windowIsMaximized = window.WindowState == WindowState.Maximized;
            var isMouseOnTitlebar = Mouse.GetPosition(thumb).Y <= window.TitleBarHeight && window.TitleBarHeight > 0;
            if (!isMouseOnTitlebar && windowIsMaximized)
            {
                return;
            }

#pragma warning disable 618
            // for the touch usage
            UnsafeNativeMethods.ReleaseCapture();
#pragma warning restore 618

            if (windowIsMaximized)
            {
                //var cursorXPos = cursorPos.x;
                EventHandler windowOnStateChanged = null;
                windowOnStateChanged = (sender, args) =>
                {
                    //window.Top = 2;
                    //window.Left = Math.Max(cursorXPos - window.RestoreBounds.Width / 2, 0);

                    window.StateChanged -= windowOnStateChanged;
                    if (window.WindowState == WindowState.Normal)
                    {
                        Mouse.Capture(thumb, CaptureMode.Element);
                    }
                };
                window.StateChanged -= windowOnStateChanged;
                window.StateChanged += windowOnStateChanged;
            }

            var criticalHandle = window.CriticalHandle;
#pragma warning disable 618
            // these lines are from DragMove
            // NativeMethods.SendMessage(criticalHandle, WM.SYSCOMMAND, (IntPtr)SC.MOUSEMOVE, IntPtr.Zero);
            // NativeMethods.SendMessage(criticalHandle, WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

            var wpfPoint = window.PointToScreen(Mouse.GetPosition(window));
            var x = (int)wpfPoint.X;
            var y = (int)wpfPoint.Y;
            NativeMethods.SendMessage(criticalHandle, WM.NCLBUTTONDOWN, (IntPtr)HT.CAPTION, new IntPtr(x | (y << 16)));
#pragma warning restore 618
        }

        internal static void DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(CustomWindow window, MouseButtonEventArgs mouseButtonEventArgs)
        {
            // restore/maximize only with left button
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
            {
                // we can maximize or restore the window if the title bar height is set (also if title bar is hidden)
                var canResize = window.ResizeMode == ResizeMode.CanResizeWithGrip || window.ResizeMode == ResizeMode.CanResize;
                var mousePos = Mouse.GetPosition(window);
                var isMouseOnTitlebar = mousePos.Y <= window.TitleBarHeight && window.TitleBarHeight > 0;
                if (canResize && isMouseOnTitlebar)
                {
#pragma warning disable 618
                    if (window.WindowState == WindowState.Normal)
                    {
                        ControlzEx.Windows.Shell.SystemCommands.MaximizeWindow(window);
                    }
                    else
                    {
                        ControlzEx.Windows.Shell.SystemCommands.RestoreWindow(window);
                    }
#pragma warning restore 618
                    mouseButtonEventArgs.Handled = true;
                }
            }
        }

        internal static void DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(CustomWindow window, MouseButtonEventArgs e)
        {
            if (window.ShowSystemMenuOnRightClick)
            {
                // show menu only if mouse pos is on title bar or if we have a window with none style and no title bar
                var mousePos = e.GetPosition(window);
                if ((mousePos.Y <= window.TitleBarHeight && window.TitleBarHeight > 0) || (window.UseNoneWindowStyle && window.TitleBarHeight <= 0))
                {
                    ShowSystemMenuPhysicalCoordinates(window, window.PointToScreen(mousePos));
                }
            }
        }

        protected IntPtr CriticalHandle
        {
            get
            {
                var value = typeof(Window)
                    .GetProperty(nameof(CriticalHandle), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(this, new object[0]);
                return (IntPtr?) value ?? IntPtr.Zero;
            }
        }

#pragma warning disable 618
        private static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
        {
            if (window == null) return;

            var hWnd = new WindowInteropHelper(window).Handle;
            if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd))
                return;

            var hMenu = NativeMethods.GetSystemMenu(hWnd, false);

            var cmd = NativeMethods.TrackPopupMenuEx(hMenu, Constants.TPM_LEFTBUTTON | Constants.TPM_RETURNCMD,
                (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, hWnd, IntPtr.Zero);
            if (0 != cmd)
                NativeMethods.PostMessage(hWnd, WM.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
        }
#pragma warning restore 618

        private static void UpdateLogicalChildren(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = dependencyObject as CustomWindow;
            if (window == null)
            {
                return;
            }

            if (e.OldValue is FrameworkElement oldChild)
            {
                window.RemoveLogicalChild(oldChild);
            }

            if (e.NewValue is FrameworkElement newChild)
            {
                window.AddLogicalChild(newChild);
                // Yes, that's crazy. But we must do this to enable all possible scenarios for setting DataContext
                // in a Window. Without set the DataContext at this point it can happen that e.g. a Flyout
                // doesn't get the same DataContext.
                // So now we can type
                //
                // this.InitializeComponent();
                // this.DataContext = new MainViewModel();
                //
                // or
                //
                // this.DataContext = new MainViewModel();
                // this.InitializeComponent();
                //
                newChild.DataContext = window.DataContext;
            }
        }

    }
}

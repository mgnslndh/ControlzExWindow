using System;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlzExWindow.Controls
{
    /// <summary>
    /// The CustomThumbContentControl control can be used for titles or something else and enables basic drag movement functionality.
    /// </summary>
    public class CustomThumbContentControl : ContentControl, ICustomThumb
    {
        private TouchDevice _currentDevice = null;
        private Point _startDragPoint;
        private Point _startDragScreenPoint;
        private Point _oldDragScreenPoint;

        static CustomThumbContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumbContentControl), new FrameworkPropertyMetadata(typeof(CustomThumbContentControl)));
            FocusableProperty.OverrideMetadata(typeof(CustomThumbContentControl), new FrameworkPropertyMetadata(default(bool)));
            EventManager.RegisterClassHandler(typeof(CustomThumbContentControl), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
        }

        public static readonly RoutedEvent DragStartedEvent
            = EventManager.RegisterRoutedEvent("DragStarted",
                                               RoutingStrategy.Bubble,
                                               typeof(DragStartedEventHandler),
                                               typeof(CustomThumbContentControl));

        public static readonly RoutedEvent DragDeltaEvent
            = EventManager.RegisterRoutedEvent("DragDelta",
                                               RoutingStrategy.Bubble,
                                               typeof(DragDeltaEventHandler),
                                               typeof(CustomThumbContentControl));

        public static readonly RoutedEvent DragCompletedEvent
            = EventManager.RegisterRoutedEvent("DragCompleted",
                                               RoutingStrategy.Bubble,
                                               typeof(DragCompletedEventHandler),
                                               typeof(CustomThumbContentControl));

        /// <summary>
        /// Adds or remove a DragStartedEvent handler
        /// </summary>
        public event DragStartedEventHandler DragStarted
        {
            add { this.AddHandler(DragStartedEvent, value); }
            remove { this.RemoveHandler(DragStartedEvent, value); }
        }

        /// <summary>
        /// Adds or remove a DragDeltaEvent handler
        /// </summary>
        public event DragDeltaEventHandler DragDelta
        {
            add { this.AddHandler(DragDeltaEvent, value); }
            remove { this.RemoveHandler(DragDeltaEvent, value); }
        }

        /// <summary>
        /// Adds or remove a DragCompletedEvent handler
        /// </summary>
        public event DragCompletedEventHandler DragCompleted
        {
            add { this.AddHandler(DragCompletedEvent, value); }
            remove { this.RemoveHandler(DragCompletedEvent, value); }
        }

        public static readonly DependencyPropertyKey IsDraggingPropertyKey
            = DependencyProperty.RegisterReadOnly("IsDragging",
                                                  typeof(bool),
                                                  typeof(CustomThumbContentControl),
                                                  new FrameworkPropertyMetadata(default(bool)));

        /// <summary>
        /// DependencyProperty for the IsDragging property.
        /// </summary>
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates that the left mouse button is pressed and is over the CustomThumbContentControl.
        /// </summary>
        public bool IsDragging
        {
            get { return (bool)this.GetValue(IsDraggingProperty); }
            protected set { this.SetValue(IsDraggingPropertyKey, value); }
        }

        public void CancelDragAction()
        {
            if (!this.IsDragging)
            {
                return;
            }

            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
            }

            this.ClearValue(IsDraggingPropertyKey);
            var horizontalChange = this._oldDragScreenPoint.X - this._startDragScreenPoint.X;
            var verticalChange = this._oldDragScreenPoint.Y - this._startDragScreenPoint.Y;
            this.RaiseEvent(new CustomThumbContentControlDragCompletedEventArgs(horizontalChange, verticalChange, true));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!this.IsDragging)
            {
                e.Handled = true;
                try
                {
                    // focus me
                    this.Focus();
                    // now capture the mouse for the drag action
                    this.CaptureMouse();
                    // so now we are in dragging mode
                    this.SetValue(IsDraggingPropertyKey, true);
                    // get the mouse points
                    this._startDragPoint = e.GetPosition(this);
                    this._oldDragScreenPoint = this._startDragScreenPoint = this.PointToScreen(this._startDragPoint);

                    this.RaiseEvent(new CustomThumbContentControlDragStartedEventArgs(this._startDragPoint.X, this._startDragPoint.Y));
                }
                catch (Exception exception)
                {
                    Trace.TraceError($"{this}: Something went wrong here: {exception} {Environment.NewLine} {exception.StackTrace}");
                    this.CancelDragAction();
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured && this.IsDragging)
            {
                e.Handled = true;
                // now we are in normal mode
                this.ClearValue(IsDraggingPropertyKey);
                // release the captured mouse
                this.ReleaseMouseCapture();
                // get the current mouse position and call the completed event with the horizontal/vertical change
                Point currentMouseScreenPoint = this.PointToScreen(e.MouseDevice.GetPosition(this));
                var horizontalChange = currentMouseScreenPoint.X - this._startDragScreenPoint.X;
                var verticalChange = currentMouseScreenPoint.Y - this._startDragScreenPoint.Y;
                this.RaiseEvent(new CustomThumbContentControlDragCompletedEventArgs(horizontalChange, verticalChange, false));
            }

            base.OnMouseLeftButtonUp(e);
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            // Cancel the drag action if we lost capture
            CustomThumbContentControl thumb = (CustomThumbContentControl)sender;
            if (Mouse.Captured != thumb)
            {
                thumb.CancelDragAction();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!this.IsDragging)
            {
                return;
            }

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                Point currentDragPoint = e.GetPosition(this);
                // Get client point and convert it to screen point
                Point currentDragScreenPoint = this.PointToScreen(currentDragPoint);
                if (currentDragScreenPoint != this._oldDragScreenPoint)
                {
                    this._oldDragScreenPoint = currentDragScreenPoint;
                    e.Handled = true;
                    var horizontalChange = currentDragPoint.X - this._startDragPoint.X;
                    var verticalChange = currentDragPoint.Y - this._startDragPoint.Y;
                    this.RaiseEvent(new DragDeltaEventArgs(horizontalChange, verticalChange) { RoutedEvent = CustomThumbContentControl.DragDeltaEvent });
                }
            }
            else
            {
                // clear some saved stuff
                if (e.MouseDevice.Captured == this)
                {
                    this.ReleaseMouseCapture();
                }

                this.ClearValue(IsDraggingPropertyKey);
                this._startDragPoint.X = 0;
                this._startDragPoint.Y = 0;
            }
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            // Release any previous capture
            this.ReleaseCurrentDevice();
            // Capture the new touch
            this.CaptureCurrentDevice(e);
        }

        protected override void OnPreviewTouchUp(TouchEventArgs e)
        {
            this.ReleaseCurrentDevice();
        }

        protected override void OnLostTouchCapture(TouchEventArgs e)
        {
            // Only re-capture if the reference is not null
            // This way we avoid re-capturing after calling ReleaseCurrentDevice()
            if (this._currentDevice != null)
            {
                this.CaptureCurrentDevice(e);
            }
        }

        private void ReleaseCurrentDevice()
        {
            if (this._currentDevice != null)
            {
                // Set the reference to null so that we don't re-capture in the OnLostTouchCapture() method
                var temp = this._currentDevice;
                this._currentDevice = null;
                this.ReleaseTouchCapture(temp);
            }
        }

        private void CaptureCurrentDevice(TouchEventArgs e)
        {
            bool gotTouch = this.CaptureTouch(e.TouchDevice);
            if (gotTouch)
            {
                this._currentDevice = e.TouchDevice;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CustomThumbContentControlAutomationPeer(this);
        }
    }
}
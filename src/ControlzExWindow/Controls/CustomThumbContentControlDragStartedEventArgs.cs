using System.Windows.Controls.Primitives;

namespace ControlzExWindow.Controls
{
    public class CustomThumbContentControlDragStartedEventArgs : DragStartedEventArgs
    {
        public CustomThumbContentControlDragStartedEventArgs(double horizontalOffset, double verticalOffset)
            : base(horizontalOffset, verticalOffset)
        {
            this.RoutedEvent = CustomThumbContentControl.DragStartedEvent;
        }
    }
}

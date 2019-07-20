using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ControlzExWindow.Controls
{
    public class CustomThumbContentControlDragCompletedEventArgs : DragCompletedEventArgs
    {
        public CustomThumbContentControlDragCompletedEventArgs(double horizontalOffset, double verticalOffset, bool canceled)
            : base(horizontalOffset, verticalOffset, canceled)
        {
            this.RoutedEvent = CustomThumbContentControl.DragCompletedEvent;
        }
    }
}

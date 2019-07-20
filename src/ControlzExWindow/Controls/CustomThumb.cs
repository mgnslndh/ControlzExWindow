using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ControlzExWindow.Controls
{
    public class CustomThumb : Thumb, ICustomThumb
    {
        private TouchDevice _currentDevice = null;

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
    }
}

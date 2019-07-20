using System.Windows;
using System.Windows.Automation.Peers;

namespace ControlzExWindow.Controls
{
    /// <summary>
    /// The CustomThumbContentControlAutomationPeer class exposes the <see cref="T:ControlzExWindow.Controls.CustomThumbContentControl" /> type to UI Automation.
    /// </summary>
    public class CustomThumbContentControlAutomationPeer : FrameworkElementAutomationPeer
    {
        public CustomThumbContentControlAutomationPeer(FrameworkElement owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override string GetClassNameCore()
        {
            return nameof(CustomThumbContentControl);
        }
    }
}
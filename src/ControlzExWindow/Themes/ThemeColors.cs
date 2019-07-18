using System.Windows.Media;

namespace ControlzExWindow.Themes
{
    public class ThemeColors
    {
        protected static Color ColorFromHex(string colorHex)
        {
            return (Color?)ColorConverter.ConvertFromString(colorHex) ?? Colors.Transparent;
        }
    }
}

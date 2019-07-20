using System.Windows;
using System.Windows.Media;

namespace ControlzExWindow.Controls
{
    public static class VisualTreeLocator
    {
        public static T FindAncestor<T>(DependencyObject dependencyObject)
            where T : class
        {
            var target = dependencyObject;
            do
            {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
        }
    }
}

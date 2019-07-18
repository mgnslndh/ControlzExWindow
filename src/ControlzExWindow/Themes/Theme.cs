using System;
using System.Windows;
using System.Windows.Media;

namespace ControlzExWindow.Themes
{
    public sealed class Theme
    {
        [ThreadStatic]
        private static ResourceDictionary _resourceDictionary;

        internal static ResourceDictionary ResourceDictionary
        {
            get
            {
                if (_resourceDictionary != null)
                {
                    return _resourceDictionary;
                }

                _resourceDictionary = new ResourceDictionary();
                LoadThemeType(ThemeType.Light);
                return _resourceDictionary;
            }
        }

        public static ThemeType ThemeType { get; set; } = ThemeType.Light;

        internal static void LoadThemeType(ThemeType themeType)
        {
            ThemeType = themeType;

            switch (themeType)
            {
                case ThemeType.Light:
                    {
                        SetResource(ThemeResourceKey.WindowBackground, LightThemeBrushes.WindowBackground);
                        SetResource(ThemeResourceKey.WindowForeground, LightThemeBrushes.WindowForeground);
                        SetResource(ThemeResourceKey.WindowBorder, LightThemeBrushes.WindowBorder);
                        SetResource(ThemeResourceKey.WindowActiveBorder, LightThemeBrushes.WindowActiveBorder);
                        SetResource(ThemeResourceKey.WindowTitleBackground, LightThemeBrushes.WindowBackground);
                        SetResource(ThemeResourceKey.WindowTitleForeground, LightThemeBrushes.WindowForeground);
                        break;
                    }

                case ThemeType.Dark:
                    {
                        SetResource(ThemeResourceKey.WindowBackground, DarkThemeBrushes.WindowBackground);
                        SetResource(ThemeResourceKey.WindowForeground, DarkThemeBrushes.WindowForeground);
                        SetResource(ThemeResourceKey.WindowBorder, DarkThemeBrushes.WindowBorder);
                        SetResource(ThemeResourceKey.WindowActiveBorder, DarkThemeBrushes.WindowActiveBorder);
                        SetResource(ThemeResourceKey.WindowTitleBackground, DarkThemeBrushes.WindowBackground);
                        SetResource(ThemeResourceKey.WindowTitleForeground, DarkThemeBrushes.WindowForeground);
                        break;
                    }
            }
        }

        public static object GetResource(ThemeResourceKey resourceKey)
        {
            return ResourceDictionary.Contains(resourceKey.ToString())
                ? ResourceDictionary[resourceKey.ToString()]
                : null;
        }

        internal static void SetResource(object key, object resource)
        {
            ResourceDictionary[key] = resource;
        }

        internal static void SetResource(ThemeResourceKey resourceKey, object resource)
        {
            SetResource(resourceKey.ToString(), resource);
        }

        internal static Color ColorFromHex(string colorHex)
        {
            return (Color?)ColorConverter.ConvertFromString(colorHex) ?? Colors.Transparent;
        }
    }
}

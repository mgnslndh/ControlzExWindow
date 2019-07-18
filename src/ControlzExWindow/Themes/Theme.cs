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
                    SetResource(ThemeResourceKey.ChromeBackground, LightThemeBrushes.ChromeBackground);
                    SetResource(ThemeResourceKey.ChromeForeground, LightThemeBrushes.ChromeForeground);
                    break;
                }

                case ThemeType.Dark:
                {
                    SetResource(ThemeResourceKey.ChromeBackground, DarkThemeBrushes.ChromeBackground);
                    SetResource(ThemeResourceKey.ChromeForeground, DarkThemeBrushes.ChromeForeground);
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
            return (Color?) ColorConverter.ConvertFromString(colorHex) ?? Colors.Transparent;
        }
    }
}

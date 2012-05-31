using System;
using System.Windows;

namespace BookHouse
{
    public class ThemeManager
    {
        public static void UseTheme(string themeName)
        {
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Clear();
            Uri rd1 = new Uri("/BookHouse;component/" + themeName, UriKind.RelativeOrAbsolute);
            ResourceDictionary dictionary = Application.LoadComponent(rd1) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }
    }
}

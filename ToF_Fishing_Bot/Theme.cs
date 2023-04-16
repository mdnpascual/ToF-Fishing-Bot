using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ToF_Fishing_Bot
{
    public static class Theme
    {
        public static readonly SolidColorBrush WhiteColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        public static readonly SolidColorBrush BlackColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
        public static readonly SolidColorBrush GreenColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 255, 0));
        public static readonly SolidColorBrush GBoxDefaultBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 213, 223, 229));
        public static readonly SolidColorBrush ButtonDefaultBGColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 221, 221, 221));
        public static readonly SolidColorBrush ColorAccent1 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 9, 25, 40));
        public static readonly SolidColorBrush ColorAccent2 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 26, 72, 116));
        public static readonly SolidColorBrush ColorAccent3 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 53, 167, 241));
        public static readonly SolidColorBrush ColorAccent4 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 141, 203, 246));
        public static readonly SolidColorBrush ColorAccent5 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 226, 242, 252));

        public static ImageSource DayImage = new BitmapImage(new Uri("pack://application:,,,/img/day.png"));
        public static ImageSource NightImage = new BitmapImage(new Uri("pack://application:,,,/img/night.png"));
        public static ImageSource DaySettingImage = new BitmapImage(new Uri("pack://application:,,,/img/setting_day.png"));
        public static ImageSource NightSettingImage = new BitmapImage(new Uri("pack://application:,,,/img/setting_night.png"));
        public static ResourceDictionary Styling = new() { Source = new Uri("/Tof_Fishing_Bot;component/Resources/StylingDictionary.xaml", UriKind.RelativeOrAbsolute) };
        public static Style DarkStyle = Styling["btnRoundDark"] as Style;
        public static Style LightStyle = Styling["btnRoundLight"] as Style;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RecordRemoteClientApp.Misc
{
    /// <summary>
    /// Class for storing the color settings and updating
    /// </summary>
    public class Settings : DependencyObject
    {
        public static readonly DependencyProperty MainColorProperty = DependencyProperty.Register("MainColor", typeof(System.Windows.Media.Brush), typeof(Settings));
        public static readonly DependencyProperty SecondaryColorProperty = DependencyProperty.Register("SecondaryColor", typeof(System.Windows.Media.Color), typeof(Settings));
        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register("HighlightColor", typeof(System.Windows.Media.Color), typeof(Settings));

        public System.Windows.Media.Brush MainColor
        {
            get { return (System.Windows.Media.Brush)GetValue(MainColorProperty); }
            set { SetValue(MainColorProperty, value); }
        }

        public System.Windows.Media.Color SecondaryColor
        {
            get { return (System.Windows.Media.Color)GetValue(SecondaryColorProperty); }
            set { SetValue(SecondaryColorProperty, value); }
        }

        public System.Windows.Media.Color HighlightColor
        {
            get { return (System.Windows.Media.Color)GetValue(HighlightColorProperty); }
            set { SetValue(HighlightColorProperty, value); }
        }

        public static Settings Instance { get; private set; }

        static Settings()
        {
            Instance = new Settings();
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecordRemoteClientApp.Views
{
    /// <summary>
    /// Interaction logic for SongDisplayUserControl.xaml
    /// </summary>
    public partial class SongDisplayUserControl : UserControl
    {
        public SongDisplayUserControl()
        {
            InitializeComponent();
        }



        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = new MemoryStream(File.ReadAllBytes(@"../../Images/ionicons-1.5.2/png/512/ios7-rewind-white.png"));
            source.EndInit();
            RewindImage.Source = source;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = new MemoryStream(File.ReadAllBytes(@"../../Images/ionicons-1.5.2/png/512/ios7-rewind-outline-white.png"));
            source.EndInit();
            RewindImage.Source = source;
        }
    }
}

using RecordRemoteClientApp.Models;
using RecordRemoteClientApp.ViewModel;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for SongUserControlView.xaml
    /// </summary>
    public partial class SongUserControlView : UserControl
    {
        public SongUserControlView()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;

            TextBox txtBox = (TextBox)sender;
            Song s = (Song)txtBox.Tag;

            vm.UpdateSongDatabase(s);
        }

        private void ListViewDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem cm = (MenuItem)sender;
            MainViewModel vm = DataContext as MainViewModel;
            vm.SetCurrentAlbum(cm.DataContext as Song);
        }
    }
}

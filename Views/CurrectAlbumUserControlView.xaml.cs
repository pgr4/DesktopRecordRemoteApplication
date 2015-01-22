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
    /// Interaction logic for CurrectAlbumUserControlView.xaml
    /// </summary>
    public partial class CurrectAlbumUserControlView : UserControl
    {
        public CurrectAlbumUserControlView()
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

        Point startPoint;

        private void ListViewItemLeftClick(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private void ListViewItemMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                // Find the data behind the ListViewItem
                Song song = (Song)listViewItem.Tag;

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("Song", song);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
            }
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

    }
}

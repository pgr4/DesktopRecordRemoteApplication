using RecordRemoteClientApp.Models.LastFM;
using RecordRemoteClientApp.Models;
using RecordRemoteClientApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace RecordRemoteClientApp.Views
{
    /// <summary>
    /// Interaction logic for AlbumTrackAssociationView.xaml
    /// </summary>
    public partial class AlbumTrackAssociationView : Window
    {
        #region Properties

        private bool _browsing = false;

        #endregion

        #region Constructors

        public AlbumTrackAssociationView()
        {
            InitializeComponent();
        }

        public AlbumTrackAssociationView(int songCount)
        {
            InitializeComponent();

            DataContext = new AlbumTrackAssociationViewModel(songCount);
        }

        #endregion

        #region Methods

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (((AlbumTrackAssociationViewModel)DataContext).CanCloseWindow())
            {
                this.Close();
            }
        }

        private void ArtistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_browsing)
            {
                TextBox sen = sender as TextBox;
                ((AlbumTrackAssociationViewModel)DataContext).GetArtists(sen.Text);
            }
        }

        private void AlbumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_browsing)
            {
                TextBox sen = sender as TextBox;
                ((AlbumTrackAssociationViewModel)DataContext).GetAlbums(sen.Text);
            }
        }

        private void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            var selectedArtist = lv.SelectedItem as LastFMArtist;
            if (lv.SelectedItem != null)
            {
                ((AlbumTrackAssociationViewModel)DataContext).UpdateArtist(selectedArtist);
            }
        }

        private void AlbumListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            var selectedAlbum = lv.SelectedItem as LastFMAlbum;
            if (lv.SelectedItem != null)
            {
                ((AlbumTrackAssociationViewModel)DataContext).UpdateAlbum(selectedAlbum);
            }
        }

        /// <summary>
        /// On selection change of the main listbox we need to refresh the bindings of the buttons in each listbox item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void songListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (songListBox.Tag.ToString() == "Normal")
            {
                //Loop through all the listboxitems
                foreach (var item in songListBox.ItemContainerGenerator.Items)
                {
                    //Get the listboxitem from the item
                    ListBoxItem lbi = songListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;

                    if (lbi != null)
                    {
                        //get the item's template parent
                        ContentPresenter templateParent = GetFrameworkElementByName<ContentPresenter>(lbi);
                        //get the DataTemplate that TextBlock in.
                        DataTemplate dataTemplate = songListBox.ItemTemplate;
                        //Get the merge and delete button from the datatemplate and update the targer (refresh binding)
                        if (dataTemplate != null && templateParent != null)
                        {
                            var btnMerge = dataTemplate.FindName("btnMerge", templateParent) as Button;
                            if (btnMerge != null)
                            {
                                BindingOperations.GetMultiBindingExpression(btnMerge, Button.VisibilityProperty).UpdateTarget();
                                BindingOperations.GetBindingExpression(btnMerge, Button.IsEnabledProperty).UpdateTarget();
                            }

                            var btnDelete = dataTemplate.FindName("btnDelete", templateParent) as Button;
                            if (btnDelete != null)
                            {
                                BindingOperations.GetMultiBindingExpression(btnDelete, Button.VisibilityProperty).UpdateTarget();
                                BindingOperations.GetBindingExpression(btnDelete, Button.IsEnabledProperty).UpdateTarget();
                            }
                        }
                    }
                }
            }
            else if (songListBox.Tag.ToString() == "Merge")
            {
                //Merge the the ListBox's selected item and the sender
                ((AlbumTrackAssociationViewModel)DataContext).MergeSelectedTrack();

                //Set the background
                SetListBoxItemBackground("Normal");
            }
        }

        private void SetListBoxItemBackground(string type)
        {
            if (type == "Merge")
            {
                //Loop through all the listboxitems
                foreach (var item in songListBox.ItemContainerGenerator.Items)
                {
                    //Get the listboxitem from the item
                    ListBoxItem lbi = songListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    //Set the background if lbi is not the selected listboxitem
                    if (!lbi.IsSelected)
                    {
                        lbi.Background = Brushes.Cyan;
                    }
                }
            }
            else
            {
                //Loop through all the listboxitems
                foreach (var item in songListBox.ItemContainerGenerator.Items)
                {
                    //Get the listboxitem from the item
                    ListBoxItem lbi = songListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    //Set the background
                    lbi.Background = Brushes.Transparent;
                }
            }
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (songListBox.Tag.ToString() == "Normal")
            {
                //Set the background of the ListBoxItems for a merge
                SetListBoxItemBackground("Merge");

                //Set the tag of the ListBox
                songListBox.Tag = "Merge";
            }
            else if (songListBox.Tag.ToString() == "Merge")
            {
                //Set the background of the ListBoxItems for a merge
                SetListBoxItemBackground("Normal");

                //Set the tag of the ListBox
                songListBox.Tag = "Normal";
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            SongAndNumber san = ((ContentPresenter)btn.TemplatedParent).Content as SongAndNumber;
            ((AlbumTrackAssociationViewModel)DataContext).DeleteSong(san);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            _browsing = true;
            ((AlbumTrackAssociationViewModel)DataContext).Browse();
            _browsing = false;
        }

        private void imgArtAlbum_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as AlbumTrackAssociationViewModel;
            vm.SelectAlbumArt(((Image)sender).DataContext as AssociationPicture);
        }

        private void AlbumTrackAssociationView_OnDrop(object sender, DragEventArgs e)
        {
            var window = (Window)sender;
            window.Opacity = 1;
            var vm = window.DataContext as AlbumTrackAssociationViewModel;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var item in files)
                {
                    vm.AddAlbumArt(item);
                }
            }
            DragLabel.Visibility = Visibility.Collapsed;
        }

        private void AlbumTrackAssociationView_OnDragOver(object sender, DragEventArgs e)
        {
            var window = (Window)sender;
            window.Opacity = .4;
            DragLabel.Visibility = Visibility.Visible;
        }

        private void AlbumTrackAssociationView_OnDragLeave(object sender, DragEventArgs e)
        {
            var window = (Window)sender;
            window.Opacity = 1;
            DragLabel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Static Functions
        private static T GetFrameworkElementByName<T>(FrameworkElement referenceElement) where T : FrameworkElement
        {
            FrameworkElement child = null;
            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                System.Diagnostics.Debug.WriteLine(child);
                if (child != null && child.GetType() == typeof(T))
                { break; }
                else if (child != null)
                {
                    child = GetFrameworkElementByName<T>(child);
                    if (child != null && child.GetType() == typeof(T))
                    {
                        break;
                    }
                }
            }
            return child as T;
        }

        #endregion

    }
}

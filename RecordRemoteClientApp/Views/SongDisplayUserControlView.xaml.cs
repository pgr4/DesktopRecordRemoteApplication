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
using RecordRemoteClientApp.ViewModel;

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

        private void RewindButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            vm.Rewind();
        }

        private void PauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            vm.Pause();
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            vm.Play();
        }

        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            vm.Skip();
        }
    }
}

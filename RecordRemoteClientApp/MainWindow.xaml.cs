using RecordRemoteClientApp.Data;
using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Models;
using RecordRemoteClientApp.ViewModel;
using RecordRemoteClientApp.Views;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RecordRemoteClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Icon = new BitmapImage(new Uri("../../ProjectImages/record-player.jpg", UriKind.Relative)); 
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Song"))
            {
                Song droppedSong = e.Data.GetData("Song") as Song;

                MainViewModel vm = DataContext as MainViewModel;
                vm.AddToQueue(droppedSong);

            }
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            Sender.SendGenericMessage(MessageCommand.Scan);
        }

        private void btnPower_Click(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainViewModel;
            if (mv.PowerType == PowerStatus.Off || mv.PowerType == PowerStatus.Unknown)
            {
                Sender.SendGenericMessage(MessageCommand.SwitchPowerOff);
            }
            else
            {
                Sender.SendGenericMessage(MessageCommand.SwitchPowerOn);
            }
        }

        private void btnGetPower_Click(object sender, RoutedEventArgs e)
        {
            Sender.SendGenericMessage(MessageCommand.GetPower);
        }

        private void btnGetStatus_Click(object sender, RoutedEventArgs e)
        {
            Sender.SendGenericMessage(MessageCommand.Status);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new SettingsView().Show();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutView().Show();
        }
    }

}

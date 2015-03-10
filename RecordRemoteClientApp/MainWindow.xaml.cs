﻿using RecordRemoteClientApp.Data;
using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Models;
using RecordRemoteClientApp.ViewModel;
using System.Windows;

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

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            Sender.SendScanMessage();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}

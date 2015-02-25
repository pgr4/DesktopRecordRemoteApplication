﻿using System.Windows.Input;
using GalaSoft.MvvmLight;
using RecordRemoteClientApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Models
{
    public class Song : ViewModelBase
    {
        public Song()
        {

        }

        public Song(tblSong item)
        {
            BreakNumber = item.Break_Number;
            ID = item.Id;
            Key = item.Key;
            Title = item.Title;
            Album = item.Album;
            Artist = item.Artist;
        }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged("Title");
            }
        }

        private int breakNumber;

        public int BreakNumber
        {
            get { return breakNumber; }
            set { breakNumber = value; }
        }

        public int ID { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public byte[] Key { get; set; }
    }
}
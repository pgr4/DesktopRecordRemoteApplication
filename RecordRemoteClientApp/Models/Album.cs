using System.Data.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Models
{
    public class Album : ViewModelBase
    {
        public Album()
        {

        }

        public Album(tblAlbum album)
        {
            Name = album.Album;
            Artist = album.Artist;
            Key = StringToKey(album.Key);
            Breaks = album.Breaks;
            Calculated = Convert.ToBoolean(album.Calculated);
            ID = album.Id;
            Image = album.Image;
        }

        private int[] StringToKey(string key)
        {
            string[] tokens = key.Split(',');

            return Array.ConvertAll<string, int>(tokens, int.Parse);
        }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string artist;

        public string Artist
        {
            get { return artist; }
            set
            {
                artist = value;
                RaisePropertyChanged("Artist");
            }
        }

        private bool calculated;

        public bool Calculated
        {
            get { return calculated; }
            set
            {
                calculated = value;
                RaisePropertyChanged("Calculated");
            }
        }

        private int? breaks;

        public int? Breaks
        {
            get { return breaks; }
            set
            {
                breaks = value;
                RaisePropertyChanged("Breaks");
            }
        }

        private int[] key;

        public int[] Key
        {
            get { return key; }
            set { key = value; }
        }

        private int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private Byte[] image;

        public Byte[] Image
        {
            get { return image; }
            set
            {
                image = value;
                RaisePropertyChanged("Image");
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Models.LastFM
{
    public class LastFMAlbum
    {
         public LastFMAlbum()
         {
            Songs = new List<string>();
        }

         public LastFMAlbum(string name, string artist, string url, Byte[] image)
        {
            Name = name;
            Artist = artist;
            Url = url;
            Image = image;
            Songs = new List<string>();
        }

        public List<string> Songs { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public Byte[] Image { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}

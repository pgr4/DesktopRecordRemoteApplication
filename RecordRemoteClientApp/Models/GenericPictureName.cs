using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using System.Windows.Media.Imaging;

namespace RecordRemoteClientApp.Models
{
    public class GenericPictureName
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        private ImageSource imgSource;

        public ImageSource ImgSource
        {
            get { return imgSource; }
            set
            {
                imgSource = value;
            }
        }

        private byte[] imgBytes;

        public byte[] ImgBytes
        {
            get { return imgBytes; }
            set
            {
                imgBytes = value;
            }
        }

        public GenericPictureName()
        {

        }

        public GenericPictureName(string name)
        {
            Name = name;
        }

        public GenericPictureName(string name, string image)
        {
            Name = name;
            var bitmapImage = new BitmapImage();
            if (image == "")
            {
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(@"C:\Users\pat\Desktop\vinyl-record.jpg");
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                ImgBytes = File.ReadAllBytes(@"C:\Users\pat\Desktop\vinyl-record.jpg");
            }
            else
            {
                var imageBuffer = new WebClient().DownloadData(image);
                
                ImgBytes = imageBuffer;

                using (var ms = new MemoryStream(imageBuffer))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                bitmapImage.Freeze();
            }
            ImgSource = bitmapImage;
        }
    }
}

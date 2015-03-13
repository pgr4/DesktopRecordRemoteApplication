﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;

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

        private byte[] imBytes;

        public byte[] ImBytes
        {
            get { return imBytes; }
            set
            {
                imBytes = value;
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
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(image); ;
            bitmapImage.EndInit();
            ImgSource = bitmapImage;
        }

        public GenericPictureName(string name, byte[] image)
        {
            Name = name;
            ImgSource = ByteToImage(image);
        }

        private ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

    }
}

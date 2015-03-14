using System;
using System.Windows;
using GalaSoft.MvvmLight;

namespace RecordRemoteClientApp.Models
{
    public class AssociationPicture : ViewModelBase
    {
        public AssociationPicture()
        {

        }

        public bool IsUserAdded { get; set; }

        private Byte[] sourceBytes;

        public Byte[] SourceBytes
        {
            get { return sourceBytes; }
            set
            {
                sourceBytes = value;
                RaisePropertyChanged("SourceBytes");
            }
        }

        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                RaisePropertyChanged("Selected");
            }
        }
    }
}

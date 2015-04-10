using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RecordRemoteClientApp.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private Color mainColor;

        public Color MainColor
        {
            get { return mainColor; }
            set
            {
                mainColor = value;
                RaisePropertyChanged("MainColor");
            }
        }

        private Color secondaryColor;

        public Color SecondaryColor
        {
            get { return secondaryColor; }
            set
            {
                secondaryColor = value;
                RaisePropertyChanged("SecondaryColor");
            }
        }

        private Color highlightColor;

        public Color HighlightColor
        {
            get { return highlightColor; }
            set
            {
                highlightColor = value;
                RaisePropertyChanged("HighlightColor");
            }
        }

    }
}

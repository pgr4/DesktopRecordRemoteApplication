using GalaSoft.MvvmLight;
using RecordRemoteClientApp.Misc;
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

        public SettingsViewModel()
        {
            MainColor = ((SolidColorBrush)Settings.Instance.MainColor).Color;
            SecondaryColor = Settings.Instance.SecondaryColor;
            HighlightColor = Settings.Instance.HighlightColor;
        }

        /// <summary>
        /// Process for Writing and setting the current Properties to settings
        /// </summary>
        public void Set()
        {
            List<string> setList = new List<string>();

            Settings.Instance.MainColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(MainColor.A, MainColor.R, MainColor.G, MainColor.B));
            setList.Add("main" + MainColor.A.ToString() + "," + MainColor.R.ToString() + "," + MainColor.G.ToString() + "," + MainColor.B.ToString());
            Settings.Instance.SecondaryColor = SecondaryColor;
            setList.Add("secondary" + SecondaryColor.A.ToString() + "," + SecondaryColor.R.ToString() + "," + SecondaryColor.G.ToString() + "," + SecondaryColor.B.ToString());
            Settings.Instance.HighlightColor = HighlightColor;
            setList.Add("highlight" + HighlightColor.A.ToString() + "," + HighlightColor.R.ToString() + "," + HighlightColor.G.ToString() + "," + HighlightColor.B.ToString());

            System.IO.File.WriteAllLines(@"C:\RecordWebApi\settings.txt", setList);
        }
    }
}

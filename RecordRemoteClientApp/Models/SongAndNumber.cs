using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Models
{
    /// <summary>
    /// For Association binding displays the name and song index
    /// </summary>
    public class SongAndNumber : ViewModelBase
    {
        public SongAndNumber()
        {

        }

        public SongAndNumber(string num, string name)
        {
            Number = num;
            Name = name;
        }

        private string number;

        public string Number
        {
            get { return number; }
            set { 
                number = value;
                RaisePropertyChanged("Number");
            }
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

    }
}

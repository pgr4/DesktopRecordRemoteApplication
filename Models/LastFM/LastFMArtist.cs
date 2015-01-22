using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Models.LastFM
{
    public class LastFMArtist
    {
        public LastFMArtist()
        {

        }

        public LastFMArtist(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}

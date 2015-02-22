using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Enumerations
{
    public enum MessageCommand
    {
        None = 0,
        NewAlbum,
        CurrentAlbum,
        Status,
        Busy = 20,
        Ready = 21
    }

    public enum QueryType
    {
        [Description("method=artist.search&artist=")]
        Artist,
        [Description("method=album.search&album=")]
        Album,
        [Description("method=album.getinfo&")]
        AlbumInfo
    }

    public static class MyEnumExtensions
    {
        public static string ToDescriptionString(this QueryType val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }

}

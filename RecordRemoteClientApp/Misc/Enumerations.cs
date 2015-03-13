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
        Scan,
        Sync,
        GetPower,
        SwitchPowerOn,
        SwitchPowerOff,

        PowerUnknown = 14,
        On = 15,
        Off = 16,
       
        Unknown = 20,
        Ready = 21,
        Play = 22,
        GoToTrack = 23,
        Pause = 24,
        Stop = 25,
        sScan = 26


    }

    public enum PowerStatus
    {
        Unknown,
        On,
        Off
    }

    public enum BusyStatus
    {
        [Description("Unknown")]
        Unknown = 20,
        [Description("Ready")]
        Ready = 21,
        [Description("Playing")]
        Play = 22,
        [Description("Moving")]
        GoToTrack = 23,
        [Description("Pausing")]
        Pause = 24,
        [Description("Stopping")]
        Stop = 25,
        [Description("Scanning")]
        sScan = 26
    }

    public enum QueryType
    {
        [Description("method=artist.search&artist=")]
        Artist,
        [Description("method=artist.gettopalbums&artist=")]
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
        public static string ToDescriptionString(this BusyStatus val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Enumerations
{
    /// <summary>
    /// Types of Header commands we will be receiving
    /// </summary>
    public enum MessageCommand
    {
        None = 0,
        NewAlbum = 1,
        CurrentAlbum = 2,
        Status = 3,
        Scan = 4,
        Sync = 5,
        GetPower = 6,
        SwitchPowerOn = 7,
        SwitchPowerOff = 8,
        PositionUpdate = 9,
        AtBeginning = 10,

        PowerUnknown = 14,
        On = 15,
        Off = 16,

        RequestSync = 18,

        Unknown = 20,
        Ready = 21,
        Play = 22,
        GoToTrack = 23,
        Pause = 24,
        Stop = 25,
        sScan = 26,

        MediaGoToTrack = 30,
        MediaPlay = 31,
        MediaStop = 32,
        MediaGoToBeginning = 33,
        QueueGoToTrack = 34,
        QueueGoToBeginning = 35
    }

    /// <summary>
    /// Power States
    /// </summary>
    public enum PowerStatus
    {
        Unknown,
        On,
        Off
    }

    /// <summary>
    /// Busy States
    /// Can be converted from message command
    /// </summary>
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

    /// <summary>
    /// Default queries
    /// </summary>
    public enum QueryType
    {
        [Description("method=artist.search&artist=")]
        Artist,
        [Description("method=artist.gettopalbums&artist=")]
        Album,
        [Description("method=album.getinfo&")]
        AlbumInfo
    }

    /// <summary>
    /// To String Helper
    /// </summary>
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

using System.Data.Linq.Mapping;

namespace RecordRemoteClientApp.Models
{
    /// <summary>
    /// Database representation of a song
    /// </summary>
    [Table(Name = "tblSong")]
    public class tblSong
    {
        [Column(Name = "Id",IsPrimaryKey = true)]
        public int Id { get; set; }
        [Column(Name = "Key")]
        public string Key { get; set; }
        [Column(Name = "Title")]
        public string Title { get; set; }
        [Column(Name = "Artist")]
        public string Artist { get; set; }
        [Column(Name = "Album")]
        public string Album { get; set; }
        [Column(Name = "Break_Number")]
        public int Break_Number { get; set; }
        [Column(Name = "Break_Location_Start")]
        public int Break_Location_Start { get; set; }
        [Column(Name = "Break_Location_End")]
        public int Break_Location_End { get; set; }
    }
}

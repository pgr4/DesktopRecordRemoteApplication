using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Documents;
using GalaSoft.MvvmLight;
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using System.Data.Linq;
using RecordRemoteClientApp.Models;
using RecordRemoteClientApp.Data;
using RecordRemoteClientApp.Views;
using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Misc;
using System.Windows.Media;

namespace RecordRemoteClientApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        //private dbDataContextDataContext db;
        private SQLiteConnection db;
        private SQLiteConnection dbConnection;

        private string default_albumart_location = @"C:\RecordWebApi\vinyl-record.jpg";
        private byte[] default_albumart;

        #endregion

        #region Public Members

        private Song selectedSong;

        public Song SelectedSong
        {
            get { return selectedSong; }
            set
            {
                selectedSong = value;
                RaisePropertyChanged("SelectedSong");
            }
        }

        private bool isPlaying;

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }

        private bool powerEnable;

        public bool PowerEnable
        {
            get { return powerEnable; }
            set
            {
                powerEnable = value;
                RaisePropertyChanged("PowerEnable");
            }
        }

        private BusyStatus busyType;

        public BusyStatus BusyType
        {
            get { return busyType; }
            set
            {
                busyType = value;
                RaisePropertyChanged("BusyType");
            }
        }

        private PowerStatus powerType;

        public PowerStatus PowerType
        {
            get { return powerType; }
            set
            {
                powerType = value;
                switch (value)
                {
                    case PowerStatus.On:
                        PowerEnable = true;
                        break;
                    case PowerStatus.Off:
                        PowerEnable = true;
                        break;
                    case PowerStatus.Unknown:
                        PowerEnable = false;
                        break;
                }
                RaisePropertyChanged("PowerType");
            }
        }

        private string pStatus;

        public string PStatus
        {
            get { return pStatus; }
            set
            {
                pStatus = value;
                RaisePropertyChanged("PStatus");
            }
        }


        private string bstatus;

        public string BStatus
        {
            get { return bstatus; }
            set
            {
                bstatus = value;
                RaisePropertyChanged("BStatus");
            }
        }

        private Table<tblAlbum> dbAlbums;

        public Table<tblAlbum> DbAlbums
        {
            get { return dbAlbums; }
            set
            {
                dbAlbums = value;
                RaisePropertyChanged("DbAlbums");
            }
        }

        private Table<tblSong> dbSongs;

        public Table<tblSong> DbSongs
        {
            get { return dbSongs; }
            set
            {
                dbSongs = value;
                RaisePropertyChanged("DbSongs");
            }
        }

        private Song currentSong;

        public Song CurrentSong
        {
            get { return currentSong; }
            set
            {
                currentSong = value;
                RaisePropertyChanged("CurrentSong");
            }
        }

        private Album currentAlbum;

        public Album CurrentAlbum
        {
            get { return currentAlbum; }
            set
            {
                currentAlbum = value;
                RaisePropertyChanged("CurrentAlbum");
            }
        }

        private ObservableCollection<Song> queueList;

        public ObservableCollection<Song> QueueList
        {
            get { return queueList; }
            set
            {
                queueList = value;
                RaisePropertyChanged("QueueList");
            }
        }

        private ObservableCollection<Song> currentSongList;

        public ObservableCollection<Song> CurrentSongList
        {
            get { return currentSongList; }
            set
            {
                currentSongList = value;
                RaisePropertyChanged("CurrentSongList");
            }
        }

        private ObservableCollection<Song> songList;

        public ObservableCollection<Song> SongList
        {
            get { return songList; }
            set
            {
                songList = value;
                RaisePropertyChanged("SongList");
            }
        }

        private Listener dataListener;

        public Listener DataListener
        {
            get { return dataListener; }
            set
            {
                dataListener = value;
                RaisePropertyChanged("DataListener");
            }
        }

        private Visibility isCurrentAlbumVisible;

        public Visibility IsCurrentAlbumVisible
        {
            get { return isCurrentAlbumVisible; }
            set
            {
                isCurrentAlbumVisible = CurrentSongList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                RaisePropertyChanged("IsCurrentAlbumVisible");
            }
        }


        #endregion

        #region Static Members

        public static int[] Key = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// Initialize or get database
        /// Get Settings
        /// Send out GetStatus GetPower and RequestSync Messages
        /// Refresh our TotalList
        /// </summary>
        public MainViewModel()
        {
            SongList = new ObservableCollection<Song>();
            CurrentSongList = new ObservableCollection<Song>();
            QueueList = new ObservableCollection<Song>();

            DataListener = Listener.Instance;
            DataListener.NewAlbumEvent += DataListener_newAlbumEvent;
            DataListener.SetPowerStatus += DataListener_SetPowerStatus;
            DataListener.SetBusyStatus += DataListener_SetBusyStatus;
            DataListener.SyncMessage += DataListener_SyncMessage;
            DataListener.EventPositionUpdate += DataListener_EventPositionUpdate;
            DataListener.EventPlayingUpdate += DataListener_PlayingUpdate;

            PowerType = PowerStatus.Unknown;
            BusyType = BusyStatus.Unknown;
            BStatus = BusyStatus.Unknown.ToString();
            PStatus = PowerStatus.Unknown.ToString();
            PowerEnable = false;

            StartDataListener();

            InitDatabase();

            RefreshSongList();

            GetDefaultAlbumArt();

            Sender.SendGenericMessage(MessageCommand.Status);
            Sender.SendGenericMessage(MessageCommand.GetPower);
            Sender.SendGenericMessage(MessageCommand.RequestSync);

            IsPlaying = false;

            GetSettings();
        }

        #endregion

        #region Private Functions

        private void GetDefaultAlbumArt()
        {
            var img = Image.FromFile(default_albumart_location);
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                default_albumart = ms.ToArray();
            }
        }

        /// <summary>
        /// Create or get the database
        /// </summary>
        private void InitDatabase()
        {
            if (!File.Exists(@"C:\RecordWebApi\Master.sqlite"))
            {
                //CREATING DB
                db = new SQLiteConnection(@"C:\RecordWebApi\Master.db");

                //CONNECTING
                dbConnection = new SQLiteConnection(@"Data Source=C:\RecordWebApi\Master.sqlite;Version=3;");
                dbConnection.Open();

                string sql;
                SQLiteCommand command;

                //CREATE TABLES
                sql = "CREATE TABLE tblSong (Id integer primary key, Key text, Title text, Artist text, Album text, Break_Number integer, Break_Location_Start integer, Break_Location_End integer)";
                command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE tblAlbum (Id integer primary key, Key text, Album text, Artist text, Calculated integer, Breaks integer, Image blob)";
                command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                db = new SQLiteConnection(@"C:\RecordWebApi\Master.db");

                //CONNECTING
                dbConnection = new SQLiteConnection(@"Data Source=C:\RecordWebApi\Master.sqlite;Version=3;");
                dbConnection.Open();
            }

            RefreshDatabaseTables();
        }


        /// <summary>
        /// Initialize or ReInitialize the records from the album and song databases
        /// </summary>
        private void RefreshDatabaseTables()
        {
            var context = new DataContext(dbConnection);

            DbSongs = context.GetTable<tblSong>();
            DbAlbums = context.GetTable<tblAlbum>();
        }

        /// <summary>
        /// Method for adding an album to the database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="album"></param>
        /// <param name="artist"></param>
        /// <param name="calculated"></param>
        /// <param name="breaks"></param>
        /// <param name="image"></param>
        private void AddAlbumToDatabase(int[] key, string album, string artist, bool calculated, int breaks, byte[] image)
        {
            string sql;
            SQLiteCommand command;
            artist = artist.Replace("'", "''");
            album = album.Replace("'", "''");
            sql = "insert into tblAlbum (Key, Album, Artist, Calculated, Breaks, Image) values (@Key,'" + album + "','" + artist + "','" + Convert.ToInt16(calculated) + "','" + breaks + "', @Image)";
            command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.Add("@Key", DbType.String).Value = MyConverters.KeyToString(key);
            command.Parameters.Add("@Image", DbType.Binary).Value = image;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Method for adding song to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="breakNum"></param>
        /// <param name="breakLocStart"></param>
        /// <param name="breakLocEnd"></param>
        private void AddSongToDatabase(int[] key, string title, string artist, string album, int breakNum, int breakLocStart, int breakLocEnd)
        {
            string sql;
            SQLiteCommand command;
            title = title.Replace("'", "''");
            artist = artist.Replace("'", "''");
            album = album.Replace("'", "''");
            sql = "insert into tblSong (Key, Title, Artist, Album, Break_Number, Break_Location_Start, Break_Location_End) values (@Key,'" + title + "','" + artist + "','" + album + "','" + breakNum + "','" + breakLocStart + "','" + breakLocEnd + "')";
            command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.Add("@Key", DbType.String).Value = MyConverters.KeyToString(key);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Will delete entries in song and album database with matching key
        /// </summary>
        /// <param name="key"></param>
        public void DeleteDatabaseEntrys(int[] key)
        {
            SQLiteCommand command;
            string sql = "delete from tblSong where Key=\"" + MyConverters.KeyToString(key) + "\"";
            command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();

            SQLiteCommand command1;
            string sql1 = "delete from tblAlbum where Key=\"" + MyConverters.KeyToString(key) + "\"";
            command1 = new SQLiteCommand(sql1, dbConnection);
            command1.ExecuteNonQuery();

            RefreshDatabaseTables();

            RefreshSongList();
            
            RefreshWebApi();
        }

        /// <summary>
        /// Function for starting UDP Listener
        /// </summary>
        private void StartDataListener()
        {
            Thread t = new Thread(DataListener.Listen);
            t.IsBackground = true;
            t.Start();
        }

        #endregion

        #region Event Functions

        /// <summary>
        /// Add na information to database
        /// Show the newAlbum information in the current song list
        /// </summary>
        /// <param name="na"></param>
        private void DataListener_newAlbumEvent(NewAlbum na)
        {
            //Check if it isn't a new album
            foreach (var a in DbAlbums)
            {
                //if (MyConverters.StringToKey(a.Key).SequenceEqual<int>(na.Key))
                if (MyConverters.IsKeyMatch(MyConverters.StringToKey(a.Key), na.Key))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        RefreshCurrentSongList(MyConverters.StringToKey(a.Key));

                        CurrentAlbum = new Album(a);

                        //Clear Queue List
                        QueueList.Clear();
                        RaisePropertyChanged("QueueList");
                        //Clear Current Song
                        CurrentSong = null;
                        RaisePropertyChanged("CurrentSong");
                        //Update the Current Song List
                        RaisePropertyChanged("CurrentSongList");
                    });

                    Sender.SendSyncMessage(MyConverters.StringToKey(a.Key));

                    Key = MyConverters.StringToKey(a.Key);

                    return;
                }
            }

            CurrentSong = null;
            CurrentAlbum = null;
            //Clear Queue List
            QueueList.Clear();
            RaisePropertyChanged("QueueList");
            //Clear Current Song
            CurrentSong = null;
            RaisePropertyChanged("CurrentSong");
            //Update the Current Song List
            RaisePropertyChanged("CurrentSongList");


            //Must accept the event by user in order to be put in database
            AutoAlbumTrackAssociationView ATAssocView = new AutoAlbumTrackAssociationView(na);
            ATAssocView.Show();
            ATAssocView.Closed += delegate
            {
                AutoAlbumTrackAssociationViewModel vm = ATAssocView.DataContext as AutoAlbumTrackAssociationViewModel;
                if (vm.CanSubmitEntry)
                {
                    int i = 0;
                    if (vm.IsManual)
                    {
                        //Go through all the songs in the Association View and add them to the database
                        foreach (SongAndNumber sn in (vm.SongList))
                        {
                            if (i == 0)
                            {
                                AddSongToDatabase(na.Key, sn.Name, vm.ManualArtistName,
                                    vm.ManualAlbumName, i + 1, int.MinValue, Convert.ToInt32(na.Key[0]));
                            }
                            else if (i == vm.SongList.Count - 1)
                            {
                                AddSongToDatabase(na.Key, sn.Name, vm.ManualArtistName,
                                    vm.ManualAlbumName, i + 1, Convert.ToInt32(na.Key[na.Key.Length - 1]),
                                    int.MaxValue);
                            }
                            else
                            {
                                AddSongToDatabase(na.Key, sn.Name, vm.ManualArtistName,
                                    vm.ManualAlbumName, i + 1, Convert.ToInt32(na.Key[i - 1]),
                                    Convert.ToInt32(na.Key[i]));
                            }

                            i++;
                        }
                        //Add the album to the database
                        AddAlbumToDatabase(na.Key, vm.ManualAlbumName, vm.ManualArtistName, true, na.Breaks, GetAlbumArt(vm.AlbumArtList.ToList()));

                    }
                    else
                    {
                        //Go through all the songs in the Association View and add them to the database
                        foreach (SongAndNumber sn in (vm.SongList))
                        {
                            if (i == 0)
                            {
                                AddSongToDatabase(na.Key, sn.Name, vm.SelectedArtist.Name,
                                    vm.SelectedAlbum.Name, i + 1, int.MinValue, Convert.ToInt32(na.Key[0]));
                            }
                            else if (i == vm.SongList.Count - 1)
                            {
                                AddSongToDatabase(na.Key, sn.Name, vm.SelectedArtist.Name,
                                    vm.SelectedAlbum.Name, i + 1, Convert.ToInt32(na.Key[na.Key.Length - 1]),
                                    int.MaxValue);
                            }
                            else
                            {
                                AddSongToDatabase(na.Key, sn.Name, vm.SelectedArtist.Name,
                                    vm.SelectedAlbum.Name, i + 1, Convert.ToInt32(na.Key[i - 1]),
                                    Convert.ToInt32(na.Key[i]));
                            }

                            i++;
                        }
                        //Add the album to the database
                        AddAlbumToDatabase(na.Key, vm.SelectedAlbum.Name, vm.SelectedArtist.Name, true, na.Breaks, GetAlbumArt(vm.AlbumArtList.ToList()));
                    }

                    RefreshCurrentSongList(na.Key);

                    RefreshSongList();

                    RefreshWebApi();

                    Sender.SendSyncMessage(na.Key);

                    Key = na.Key;
                }
            };
        }

        /// <summary>
        /// Returns the album art in a byte array or if null gets the default album art
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        private byte[] GetAlbumArt(List<AssociationPicture> ls)
        {
            var item = (from i in ls
                        where i.Selected == true
                        select i).FirstOrDefault();

            if (item == null)
            {
                return default_albumart;
            }
            else
            {
                return item.SourceBytes;
            }
        }

        /// <summary>
        /// Upon getting a sync message refresh our song lists and go fetch album info from database
        /// </summary>
        /// <param name="key"></param>
        private void DataListener_SyncMessage(int[] key)
        {
            RefreshCurrentSongList(key);

            RefreshSongList();

            foreach (var a in DbAlbums)
            {
                if (MyConverters.StringToKey(a.Key).SequenceEqual<int>(key))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        CurrentAlbum = new Album(a);

                        //Clear Queue List
                        QueueList.Clear();
                        RaisePropertyChanged("QueueList");
                        //Clear Current Song
                        CurrentSong = null;
                        RaisePropertyChanged("CurrentSong");
                        //Update the Current Song List
                        RaisePropertyChanged("CurrentSongList");

                    }));
                }
            }
        }

        /// <summary>
        /// Upon getting a busy message update our current busy state
        /// </summary>
        /// <param name="bs"></param>
        private void DataListener_SetBusyStatus(BusyStatus bs)
        {
            BusyType = bs;
            BStatus = bs.ToDescriptionString();
        }

        /// <summary>
        /// Upon getting a power message update our current power state
        /// </summary>
        /// <param name="bs"></param>
        private void DataListener_SetPowerStatus(PowerStatus ps)
        {
            PowerType = ps;
            PStatus = ps.ToString();

            if (ps == PowerStatus.Unknown || ps == PowerStatus.Off)
            {
                SelectedSong = null;
                CurrentAlbum = null;
                CurrentSong = null;
                CurrentSongList.Clear();
                QueueList.Clear();
                IsPlaying = false;
                //IsCurrentAlbumVisible = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Upon getting an update message update the current playing song
        /// </summary>
        /// <param name="loc"></param>
        private void DataListener_EventPositionUpdate(int? loc)
        {
            if (QueueList.Count != 0)
            {
                CurrentSong = QueueList[0];
                QueueList.RemoveAt(0);
                RaisePropertyChanged("CurrentSong");
                RaisePropertyChanged("QueueList");
                if (CurrentSong.BreakLocationStart == int.MinValue)
                {
                    Sender.SendGenericMessage(MessageCommand.QueueGoToBeginning);
                }
                else
                {
                    var b = BitConverter.GetBytes(CurrentSong.BreakLocationStart);
                    Sender.QueueGoToTrackMessage(b[1], b[0]);
                }
            }
            else
            {
                if (loc == null)
                {
                    if (CurrentSongList.Count > 0)
                    {
                        CurrentSong = CurrentSongList[0];
                    }
                }
                else
                {
                    var item = (from i in CurrentSongList
                                where loc.Value == i.BreakLocationStart
                                select i).FirstOrDefault();
                    CurrentSong = item;
                }
            }
        }

        /// <summary>
        /// Upon getting a playing update update whether we are playing or not
        /// </summary>
        /// <param name="b"></param>
        private void DataListener_PlayingUpdate(bool b)
        {
            IsPlaying = b;
        }

        #endregion

        #region Public Functions

        #region Media Controls

        /// <summary>
        /// Send a command to drop tonearm
        /// </summary>
        public void Play()
        {
            if (SelectedSong != null)
            {
                if (IsPlaying == false && CurrentSong == SelectedSong)
                {
                    Sender.PlayMessage();
                }
                else
                {
                    if (SelectedSong.BreakLocationStart == int.MinValue)
                    {
                        Sender.SendGenericMessage(MessageCommand.MediaGoToBeginning);
                    }
                    else
                    {
                        var b = BitConverter.GetBytes(SelectedSong.BreakLocationStart);
                        Sender.GoToTrackMessage(b[1], b[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Send a command to lift tonearm
        /// </summary>
        public void Pause()
        {
            Sender.StopMessage();
        }

        /// <summary>
        /// If current song is last song then send a gototrack for first song
        /// else send a gototrack for next song
        /// </summary>
        public void Skip()
        {
            if (CurrentSong != null)
            {
                var skipSong = (from item in CurrentSongList
                                where item.BreakLocationEnd == CurrentSong.BreakLocationStart
                                select item).FirstOrDefault();

                if (skipSong != null)
                {
                    var b = BitConverter.GetBytes(SelectedSong.BreakLocationStart);
                    Sender.GoToTrackMessage(b[1], b[0]);
                }
                else
                {
                    Sender.SendGenericMessage(MessageCommand.MediaGoToBeginning);
                }
            }
        }

        /// <summary>
        /// If current song is first song then send a gototrack for first song
        /// else send a gotottrack for previous song
        /// </summary>
        public void Rewind()
        {
            if (CurrentSong != null)
            {
                var rewindSong = (from item in CurrentSongList
                                  where item.BreakLocationStart == CurrentSong.BreakLocationEnd
                                  select item).FirstOrDefault();

                if (rewindSong != null)
                {
                    var b = BitConverter.GetBytes(rewindSong.BreakLocationStart);
                    Sender.GoToTrackMessage(b[1], b[0]);
                }
                else
                {
                    Sender.SendGenericMessage(MessageCommand.MediaGoToBeginning);
                }
            }
        }

        #endregion

        #region misc

        /// <summary>
        /// Method for getting the saved settings
        /// </summary>
        private void GetSettings()
        {
            foreach (var item in File.ReadAllLines(@"C:\RecordWebApi\settings.txt"))
            {
                if (item.Contains("main"))
                {
                    var main = item.Replace("main", "");
                    string[] sBytes = main.Split(',');
                    Settings.Instance.MainColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.Parse(sBytes[0]), byte.Parse(sBytes[1]), byte.Parse(sBytes[2]), byte.Parse(sBytes[3])));
                }
                else if (item.Contains("secondary"))
                {
                    var secondary = item.Replace("secondary", "");
                    string[] sBytes = secondary.Split(',');
                    Settings.Instance.SecondaryColor = System.Windows.Media.Color.FromArgb(byte.Parse(sBytes[0]), byte.Parse(sBytes[1]), byte.Parse(sBytes[2]), byte.Parse(sBytes[3]));
                }
                else if (item.Contains("highlight"))
                {
                    var highlight = item.Replace("highlight", "");
                    string[] sBytes = highlight.Split(',');
                    Settings.Instance.HighlightColor = System.Windows.Media.Color.FromArgb(byte.Parse(sBytes[0]), byte.Parse(sBytes[1]), byte.Parse(sBytes[2]), byte.Parse(sBytes[3]));
                }
            }
        }

        /// <summary>
        /// Method to set a current album manually from context menu in the TotalList
        /// </summary>
        /// <param name="song"></param>
        public void SetCurrentAlbum(Song song)
        {
            RefreshCurrentSongList(song.Key);

            RefreshSongList();

            RefreshWebApi();

            Sender.SendSyncMessage(song.Key);

            Key = song.Key;
        }

        /// <summary>
        /// Show all the songs from the database on startup
        /// </summary>
        public void RefreshSongList()
        {
            songList.Clear();

            foreach (var item in DbSongs)
            {
                SongList.Add(new Song(item));
            }

            RaisePropertyChanged("SongList");
        }

        /// <summary>
        /// Update the Current Song List
        /// </summary>
        /// <param name="b"></param>
        public void RefreshCurrentSongList(int[] b)
        {
            var songs = (from item in DbSongs
                         select item).ToList();

            songs = (from item in songs
                     where MyConverters.StringToKey(item.Key).SequenceEqual(b)
                     select item).ToList();

            CurrentSongList.Clear();

            foreach (var item in songs)
            {
                CurrentSongList.Add(new Song(item));
            }

            //SQLITE DOESN'T LIKE FIRSTORDEFAULT
            var albums = (from a in dbAlbums
                          select a).ToList();

            albums = (from a in albums
                      where MyConverters.StringToKey(a.Key).SequenceEqual(b)
                      select a).ToList();

            foreach (var item in albums)
            {
                CurrentAlbum = new Album(item);
                break;
            }

            RaisePropertyChanged("CurrentSongList");
        }

        /// <summary>
        /// Send out an HTTP Request that will update the database on the webapi end
        /// </summary>
        public void RefreshWebApi()
        {
            var loc = new Uri("http://" + Listener.ThisIpAddress + @"/api/update");
            var request = WebRequest.Create(loc);
            string text;
            var response = request.GetResponse();

            //Read to end seems slow ~1.5seconds
            using (BufferedStream buffer = new BufferedStream(response.GetResponseStream()))
            {
                using (StreamReader reader = new StreamReader(buffer))
                {
                    text = reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Add a Song s to QueueList
        /// </summary>
        /// <param name="s"></param>
        public void AddToQueue(Song s)
        {
            QueueList.Add(s);
        }

        #endregion

        #endregion
    }
}

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

namespace RecordRemoteClientApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        //private dbDataContextDataContext db;
        private SQLiteConnection db;
        private SQLiteConnection dbConnection;

        private string default_albumart_location = @"C:\Users\pat\Desktop\vinyl-record.jpg";
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

        #region Constructors

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

            IsPlaying = false;
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
                sql = "CREATE TABLE tblSong (Id integer primary key, Key blob, Title text, Artist text, Album text, Break_Number integer, Break_Location_Start integer, Break_Location_End integer)";
                command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE tblAlbum (Id integer primary key, Key blob, Album text, Artist text, Calculated integer, Breaks integer, Image blob)";
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

        private void AddAlbumToDatabase(byte[] key, string album, string artist, bool calculated, int breaks, byte[] image)
        {
            string sql;
            SQLiteCommand command;
            artist = artist.Replace("'", "''");
            album = album.Replace("'", "''");
            sql = "insert into tblAlbum (Key, Album, Artist, Calculated, Breaks, Image) values (@Key,'" + album + "','" + artist + "','" + Convert.ToInt16(calculated) + "','" + breaks + "', @Image)";
            command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.Add("@Key", DbType.Binary).Value = key;
            command.Parameters.Add("@Image", DbType.Binary).Value = image;
            command.ExecuteNonQuery();
        }

        private void AddSongToDatabase(byte[] key, string title, string artist, string album, int breakNum, int breakLocStart, int breakLocEnd)
        {
            string sql;
            SQLiteCommand command;
            title = title.Replace("'", "''");
            artist = artist.Replace("'", "''");
            album = album.Replace("'", "''");
            sql = "insert into tblSong (Key, Title, Artist, Album, Break_Number, Break_Location_Start, Break_Location_End) values (@Key,'" + title + "','" + artist + "','" + album + "','" + breakNum + "','" + breakLocStart + "','" + breakLocEnd + "')";
            command = new SQLiteCommand(sql, dbConnection);
            command.Parameters.Add("@Key", DbType.Binary).Value = key;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Update the Song's title in the database if an entry in the total song list is edited
        /// TODO:ERROR
        /// </summary>
        /// <param name="s"></param>
        public void UpdateSongDatabase(Song s)
        {
            //string sql;
            //SQLiteCommand command;
            //sql = "update tblSong set Title = @Title, Where Id = @Id)";
            //var titleParam = new SQLiteParameter("@Title", DbType.String) { Value = s.Title };
            //var IdParam = new SQLiteParameter("@Id", DbType.Int16) { Value = s.ID };
            //command = new SQLiteCommand(sql, dbConnection);
            //command.Parameters.Add(titleParam);
            //command.Parameters.Add(IdParam);
            //command.ExecuteNonQuery();
        }

        private void StartDataListener()
        {
            Thread t = new Thread(DataListener.Speak);
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
                if (a.Key.SequenceEqual(na.Key))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        RefreshCurrentSongList(na.Key);

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

                    Sender.SendSyncMessage(na.Key);

                    return;
                }
            }

            //Must accept the event by user in order to be put in database
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
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
                                else if (i == vm.SongList.Count)
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
                        }

                        //Add the album to the database
                        AddAlbumToDatabase(na.Key, vm.SelectedAlbum.Name, vm.SelectedArtist.Name, true, na.Breaks, GetAlbumArt(vm.AlbumArtList.ToList()));

                        RefreshCurrentSongList(na.Key);

                        RefreshSongList();

                        RefreshWebApi();

                        Sender.SendSyncMessage(na.Key);
                    }
                };
            });
        }

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

        private void DataListener_SyncMessage(byte[] key)
        {
            RefreshCurrentSongList(key);
            RefreshSongList();
            foreach (var a in DbAlbums)
            {
                if (a.Key.SequenceEqual(key))
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
                }
            }
        }

        private void DataListener_SetBusyStatus(BusyStatus bs)
        {
            BusyType = bs;
            BStatus = bs.ToDescriptionString();
        }

        private void DataListener_SetPowerStatus(PowerStatus ps)
        {
            PowerType = ps;
            PStatus = ps.ToString();
        }

        private void DataListener_EventPositionUpdate(byte? b)
        {
            if (b == null)
            {
                if (SongList.Count > 0)
                {
                    CurrentSong = SongList[0];
                }
            }
            else
            {
                var item = (from i in SongList
                            where Convert.ToInt32(b.Value) == i.BreakLocationStart
                            select i).FirstOrDefault();
                CurrentSong = item;
            }
        }

        #endregion

        #region Public Functions

        #region Media Controls

        public void Play()
        {
            if (SelectedSong != null)
            {
                Sender.PlayMessage(SelectedSong);
                IsPlaying = true;
            }
        }

        public void Pause()
        {
            Sender.SendGenericMessage(MessageCommand.MediaStop);
            IsPlaying = false;
        }

        public void Skip()
        {
            if (CurrentSong != null)
            {
                var skipSong = (from item in CurrentSongList
                                where item.BreakLocationEnd == CurrentSong.BreakLocationStart
                                select item).FirstOrDefault();

                if (skipSong != null)
                {
                    Sender.SendSkipMessage(skipSong);
                }
                else
                {
                    Sender.SendGenericMessage(MessageCommand.MediaGoToBeginning);
                }
            }
        }

        public void Rewind()
        {
            if (CurrentSong != null)
            {
                var rewindSong = (from item in CurrentSongList
                                  where item.BreakLocationStart == CurrentSong.BreakLocationEnd
                                  select item).FirstOrDefault();

                if (rewindSong != null)
                {
                    Sender.SendRewindMessage(rewindSong);
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
        public void RefreshCurrentSongList(byte[] b)
        {
            var songs = (from item in DbSongs
                         where item.Key == b
                         select item).ToList();

            CurrentSongList.Clear();

            foreach (var item in songs)
            {
                CurrentSongList.Add(new Song(item));
            }

            //SQLITE DOESN'T LIKE FIRSTORDEFAULT
            var albums = (from a in dbAlbums
                          where a.Key == b
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

        public void AddToQueue(Song s)
        {
            QueueList.Add(s);
        }

        #endregion

        #endregion
    }
}

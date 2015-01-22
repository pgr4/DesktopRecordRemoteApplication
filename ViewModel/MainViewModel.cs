using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

namespace RecordRemoteClientApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        //private dbDataContextDataContext db;
        private SQLiteConnection db;
        private SQLiteConnection dbConnection;

        #endregion

        #region Public Members

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

            StartDataListener();

            if (!File.Exists("Master.sqlite"))
            {
                //CREATING DB
                db = new SQLiteConnection("Master.db");

                //CONNECTING
                dbConnection = new SQLiteConnection("Data Source=Master.sqlite;Version=3;");
                dbConnection.Open();

                string sql;
                SQLiteCommand command;

                //CREATE TABLES
                sql = "CREATE TABLE tblSong (Id integer primary key, Key blob, Title text, Artist text, Album text, Break_Number integer)";
                command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE tblAlbum (Id integer primary key, Key blob, Album text, Artist text, Calculated integer, Breaks integer, Image blob)";
                command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                db = new SQLiteConnection("Master.db");

                //CONNECTING
                dbConnection = new SQLiteConnection("Data Source=Master.sqlite;Version=3;");
                dbConnection.Open();
            }

            RefreshDatabaseTables();

            RefreshSongList();

            var img = Image.FromFile(default_albumart_location);
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                default_albumart = ms.ToArray();
            }
        }

        #endregion

        private string default_albumart_location = @"C:\Users\pat\Desktop\vinyl-record.jpg";
        private byte[] default_albumart;

        #region Private Functions

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

        private void AddSongToDatabase(byte[] key, string title, string artist, string album, int breaknum)
        {
            string sql;
            SQLiteCommand command;
            title = title.Replace("'", "''");
            artist = artist.Replace("'", "''");
            album = album.Replace("'", "''");
            sql = "insert into tblSong (Key, Title, Artist, Album, Break_Number) values (@Key,'" + title + "','" + artist + "','" + album + "','" + breaknum + "')";
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

                    return;
                }
            }

            //Must accept the event by user in order to be put in database
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                AlbumTrackAssociationView ATAssocView = new AlbumTrackAssociationView(na.Breaks);
                ATAssocView.Show();

                ATAssocView.Closed += delegate
                {
                    if (((AlbumTrackAssociationViewModel)ATAssocView.DataContext).CanSubmitEntry)
                    {
                        int i = 0;

                        //Go through all the songs in the Association View and add them to the database
                        foreach (SongAndNumber sn in ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).SongList)
                        {
                            AddSongToDatabase(na.Key, sn.Name, ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).ArtistName, ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).AlbumName, i++);
                        }

                        //Add the album to the database
                        AddAlbumToDatabase(na.Key, ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).AlbumName, ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).ArtistName, true, na.Breaks,
                            ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).SelectedAlbum == null ? default_albumart : ((AlbumTrackAssociationViewModel)ATAssocView.DataContext).SelectedAlbum.Image);

                        RefreshCurrentSongList(na.Key);

                        RefreshSongList();

                        RefreshWebApi();
                    }
                };
            });

        }

        #endregion

        #region Public Functions

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

            ///TODO:WHAT???
            songs = (from item in dbSongs
                     where item.Key == b
                     select item).ToList();

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
        // TODO:IMPLEMENT
        /// </summary>
        public void RefreshWebApi()
        {
          String loc = "http://192.168.1.247/api/update";
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
    }
}

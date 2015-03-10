using System.Data.Linq;
using GalaSoft.MvvmLight;
using RecordRemoteClientApp.Models;
using RecordRemoteClientApp.Models.LastFM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.IO;
using TagLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace RecordRemoteClientApp.ViewModel
{
    public class AlbumTrackAssociationViewModel : ViewModelBase
    {
        #region Members

        private Visibility showAlbumHint;

        public Visibility ShowAlbumHint
        {
            get { return showAlbumHint; }
            set
            {
                showAlbumHint = value;
                RaisePropertyChanged("ShowAlbumHint");
            }
        }
        

        public string AlbumName { get; set; }
        public string ArtistName { get; set; }

        private int _numberOfSongs { get; set; }

        private BackgroundWorker _bwArtist;
        private BackgroundWorker _bwAlbum;
        private BackgroundWorker _bwAlbumInfo;

        private string LastSearchedArtist { get; set; }
        private string LastSearchedAlbum { get; set; }

        public ObservableCollection<SongAndNumber> SongList { get; set; }
        public ObservableCollection<LastFMArtist> ArtistList { get; set; }
        public ObservableCollection<LastFMAlbum> AlbumList { get; set; }
        public ObservableCollection<AssociationPicture> AlbumArtList { get; set; }

        public bool ArtistsVisible { get; set; }
        public bool AlbumsVisible { get; set; }

        public LastFMArtist SelectedArtist { get; set; }
        public LastFMAlbum SelectedAlbum { get; set; }

        public Visibility ArtistBusyVisibility { get { return _bwArtist.IsBusy ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility AlbumBusyVisibility { get { return _bwAlbum.IsBusy ? Visibility.Visible : Visibility.Collapsed; } }

        private SongAndNumber _selectedSongAndNumber;

        public SongAndNumber SelectedSongAndNumber
        {
            get { return _selectedSongAndNumber; }
            set
            {
                if (OperationState == "Normal")
                {
                    _selectedSongAndNumber = value;
                }
                else if (OperationState == "Merge")
                {
                    MergeSongAndNumber = value;
                }
            }
        }

        public SongAndNumber MergeSongAndNumber { get; set; }

        public string OperationState { get; set; }

        public bool CanSubmitEntry { get; set; }

        public bool CanRemove
        {
            get { return SongList != null && (SongList.Count > _numberOfSongs); }
        }

        #endregion

        #region Constructors

        public AlbumTrackAssociationViewModel()
        {

        }

        public AlbumTrackAssociationViewModel(int songCount)
        {
            _bwArtist = new BackgroundWorker();
            _bwArtist.DoWork += bwArtist_DoWork;
            _bwArtist.RunWorkerCompleted += _bwArtist_RunWorkerCompleted;

            _bwAlbum = new BackgroundWorker();
            _bwAlbum.DoWork += bwAlbum_DoWork;
            _bwAlbum.RunWorkerCompleted += _bwAlbum_RunWorkerCompleted;

            _bwAlbumInfo = new BackgroundWorker();
            _bwAlbumInfo.DoWork += bwAlbumInfo_DoWork;
            _bwAlbumInfo.RunWorkerCompleted += _bwAlbumInfo_RunWorkerCompleted;

            _numberOfSongs = songCount;

            SongList = new ObservableCollection<SongAndNumber>();
            ArtistList = new ObservableCollection<LastFMArtist>();
            AlbumList = new ObservableCollection<LastFMAlbum>();
            AlbumArtList = new ObservableCollection<AssociationPicture>();

            ArtistsVisible = false;
            AlbumsVisible = false;

            SelectedArtist = null;
            SelectedAlbum = null;

            OperationState = "Normal";

            CanSubmitEntry = false;

            for (int i = 0; i < songCount; i++)
            {
                SongList.Add(new SongAndNumber() { Name = string.Empty, Number = (i + 1).ToString() });
            }

            ShowAlbumHint = Visibility.Visible;

            RaisePropAll();
        }

        #endregion

        #region Functions

        public void RaisePropAll()
        {
            RaisePropertyChanged("AlbumName");
            RaisePropertyChanged("ArtistName");
            RaisePropertyChanged("SongList");
            RaisePropertyChanged("ArtistList");
            RaisePropertyChanged("AlbumList");
            RaisePropertyChanged("ArtistsVisible");
            RaisePropertyChanged("AlbumsVisible");
        }

        /// <summary>
        /// Will not allow the user to close the window manually if information is not filled out for the album
        /// </summary>
        /// <returns></returns>
        public bool CanCloseWindow()
        {
            StringBuilder sb = new StringBuilder("Fill in a value for the following\n");

            bool ret = true;

            foreach (SongAndNumber san in SongList)
            {
                if (string.IsNullOrWhiteSpace(san.Name))
                {
                    sb.Append("Song Number " + san.Number + "\n");
                    ret = false;
                }
            }

            if (string.IsNullOrWhiteSpace(ArtistName))
            {
                sb.Append("Artist Name\n");
                ret = false;
            }

            if (string.IsNullOrWhiteSpace(AlbumName))
            {
                sb.Append("Album Name\n");
                ret = false;
            }

            if (SongList.Count != _numberOfSongs)
            {
                sb.Append("Need " + _numberOfSongs + " tracks\n");
                ret = false;
            }

            if (!ret)
            {
                MessageBox.Show(sb.ToString());
                CanSubmitEntry = false;
            }
            else
            {
                CanSubmitEntry = true;
            }

            return ret;
        }

        /// <summary>
        /// Fill the SongList with the arr 
        /// Fills SongList with amount of _numberOfSongs
        /// If there is not enough strings in arr then it will add empty strings as the song name
        /// </summary>
        /// <param name="arr"></param>
        public void FillSongList(string[] arr)
        {
            SongList.Clear();

            for (int i = 0; i < _numberOfSongs; i++)
            {
                if (arr.Count() > i)
                {
                    SongList.Add(new SongAndNumber((i + 1).ToString(), arr[i]));
                }
                else
                {
                    SongList.Add(new SongAndNumber((i + 1).ToString(), string.Empty));
                }
            }

            RaisePropertyChanged("SongList");
        }

        /// <summary>
        /// If we aren't already trying to get Artists from LastFM then try to get them using the searchString
        /// </summary>
        /// <param name="searchString"></param>
        public void GetArtists(string searchString)
        {
            if (!_bwArtist.IsBusy)
            {
                LastSearchedArtist = searchString;
                _bwArtist.RunWorkerAsync(searchString);
            }
        }

        public void GetAlbums(string searchString)
        {
            if (!_bwAlbum.IsBusy && SelectedArtist != null)
            {
                LastSearchedAlbum = searchString;
                _bwAlbum.RunWorkerAsync(searchString);
            }
        }

        public void GetAlbumInfo()
        {
            if (!_bwAlbumInfo.IsBusy && SelectedArtist != null && SelectedAlbum != null)
            {
                _bwAlbumInfo.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwArtist_DoWork(object sender, DoWorkEventArgs e)
        {
            RaisePropertyChanged("ArtistBusyVisibility");

            ArtistList = new ObservableCollection<LastFMArtist>(LastFMLookup.ArtistQuery(e.Argument.ToString()));

            if (ArtistList.Count > 0)
            {
                ArtistsVisible = true;
            }
            else
            {
                ArtistsVisible = false;
            }

            RaisePropertyChanged("ArtistList");
            RaisePropertyChanged("ArtistsVisible");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwAlbum_DoWork(object sender, DoWorkEventArgs e)
        {
            RaisePropertyChanged("AlbumBusyVisibility");

            AlbumList = new ObservableCollection<LastFMAlbum>(LastFMLookup.AlbumQuery(e.Argument.ToString(), ArtistName));

            if (AlbumList.Count > 0)
            {
                AlbumsVisible = true;
            }
            else
            {
                AlbumsVisible = false;
            }

            RaisePropertyChanged("AlbumList");
            RaisePropertyChanged("AlbumsVisible");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwAlbumInfo_DoWork(object sender, DoWorkEventArgs e)
        {
            //The album could have changed need to make sure it is the same one we made the query with!!!!
            //USE SELECTEDARTIST NAME OR SELECTEDALBUM ARTIST???
            SelectedAlbum.Songs = LastFMLookup.AlbumInfoQuery(SelectedAlbum.Artist, SelectedAlbum.Name, _numberOfSongs);

            SongList = new ObservableCollection<SongAndNumber>();

            for (int i = 0; i < SelectedAlbum.Songs.Count; i++)
            {
                SongList.Add(new SongAndNumber((i + 1).ToString(), SelectedAlbum.Songs[i]));
            }

            if (SongList.Count < _numberOfSongs)
            {
                AddDummieSongs();
            }

            RaisePropertyChanged("SongList");
        }

        /// <summary>
        /// After getting the artists from LastFM do a check to see if the queried text changed. If so then run another query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _bwArtist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (LastSearchedArtist != ArtistName)
            {
                GetArtists(ArtistName);
            }

            RaisePropertyChanged("ArtistBusyVisibility");
        }

        /// <summary>
        /// After getting the artists from LastFM do a check to see if the queried text changed. If so then run another query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _bwAlbum_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (LastSearchedAlbum != AlbumName)
            {
                GetAlbums(AlbumName);
            }

            RaisePropertyChanged("AlbumBusyVisibility");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _bwAlbumInfo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        /// <summary>
        /// Called when the user selects an artist from the drop down
        /// Updates the Artist Name and 
        /// </summary>
        /// <param name="selectedArtist"></param>
        public void UpdateArtist(LastFMArtist selectedArtist)
        {
            SelectedArtist = selectedArtist;
            ArtistName = selectedArtist.Name;
            ArtistsVisible = false;
            RaisePropertyChanged("ArtistName");
            RaisePropertyChanged("ArtistsVisible");
        }

        /// <summary>
        /// Called when the user selects an album from the drop down
        /// Updates the Artist Name and 
        /// </summary>
        /// <param name="selectedAlbum"></param>
        public void UpdateAlbum(LastFMAlbum selectedAlbum)
        {
            SelectedAlbum = selectedAlbum;
            AlbumName = selectedAlbum.Name;
            AlbumsVisible = false;
            RaisePropertyChanged("AlbumName");
            RaisePropertyChanged("AlbumsVisible");

            AlbumArtList.Add(new AssociationPicture() { Selected = (AlbumArtList.Count == 0), SourceBytes = selectedAlbum.Image});

            GetAlbumInfo();
        }

        public void RenumberSongList()
        {
            foreach (SongAndNumber san in SongList)
            {
                san.Number = (SongList.IndexOf(san) + 1).ToString();
            }
        }

        public void DeleteSong(SongAndNumber item)
        {
            if (SongList.Count == _numberOfSongs)
            {
                return;
            }

            SongList.Remove(item);

            foreach (SongAndNumber san in SongList)
            {
                san.Number = (SongList.IndexOf(san) + 1).ToString();
            }

            RaisePropertyChanged("SongList");
            RaisePropertyChanged("CanRemove");
        }

        public void MergeSelectedTrack()
        {
            if (SongList.Count == _numberOfSongs)
            {
                return;
            }

            if (SelectedSongAndNumber != null && MergeSongAndNumber != null)
            {
                if (!string.IsNullOrWhiteSpace(SelectedSongAndNumber.Name) && !string.IsNullOrWhiteSpace(MergeSongAndNumber.Name))
                {
                    int selectedIndex = SongList.IndexOf(SelectedSongAndNumber);
                    int mergeIndex = SongList.IndexOf(MergeSongAndNumber);
                    if (selectedIndex != -1 && mergeIndex != -1)
                    {
                        if (selectedIndex < mergeIndex)
                        {
                            SongList[selectedIndex].Name += "/" + SongList[mergeIndex].Name;
                            SongList.Remove(SongList[mergeIndex]);
                        }
                        else
                        {
                            SongList[mergeIndex].Name += "/" + SongList[selectedIndex].Name;
                            SongList.Remove(SongList[selectedIndex]);
                        }
                    }
                }
            }

            OperationState = "Normal";

            SelectedSongAndNumber = null;
            MergeSongAndNumber = null;

            RenumberSongList();

            RaisePropertyChanged("OperationState");
            RaisePropertyChanged("SelectedSongAndNumber");
            RaisePropertyChanged("SongList");
            RaisePropertyChanged("CanRemove");
        }

        /// <summary>
        /// If we do not have enough songs from the method of obtaining them
        /// Then we have to add Dummie songs
        /// </summary>
        public void AddDummieSongs()
        {
            if (SongList.Count < _numberOfSongs)
            {
                for (int i = 0; i < _numberOfSongs - SongList.Count; i++)
                {
                    SongList.Add(new SongAndNumber((SongList.Count + 1).ToString(), ""));
                }
            }
        }

        public void Browse()
        {
            try
            {
                var x = new System.Windows.Forms.FolderBrowserDialog();
                var res = x.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    //Clear the binded list
                    SongList.Clear();
                    AlbumArtList.Clear();

                    //Get a list of all the children from the path
                    var mp3List = Directory.GetFiles(x.SelectedPath);

                    mp3List = (from item in mp3List
                               where item.Contains(".mp3")
                               select item).ToArray();

                    foreach (var item in mp3List)
                    {
                        TagLib.File f = TagLib.File.Create(item);
                        SongList.Add(new SongAndNumber(f.Tag.Track.ToString(), f.Tag.Title));
                        //Grabbing last file's Artist and Album
                        AlbumName = f.Tag.Album;
                        ArtistName = f.Tag.FirstArtist; //Deprecated

                        for (int i = 0; i < f.Tag.Pictures.Count(); i++)
                        {
                            var has = (from aa in AlbumArtList
                                       where aa.SourceBytes.SequenceEqual(f.Tag.Pictures[i].Data.Data)
                                       select aa).FirstOrDefault();

                            if (has == null)
                            {
                                AlbumArtList.Add(new AssociationPicture()
                                {
                                    Selected = (AlbumArtList.Count == 0),
                                    SourceBytes = f.Tag.Pictures[i].Data.Data
                                });
                            }
                        }
                    }

                    if (SongList.Count < _numberOfSongs)
                    {
                        AddDummieSongs();
                    }

                    CheckSongListNumbers();

                    RaisePropertyChanged("AlbumName");
                    RaisePropertyChanged("ArtistName");
                    RaisePropertyChanged("SongList");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace, e.Message);
            }
        }

        public void CheckSongListNumbers()
        {
            SongList = new ObservableCollection<SongAndNumber>(SongList.OrderBy(i => i.Number).ToList());

            //If the number does not match the position in the list then set it to it
            //Could do more in here to further improve upon this
            foreach (var item in SongList.Where(item => item.Number != (SongList.IndexOf(item) + 1).ToString()))
            {
                item.Number = (SongList.IndexOf(item) + 1).ToString();
            }
        }

        public void SelectAlbumArt(AssociationPicture ap)
        {
            foreach (var item in AlbumArtList)
            {
                if (item == ap && item.Selected)
                {
                    return;
                }

                if (item == ap && !item.Selected)
                {
                    item.Selected = true;
                }
                else
                {
                    item.Selected = false;
                }
            }
        }

        public void AddAlbumArt(string loc)
        {
            try
            {
                AlbumArtList.Add(new AssociationPicture() { Selected = (AlbumArtList.Count == 0), SourceBytes = System.IO.File.ReadAllBytes(loc) });
            }
            catch (Exception)
            {
                MessageBox.Show("Select an appropriate file to add", "Error");
            }
           
        }

        #endregion

    }
}

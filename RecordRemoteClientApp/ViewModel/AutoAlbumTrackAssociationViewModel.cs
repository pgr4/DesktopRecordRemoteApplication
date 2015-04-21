using System.Data.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Linq.Expressions;

namespace RecordRemoteClientApp.ViewModel
{
    public class AutoAlbumTrackAssociationViewModel : ViewModelBase
    {
        #region Members

        public string ManualAlbumName { get; set; }
        public string ManualArtistName { get; set; }

        private GridLength artistWidthType;

        public GridLength ArtistWidthType
        {
            get { return artistWidthType; }
            set
            {
                artistWidthType = value;
                RaisePropertyChanged("ArtistWidthType");
            }
        }

        private GridLength albumWidthType;

        public GridLength AlbumWidthType
        {
            get { return albumWidthType; }
            set
            {
                albumWidthType = value;
                RaisePropertyChanged("AlbumWidthType");
            }
        }

        /// <summary>
        /// 0: Initial
        /// 1: Artists Displayed
        /// 2: Albums Displayed
        /// 3: Songs Displayed
        /// Controls visibility as well
        /// </summary>
        private int methodLevel;

        public int MethodLevel
        {
            get { return methodLevel; }
            set
            {
                methodLevel = value;
                RaisePropertyChanged("MethodLevel");
            }
        }

        public string QueryResult
        {
            get
            {
                if (IsBrowsing)
                {
                    return "Showing results from " + FolderPath;
                }
                if (SelectedArtist != null && SelectedAlbum != null)
                {
                    return "Showing Songs for " + SelectedArtist.Name + "'s " + SelectedAlbum.Name;
                }
                else if (SelectedArtist == null)
                {
                    return "Showing Artists matching " + QueryArtist;
                }
                else if (SelectedArtist != null)
                {
                    return "Showing Albums for " + QueryArtist;
                }
                else
                {
                    return "";
                }
            }
        }

        public bool IsBrowsing { get; set; }

        public string FolderPath { get; set; }

        public string QueryArtist { get; set; }

        public bool CanGoBack { get { return (HoldingArtists != null || HoldingArtists != null); } }

        private ObservableCollection<GenericPictureName> autoList;

        public ObservableCollection<GenericPictureName> AutoList
        {
            get { return autoList; }
            set
            {
                autoList = value;
                RaisePropertyChanged("AutoList");
            }
        }

        private string switchText;

        public string SwitchText
        {
            get { return switchText; }
            set
            {
                switchText = value;
                RaisePropertyChanged("SwitchText");
            }
        }

        private bool isManual;

        public bool IsManual
        {
            get { return isManual; }
            set
            {
                isManual = value;
                RaisePropertyChanged("IsManual");
            }
        }

        private bool isAutoFill;

        public bool IsAutoFill
        {
            get { return isAutoFill; }
            set
            {
                isAutoFill = value;
                RaisePropertyChanged("IsAutoFill");
            }
        }

        public Visibility ShowAlbumHint
        {
            get
            {
                if (AlbumArtList == null)
                {
                    return Visibility.Visible;
                }
                else if (AlbumArtList.Count == 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        private int SavedMethodLevel { get; set; }

        private int[] _key { get; set; }
        private int _numberOfSongs { get; set; }

        private BackgroundWorker _bwArtist;
        private BackgroundWorker _bwAlbum;
        private BackgroundWorker _bwAlbumInfo;

        public ObservableCollection<SongAndNumber> SongList { get; set; }
        public ObservableCollection<AssociationPicture> AlbumArtList { get; set; }

        public List<GenericPictureName> HoldingArtists { get; set; }
        public List<GenericPictureName> HoldingAlbums { get; set; }

        public bool ArtistsVisible { get; set; }
        public bool AlbumsVisible { get; set; }

        public GenericPictureName SelectedArtist { get; set; }
        public GenericPictureName SelectedAlbum { get; set; }

        private Visibility isBusyVisibility;

        public Visibility IsBusyVisibility
        {
            get { return isBusyVisibility; }
            set
            {
                isBusyVisibility = value;
                RaisePropertyChanged("IsBusyVisibility");
            }
        }

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

        /// <summary>
        /// Constructor
        /// Initialization
        /// </summary>
        /// <param name="na"></param>
        public AutoAlbumTrackAssociationViewModel(NewAlbum na)
        {
            MethodLevel = 0;
            SavedMethodLevel = 0;

            _bwArtist = new BackgroundWorker();
            _bwArtist.DoWork += bwArtist_DoWork;

            _bwAlbum = new BackgroundWorker();
            _bwAlbum.DoWork += bwAlbum_DoWork;

            _bwAlbumInfo = new BackgroundWorker();
            _bwAlbumInfo.DoWork += bwAlbumInfo_DoWork;

            _numberOfSongs = na.Breaks + 1;
            _key = na.Key;

            SongList = new ObservableCollection<SongAndNumber>();
            AlbumArtList = new ObservableCollection<AssociationPicture>();
            AutoList = new ObservableCollection<GenericPictureName>();

            ArtistsVisible = false;
            AlbumsVisible = false;

            SelectedArtist = null;
            SelectedAlbum = null;

            OperationState = "Normal";

            CanSubmitEntry = false;

            FolderPath = "";

            QueryArtist = "";

            IsBrowsing = false;

            for (int i = 0; i < _numberOfSongs; i++)
            {
                SongList.Add(new SongAndNumber() { Name = "Track " + (i + 1), Number = (i + 1).ToString() });
            }

            IsManual = false;
            SwitchText = "Switch to Manual";

            ArtistWidthType = new GridLength(1, GridUnitType.Star);
            AlbumWidthType = new GridLength(1, GridUnitType.Auto);

            RaisePropertyChanged("ShowAlbumHint");
            RaisePropertyChanged("SongList");
            RaisePropertyChanged("ArtistList");
            RaisePropertyChanged("AlbumList");
            RaisePropertyChanged("ArtistsVisible");
            RaisePropertyChanged("AlbumsVisible");
        }

        #endregion

        #region Functions

        #region Reordering Songs

        /// <summary>
        /// Reset the items in SongList to reflect their current index
        /// </summary>
        public void RenumberSongList()
        {
            foreach (SongAndNumber san in SongList)
            {
                san.Number = (SongList.IndexOf(san) + 1).ToString();
            }
        }

        /// <summary>
        /// Method for deleting an item from SongList
        /// Refuse to remove if it will put us under _numberOfSongs
        /// Renumber List after delete
        /// </summary>
        /// <param name="item"></param>
        public void DeleteSong(SongAndNumber item)
        {
            if (SongList.Count == _numberOfSongs)
            {
                return;
            }

            SongList.Remove(item);

            RenumberSongList();

            RaisePropertyChanged("SongList");
            RaisePropertyChanged("CanRemove");
        }

        /// <summary>
        /// Method for merging two tracks
        /// Combines song titles and removes one from the list
        /// Renumbers after
        /// </summary>
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

        /// <summary>
        /// Attempts to order SongList
        /// </summary>
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

        #endregion

        #region Misc

        /// <summary>
        /// Method for changing from manual to automatic
        /// </summary>
        public void ChangeType()
        {
            IsManual = !IsManual;
            if (IsManual)
            {
                SwitchText = "Switch to Automatic";
                AlbumWidthType = new GridLength(1, GridUnitType.Star);
                SavedMethodLevel = MethodLevel;
                MethodLevel = 50;
            }
            else
            {
                SwitchText = "Switch to Manual";
                AlbumWidthType = new GridLength(1, GridUnitType.Auto);
                MethodLevel = SavedMethodLevel;
            }
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
            if (!isManual)
            {
                if (SelectedArtist == null)
                {
                    sb.Append("Artist\n");
                    ret = false;
                }

                if (SelectedAlbum == null)
                {
                    sb.Append("Album\n");
                    ret = false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ManualArtistName))
                {
                    sb.Append("Artist\n");
                    ret = false;
                }

                if (string.IsNullOrWhiteSpace(ManualAlbumName))
                {
                    sb.Append("Album\n");
                    ret = false;
                }
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
        /// Method for getting song information from user data
        /// </summary>
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
                        SelectedAlbum = new GenericPictureName(f.Tag.Album);
                        SelectedArtist = new GenericPictureName(f.Tag.FirstArtist); //Deprecated

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

                    FolderPath = x.SelectedPath;
                    IsBrowsing = true;
                    MethodLevel = 3;
                    RaisePropertyChanged("SongList");
                    RaisePropertyChanged("ShowAlbumHint");
                    RaisePropertyChanged("QueryResult");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace, e.Message);
            }
        }

        /// <summary>
        /// Method for removing albumart from the AlbumArtList
        /// </summary>
        /// <param name="imageBytes"></param>
        public void RemoveAlbumArt(byte[] imageBytes)
        {
            for (int i = 0; i < AlbumArtList.Count; i++)
            {
                if (AlbumArtList[i].SourceBytes.SequenceEqual(imageBytes))
                {
                    if (AlbumArtList[i].Selected)
                    {
                        AlbumArtList.RemoveAt(i);
                        if (AlbumArtList.Count != 0)
                        {
                            AlbumArtList[0].Selected = true;
                        }
                    }
                    else
                    {
                        AlbumArtList.RemoveAt(i);
                        break;
                    }
                }
            }
            RaisePropertyChanged("ShowAlbumHint");
        }

        /// <summary>
        /// Method for setting an item in AlbumArtList to selected
        /// </summary>
        /// <param name="imageBytes"></param>
        public void SelectAlbumArt(byte[] imageBytes)
        {
            foreach (var item in AlbumArtList)
            {
                item.Selected = false;
                if (item.SourceBytes.SequenceEqual(imageBytes))
                {
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        /// Method for setting an item in AlbumArtList to selected
        /// </summary>
        /// <param name="imageBytes"></param>
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

        /// <summary>
        /// Method for adding an item to AlbumArtList
        /// </summary>
        /// <param name="imageBytes"></param>
        public void AddAlbumArt(string loc)
        {
            try
            {
                AlbumArtList.Add(new AssociationPicture()
                {
                    Selected = (AlbumArtList.Count == 0),
                    SourceBytes = System.IO.File.ReadAllBytes(loc),
                    IsUserAdded = true
                });
            }
            catch (Exception)
            {
                MessageBox.Show("Select an appropriate file to add", "Error");

            }
            finally
            {
                RaisePropertyChanged("ShowAlbumHint");
            }
        }

        /// <summary>
        /// Removes all album art that was added by the user
        /// </summary>
        public void RemoveNonUserAddedAlbumArt()
        {
            AlbumArtList = new ObservableCollection<AssociationPicture>((
                from item in AlbumArtList
                where item.IsUserAdded
                select item).ToList());
        }

        /// <summary>
        /// Method for switching the level to the previous
        /// album->artist
        /// song->album
        /// </summary>
        public void GoBack()
        {
            if (IsBrowsing)
            {
                IsBrowsing = false;
                AutoList = new ObservableCollection<GenericPictureName>(HoldingArtists);
                SelectedArtist = null;
                SelectedAlbum = null;
                MethodLevel = 1;
                RaisePropertyChanged("QueryResult");
            }
            else
            {
                IsBrowsing = false;
                switch (MethodLevel)
                {
                    case 2:
                        AutoList = new ObservableCollection<GenericPictureName>(HoldingArtists);
                        SelectedArtist = null;
                        MethodLevel = 1;
                        RaisePropertyChanged("QueryResult");
                        break;
                    case 3:
                        SelectedAlbum = null;
                        AutoList = new ObservableCollection<GenericPictureName>(HoldingAlbums);
                        MethodLevel = 2;
                        RaisePropertyChanged("QueryResult");
                        RemoveNonUserAddedAlbumArt();
                        break;
                }
            }
        }

        #endregion

        #region Background Worker

        /// <summary>
        /// Run bwArtist_DoWork 
        /// </summary>
        public void GetArtists(string searchString)
        {
            if (!_bwArtist.IsBusy)
            {
                QueryArtist = searchString;
                _bwArtist.RunWorkerAsync(searchString);
            }
        }

        /// <summary>
        /// Run bwAlbum_DoWork 
        /// </summary>
        private void GetAlbums()
        {
            if (!_bwAlbum.IsBusy)
            {
                _bwAlbum.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Run bwAlbumInfo_DoWork 
        /// </summary>
        private void GetAlbumInfo()
        {
            if (!_bwAlbumInfo.IsBusy)
            {
                _bwAlbumInfo.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Thread work for getting artists from online source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwArtist_DoWork(object sender, DoWorkEventArgs e)
        {
            MethodLevel += 10;
            AutoList = new ObservableCollection<GenericPictureName>(LastFMLookup.ArtistQuery(e.Argument.ToString()));
            HoldingArtists = new List<GenericPictureName>(AutoList);
            MethodLevel = 1;
            RaisePropertyChanged("AutoList");
            RaisePropertyChanged("QueryResult");
        }

        /// <summary>
        /// Thread work for getting albums from online source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwAlbum_DoWork(object sender, DoWorkEventArgs e)
        {
            MethodLevel += 10;
            AutoList = new ObservableCollection<GenericPictureName>(LastFMLookup.AlbumQuery(SelectedArtist.Name));
            HoldingAlbums = new List<GenericPictureName>(AutoList);
            MethodLevel = 2;
            RaisePropertyChanged("AutoList");
            RaisePropertyChanged("QueryResult");
        }

        /// <summary>
        /// Thread work for getting song info from online source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwAlbumInfo_DoWork(object sender, DoWorkEventArgs e)
        {
            MethodLevel += 10;
            List<string> lst = LastFMLookup.AlbumInfoQuery(SelectedArtist.Name, SelectedAlbum.Name, _numberOfSongs);

            SongList = new ObservableCollection<SongAndNumber>();

            for (int i = 0; i < lst.Count; i++)
            {
                SongList.Add(new SongAndNumber((i + 1).ToString(), lst[i]));
            }

            if (SongList.Count < _numberOfSongs)
            {
                AddDummieSongs();
            }

            MethodLevel = 3;
            RaisePropertyChanged("SongList");
            RaisePropertyChanged("QueryResult");
        }

        /// <summary>
        /// Called after double clicking an artist.
        /// Set the artist and setup for album detection
        /// </summary>
        /// <param name="gpn"></param>
        public void SetSelectedArtist(GenericPictureName gpn)
        {
            MethodLevel = 1;
            SelectedArtist = gpn;

            SelectedAlbum = null;
            AutoList.Clear();

            GetAlbums();
        }

        /// <summary>
        /// Called after double clicking an album.
        /// Set the album and display songs
        /// </summary>
        /// <param name="gpn"></param>
        public void SetSelectedAlbum(GenericPictureName gpn)
        {
            MethodLevel = 2;
            SelectedAlbum = gpn;
            AutoList.Clear();
            AlbumArtList.Add(new AssociationPicture() { Selected = (AlbumArtList.Count == 0), SourceBytes = SelectedAlbum.ImgBytes, IsUserAdded = false });
            RaisePropertyChanged("ShowAlbumHint");
            GetAlbumInfo();
        }

        #endregion

        #endregion
    }
}

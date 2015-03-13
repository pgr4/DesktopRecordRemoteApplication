using System.Xml.Linq;
using System.Xml.XPath;
using RecordRemoteClientApp.Models.LastFM;
using RecordRemoteClientApp.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace RecordRemoteClientApp.Models
{
    static public class LastFMLookup
    {
        private const string StartQuery = "http://ws.audioscrobbler.com/2.0/?";
        private const string EndQuery = "&api_key=557e6ea3fad888bd915f713613941051";

        public static List<GenericPictureName> ArtistQuery(string queryString)
        {
            try
            {
                List<GenericPictureName> FMArtistList = new List<GenericPictureName>();
                string tot = StartQuery + QueryType.Artist.ToDescriptionString() + WebUtility.UrlEncode(queryString) + EndQuery;
                var request = WebRequest.Create(tot);
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

                XElement sd = XElement.Parse(text);

                var list = (from x in sd.Element("results").Element("artistmatches").Elements()
                            select x).ToList();

                foreach (var item in list)
                {
                    if (IsAble(item.Element("name").Value.ToLower(), queryString.ToLower()))
                    {
                        FMArtistList.Add(new GenericPictureName(item.Element("name").Value, item.Element("image").Value));
                    }
                }

                return FMArtistList.OrderBy(i => i.Name).ToList();
            }
            catch
            {
                return new List<GenericPictureName>();
            }
        }

        public static List<GenericPictureName> AlbumQuery(string queryString)
        {
            try
            {
                List<GenericPictureName> FMAlbumList = new List<GenericPictureName>();
                string tot = StartQuery + QueryType.Album.ToDescriptionString() + WebUtility.UrlEncode(queryString) + EndQuery;
                var request = WebRequest.Create(tot);
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

                XElement sd = XElement.Parse(text);

                var list = (from x in sd.Element("topalbums").Elements()
                            select x).ToList();

                var webClient = new WebClient();
                foreach (var item in list)
                {
                    FMAlbumList.Add(new GenericPictureName(item.Element("name").Value, item.Elements("image").FirstOrDefault(i => i.Attribute("size").Value == "extralarge").Value));
                }

                return FMAlbumList.OrderBy(i => i.Name).ToList();
            }
            catch
            {
                return new List<GenericPictureName>();
            }
        }

        public static List<string> AlbumInfoQuery(string queryArtistString, string queryAlbumString, int songCount)
        {
            try
            {
                List<string> FMTrackList = new List<string>();
                string tot = StartQuery + QueryType.AlbumInfo.ToDescriptionString() + "artist=" + WebUtility.UrlEncode(queryArtistString) + "&" + "album=" + WebUtility.UrlEncode(queryAlbumString) + "&" + EndQuery;
                var request = WebRequest.Create(tot);
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

                XElement sd = XElement.Parse(text);

                var list = (from x in sd.Element("album").Element("tracks").Elements()
                            select x).ToList();

                foreach (var item in list)
                {
                    FMTrackList.Add(item.Element("name").Value);
                }

                return FMTrackList;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsAble(string compareString, string filterString)
        {
            for (int i = 0; i < filterString.Length; i++)
            {
                if (filterString[i] != compareString[i])
                {
                    return false;
                }
            }

            return true;
        }

    }
}

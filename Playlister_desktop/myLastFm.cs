using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Newtonsoft.Json;

namespace Playlister
{
    public static class myLastFm
    {
        public interface ITrackResults
        {
            List<Track> tracklist { get; set; }
            int page { get; set; }
            int perPage { get; }
            int totalPages { get; set; }
            int total { get; set; }


        }

        public interface ILibrary
        {
            HashSet<Track> library { get; set; }

            int addPage();
            int turnPage();

        }

        public class Streamable
        {
            public string text { get; set; }
            public string fulltrack { get; set; }
        }

        public class Image
        {
            public string text { get; set; }
            public string size { get; set; }
        }

        public class Artist
        {
            public string name { get; set; }
            public string mbid { get; set; }
            public string url { get; set; }

            public Artist(string x)
            {
                this.name = x;
            }
        }

        public class Album
        {
            public string artist { get; set; }
            public string title { get; set; }
            public string mbid { get; set; }
            public string url { get; set; }
            public List<Image> image { get; set; }
            public Attr attr { get; set; }
            public string text { get; set; }
        }

        public class Date
        {
            public string uts { get; set; }
            public string size { get; set; }

        }

        public class Track
        {
            public string name { get; set; }
            public string duration { get; set; }
            public string mbid { get; set; }
            public string url { get; set; }
            //TODO: change how I'm deserializing so I can parse these?
            //public string streamable { get; set; }
            //public Streamable streamable { get; set; }
            public int listeners { get; set; }
            public int playcount { get; set; }
            public Artist artist { get; set; }
            public Album album { get; set; }
            public Toptags toptags { get; set; }
            public List<Image> image { get; set; }
            public Attr attr { get; set; }
            public Wiki wiki { get; set; }
            public Date date { get; set; }

            public Track(string a, string s)
            {
                this.name = s;
                this.artist = new Artist(a);
                this.getInfo();
            }

            public Track(string m)
            {
                this.mbid = m;
                this.getInfo();
            }

            public void getInfo()
            {
                string req;
                trackRootObject trackInfo = new trackRootObject();

                if (this.mbid != null && this.mbid != "")
                {
                    req = myLastFm.prepRequest("method=track.getInfo&mbid=" + this.mbid);
                    trackInfo = JsonConvert.DeserializeObject<trackRootObject>(lastFmJsonReq(req));


                }
                else
                {
                    req = myLastFm.prepRequest("method=track.getInfo&artist="+this.artist.name+"&track="+this.name);
                    trackInfo = JsonConvert.DeserializeObject<trackRootObject>(lastFmJsonReq(req));
                }

                this.listeners = trackInfo.track.listeners;
                this.playcount = trackInfo.track.listeners;
                this.artist = trackInfo.track.artist;
                this.album = trackInfo.track.album;
                this.toptags = trackInfo.track.toptags;
                this.image = trackInfo.track.image;
                this.attr = trackInfo.track.attr;
                this.wiki = trackInfo.track.wiki;
                this.date = trackInfo.track.date;
            }
        }

        public class TrackComparer : IEqualityComparer<Track>
        {
            // Products are equal if their album and artist are equal.
            public bool Equals(Track x, Track y)
            {

                AlbumComparer albumC = new AlbumComparer();
                ArtistComparer artistC = new ArtistComparer();

                //Check whether the compared objects reference the same object.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                //if (x.mbid == "" || y.mbid == "")
                //    return false;
                //else
                //    return x.mbid == y.mbid;

                return x.name == y.name & artistC.Equals(x.artist, y.artist) & albumC.Equals(x.album, y.album);

            }

            public int GetHashCode(Track song)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(song, null)) return 0;

                AlbumComparer albumC = new AlbumComparer();
                ArtistComparer artistC = new ArtistComparer();

                //Get hash code for the Name field if it is not null.
                int hashMBID = song.name.GetHashCode() * albumC.GetHashCode(song.album) * artistC.GetHashCode(song.artist);

                //Calculate the hash code for the product.
                return hashMBID;
            }

       
        }

        public class TagComparer : IEqualityComparer<Tag>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(Tag x, Tag y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                if (x.name == "" || y.name == "")
                    return false;
                else
                    return x.name == y.name;
            }

            public int GetHashCode(Tag tag)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(tag, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashMBID = tag.name == null ? 0 : tag.name.GetHashCode();

                //Calculate the hash code for the product.
                return hashMBID;
            }

        }

        public class ArtistComparer : IEqualityComparer<Artist>
        {
            // Products are equal if their names are equal.
            public bool Equals(Artist x, Artist y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                if (x.name == "" || y.name == "")
                    return false;
                else
                    return x.name == y.name;
            }

            public int GetHashCode(Artist artist)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(artist, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashArt = artist.name == null ? 0 : artist.name.GetHashCode();

                //Calculate the hash code for the product.
                return hashArt;
            }

        }

        public class AlbumComparer : IEqualityComparer<Album>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(Album x, Album y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                if (x.title == "" || y.title == "")
                    return false;
                else
                    return x.title == y.title & ArtistComparer.Equals(x.artist,y.artist);
            }

            public int GetHashCode(Album album)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(album, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashAlb = string.Concat(album.title,"|",album.artist) == null ? 0 : string.Concat(album.title,"|",album.artist).GetHashCode();

                //Calculate the hash code for the product.
                return hashAlb;
            }

        }

        public class Tag : ITrackResults
        {
            public string name { get; set; }
            public string url { get; set; }
            public List<Track> tracklist { get; set; }
            public int page { get; set; }
            public int perPage {
                get
                {
                    return 500;
                }
            }
            public int totalPages { get; set; }
            public int total { get; set; }

            public Tag(string s)
            {
                name = s;
            }

            private compare compMode = compare.none;
            private int playcount = 0;

            public Tag()
            {
                tracklist = new List<Track>();
            }

            public int addPage()
            {

                page++;

                if (page > totalPages)
                    return -1;
           
                string req = myLastFm.prepRequest("method=tag.gettoptracks&limit=500&page=" + page.ToString());
                TrackResults trackResults = (TrackResults)JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                totalPages = trackResults.totalPages;

                if ((compMode == compare.less | compMode == compare.exact) & trackResults.tracklist.Last().playcount > playcount)
                {
                    //skip this page
                    return addPage();
                }
                else if ((compMode == compare.greater | compMode == compare.exact) & trackResults.tracklist.First().playcount < playcount)
                {
                    //ya done
                    return -1;
                }
                else if ((compMode == compare.less & trackResults.tracklist.First().playcount < playcount) | compMode == null)
                {
                    //add the entire page
                    tracklist.AddRange(trackResults.tracklist);
                    return 0;
                }
                else if (compMode == compare.greater & trackResults.tracklist.Last().playcount > playcount)
                {
                    //add the entire page
                    tracklist.AddRange(trackResults.tracklist);
                    return 0;
                }
                else
                {
                    foreach (myLastFm.Track song in trackResults.tracklist)
                    {
                        if (compMode == compare.less & song.playcount < playcount)
                            tracklist.Add(song);
                        else if (compMode == compare.greater & song.playcount > playcount)
                            tracklist.Add(song);
                        else if (compMode == compare.exact & song.playcount == playcount)
                            tracklist.Add(song);
                    }
                    return 0;
                }

            }

            public int turnPage()
            {
                page++;

                if (page > totalPages)
                    return -1;

                tracklist = new List<Track>();

                string req = myLastFm.prepRequest("method=chart.gettoptracks&limit=500&page=" + page.ToString());
                TrackResults trackResults = (TrackResults)JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                totalPages = trackResults.totalPages;

                if ((compMode == compare.less | compMode == compare.exact) & trackResults.tracklist.Last().playcount > playcount)
                {
                    //skip this page
                    return addPage();
                }
                else if ((compMode == compare.greater | compMode == compare.exact) & trackResults.tracklist.First().playcount < playcount)
                {
                    //ya done
                    return -1;
                }
                else if ((compMode == compare.less & trackResults.tracklist.First().playcount < playcount) | compMode == null)
                {
                    //add the entire page
                    tracklist.AddRange(trackResults.tracklist);
                    return 0;
                }
                else if (compMode == compare.greater & trackResults.tracklist.Last().playcount > playcount)
                {
                    //add the entire page
                    tracklist.AddRange(trackResults.tracklist);
                    return 0;
                }
                else
                {
                    foreach (myLastFm.Track song in trackResults.tracklist)
                    {
                        if (compMode == compare.less & song.playcount < playcount)
                            tracklist.Add(song);
                        else if (compMode == compare.greater & song.playcount > playcount)
                            tracklist.Add(song);
                        else if (compMode == compare.exact & song.playcount == playcount)
                            tracklist.Add(song);
                    }
                    return 0;
                }

            }

        }

        public class Toptags
        {
            public List<Tag> tag { get; set; }
        }

        public class Wiki
        {
            public string published { get; set; }
            public string summary { get; set; }
            public string content { get; set; }
        }

        public class Attr
        {
            public string rank { get; set; }
            public string tag { get; set; }
            public int page { get; set; }
            public int perPage { get; set; }
            public int totalPages { get; set; }
            public int total { get; set; }
            public string position { get; set; }
            public string user { get; set; }
            public string artist { get; set; }
        }

        public class Tracks
        {
            public List<Track> tracks { get; set; }
            public List<Track> track { get; set; }
            public Attr attr { get; set; }

        }

        public class tracksRootObject 
        {
            public Tracks tracks { get; set; }
            public Tracks toptracks { get; set; }
            public Attr attr { get; set; }
            public Tracks artisttracks { get; set; }

            public static explicit operator TrackResults(tracksRootObject x)
            {
                TrackResults tr = new TrackResults();
                
                //god their results are so inconsistent
                //tracklist
                if (x.tracks != null) {
                     if(x.tracks.tracks != null)
                        tr.tracklist = x.tracks.tracks;
                    if(x.tracks.track != null)
                        tr.tracklist = x.tracks.track;
                    if(x.tracks.attr != null){
                        tr.total = x.tracks.attr.total;
                        tr.totalPages = x.tracks.attr.totalPages;
                        tr.page = x.tracks.attr.page;
                    }
                }
                if (x.toptracks != null) {
                     if(x.toptracks.tracks != null)
                        tr.tracklist = x.toptracks.tracks;
                    if(x.toptracks.track != null)
                        tr.tracklist = x.toptracks.track;
                    if(x.toptracks.attr != null){
                        tr.total = x.toptracks.attr.total;
                        tr.totalPages = x.toptracks.attr.totalPages;
                        tr.page = x.toptracks.attr.page;
                    }
                }
                if (x.artisttracks != null) {
                     if(x.artisttracks.tracks != null)
                        tr.tracklist = x.artisttracks.tracks;
                    if(x.artisttracks.track != null)
                        tr.tracklist = x.artisttracks.track;
                    if(x.artisttracks.attr != null){
                        tr.total = x.artisttracks.attr.total;
                        tr.totalPages = x.artisttracks.attr.totalPages;
                        tr.page = x.artisttracks.attr.page;
                    }
                }


                return tr;

            }
        }

        public class trackRootObject
        {
            public Track track { get; set; }

            public static explicit operator TrackResults(trackRootObject x)
            {
                TrackResults tr = new TrackResults();
                tr.tracklist = new List<Track>(1);
                tr.tracklist.Add(x.track);
                tr.total = 1;
                tr.page = 1;
                tr.totalPages = 1;
                return tr;
            }

        }

        //TODO: simplify api interations via more custom classes
        public class TrackResults : ITrackResults
        {
            public List<Track> tracklist { get; set; }
            public int page { get; set; }
            public int perPage {
                get
                {
                    return 500;
                }
            }
            public int totalPages { get; set; }
            public int total { get; set; }

        }

        public class Chart : ILibrary
        {
            public HashSet<Track> library { get; set; }
            private int page = 0;
            private int totalPages = 1;
            private compare compMode = compare.none;
            private int playcount = 0;
            public int perPage
            {
                get
                {
                    return 500;
                }
            }

            public Chart(compare param_comparison = compare.none, int param_playcount = 0)
            {
                compMode = param_comparison;
                playcount = param_playcount;
                library = new HashSet<Track>();
            }

            public int addPage()
            {

                page++;

                if (page > totalPages)
                    return -1;

                //library = new HashSet<Track>();
           
                string req = myLastFm.prepRequest("method=chart.gettoptracks&limit=500&page=" + page.ToString());
                TrackResults trackResults = (TrackResults)JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                totalPages = trackResults.totalPages;

                if ((compMode == compare.less | compMode == compare.exact) & trackResults.tracklist.Last().playcount > playcount)
                {
                    //skip this page
                    return addPage();
                }
                else if ((compMode == compare.greater | compMode == compare.exact) & trackResults.tracklist.First().playcount < playcount)
                {
                    //ya done
                    return -1;
                }
                else if ((compMode == compare.less & trackResults.tracklist.First().playcount < playcount) | compMode == null)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else if (compMode == compare.greater & trackResults.tracklist.Last().playcount > playcount)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else
                {
                    foreach (myLastFm.Track song in trackResults.tracklist)
                    {
                        if (compMode == compare.less & song.playcount < playcount)
                            library.Add(song);
                        else if (compMode == compare.greater & song.playcount > playcount)
                            library.Add(song);
                        else if (compMode == compare.exact & song.playcount == playcount)
                            library.Add(song);
                    }
                    return 0;
                }

            }

            public int turnPage()
            {
                page++;

                if (page > totalPages)
                    return -1;

                library = new HashSet<Track>();

                string req = myLastFm.prepRequest("method=chart.gettoptracks&limit=500&page=" + page.ToString());
                TrackResults trackResults = (TrackResults)JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                totalPages = trackResults.totalPages;

                if ((compMode == compare.less | compMode == compare.exact) & trackResults.tracklist.Last().playcount > playcount)
                {
                    //skip this page
                    return addPage();
                }
                else if ((compMode == compare.greater | compMode == compare.exact) & trackResults.tracklist.First().playcount < playcount)
                {
                    //ya done
                    return -1;
                }
                else if ((compMode == compare.less & trackResults.tracklist.First().playcount < playcount) | compMode == null)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else if (compMode == compare.greater & trackResults.tracklist.Last().playcount > playcount)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else
                {
                    foreach (myLastFm.Track song in trackResults.tracklist)
                    {
                        if (compMode == compare.less & song.playcount < playcount)
                            library.Add(song);
                        else if (compMode == compare.greater & song.playcount > playcount)
                            library.Add(song);
                        else if (compMode == compare.exact & song.playcount == playcount)
                            library.Add(song);
                    }
                    return 0;
                }

            }

        }

        public class AppUser : ILibrary
        {
            public HashSet<Track> library { get; set; }
            private int page = 0;
            private int totalPages = 1;
            private compare compMode = compare.none;
            private int playcount = 0;
            public int perPage
            {
                get
                {
                    return 500;
                }
            }

            private string username = Properties.Settings.Default.keyUser;

            public AppUser(compare param_comparison, int param_playcount = 0)
            {
                compMode = param_comparison;
                playcount = param_playcount;
                library = new HashSet<Track>();
            }

            public AppUser()
            {
                library = new HashSet<Track>();
            }

            public int addPage()
            {

                page++;

                if (page > totalPages)
                    return -1;

                //library = new HashSet<Track>();
           
                string req = myLastFm.prepRequest("method=user.gettoptracks&user=" + username + "&period=overall&limit=500&page=" + page.ToString());
                TrackResults trackResults = (TrackResults)JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                totalPages = trackResults.totalPages;

                if ((compMode == compare.less | compMode == compare.exact) & trackResults.tracklist.Last().playcount > playcount)
                {
                    //skip this page
                    return addPage();
                }
                else if ((compMode == compare.greater | compMode == compare.exact) & trackResults.tracklist.First().playcount < playcount)
                {
                    //ya done
                    return -1;
                }
                else if ((compMode == compare.less & trackResults.tracklist.First().playcount < playcount) | compMode == compare.none)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else if (compMode == compare.greater & trackResults.tracklist.Last().playcount > playcount)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else
                {
                    foreach (myLastFm.Track song in trackResults.tracklist)
                    {
                        if (compMode == compare.less & song.playcount < playcount)
                            library.Add(song);
                        else if (compMode == compare.greater & song.playcount > playcount)
                            library.Add(song);
                        else if (compMode == compare.exact & song.playcount == playcount)
                            library.Add(song);
                    }
                    return 0;
                }

            }

            public int turnPage()
            {
                page++;

                if (page > totalPages)
                    return -1;

                library = new HashSet<Track>();

                string req = myLastFm.prepRequest("method=user.gettoptracks&user=" + username + "&period=overall&limit=500&page=" + page.ToString());
                TrackResults trackResults = (TrackResults)JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                totalPages = trackResults.totalPages;

                if ((compMode == compare.less | compMode == compare.exact) & trackResults.tracklist.Last().playcount > playcount)
                {
                    //skip this page
                    return addPage();
                }
                else if ((compMode == compare.greater | compMode == compare.exact) & trackResults.tracklist.First().playcount < playcount)
                {
                    //ya done
                    return -1;
                }
                else if ((compMode == compare.less & trackResults.tracklist.First().playcount < playcount) | compMode == null)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else if (compMode == compare.greater & trackResults.tracklist.Last().playcount > playcount)
                {
                    //add the entire page
                    library.UnionWith(trackResults.tracklist);
                    return 0;
                }
                else
                {
                    foreach (myLastFm.Track song in trackResults.tracklist)
                    {
                        if (compMode == compare.less & song.playcount < playcount)
                            library.Add(song);
                        else if (compMode == compare.greater & song.playcount > playcount)
                            library.Add(song);
                        else if (compMode == compare.exact & song.playcount == playcount)
                            library.Add(song);
                    }
                    return 0;
                }

            }


        }
       

        public static string prepRequest(string request){
            return Properties.Settings.Default.apiAddr + request + "&api_key=" + Properties.Settings.Default.apiKey;
        }

        public static void getAuthSession(bool force)
        {
            if (Properties.Settings.Default.sessionKey != "" && !force)
            {
                Console.WriteLine("Stored Key for User " + Properties.Settings.Default.keyUser + ": " + Properties.Settings.Default.sessionKey);
                return;
            }

            string mytok = "";
            string sessionKey = "";

            string request = prepRequest("method=auth.getToken" ) + "&api_sig=" + helpers.getSig("api_key" + Properties.Settings.Default.apiKey + "methodauth.getToken");

            XmlDocument xDoc = myLastFm.lastFmXMLReq(request);

            XmlNodeList xnList = xDoc.SelectNodes("/lfm");

            foreach (XmlNode xn in xnList)
            {
                mytok = xn["token"].InnerText;
                Console.WriteLine("Token:" + mytok);
            }

            // need to wait after this somehow...
            Process.Start("http://www.last.fm/api/auth?api_key=" + Properties.Settings.Default.apiKey + "&token=" + mytok);
            MessageBoxResult waiter = MessageBox.Show("Your default web brower is now launching. Please click below when you have allowed this program to modify your account (add a playlist).");

            // Do it all again for sessionKey

            request = prepRequest("method=auth.getSession&token=" + mytok) + "&api_sig=" + helpers.getSig("api_key" + Properties.Settings.Default.apiKey + "methodauth.getSessiontoken" + mytok);

            xDoc = myLastFm.lastFmXMLReq(request);

            xnList = xDoc.SelectNodes("/lfm/session");

            foreach (XmlNode xn in xnList)
            {
                sessionKey = xn["key"].InnerText;
                Console.WriteLine("Key:" + sessionKey);
            }

            Properties.Settings.Default.sessionKey = sessionKey;
            Properties.Settings.Default.Save(); //save immediately, don't wait for close

        }

        public static XmlDocument lastFmXMLReq(string reqString)
        {
            Console.WriteLine("Request String: " + reqString);
            WebRequest lastfm = WebRequest.Create(reqString);
            lastfm.Method = "GET";
            // Get the response.
            try
            {
                HttpWebResponse response = (HttpWebResponse)lastfm.GetResponse();
                // Display the status.
                Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content. 
                string apiResponse = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine(apiResponse);
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();

                // Parse string as xml
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(apiResponse);

                return xDoc;
            }
            catch (WebException webExcp)
            {
                // If you reach this point, an exception has been caught.
                Console.WriteLine("A WebException has been caught.");
                // Write out the WebException message.
                Console.WriteLine(webExcp.ToString());
                // Get the WebException status code.
                WebExceptionStatus status = webExcp.Status;
                // If status is WebExceptionStatus.ProtocolError, 
                //   there has been a protocol error and a WebResponse 
                //   should exist. Display the protocol error.
                if (status == WebExceptionStatus.ProtocolError)
                {
                    Console.Write("The server returned protocol error ");
                    // Get HttpWebResponse so that you can check the HTTP status code.
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                    Console.WriteLine((int)httpResponse.StatusCode + " - "
                       + httpResponse.StatusCode);
                }

                return null;
            }

        }

        public static string lastFmJsonReq(string reqString)
        {
            reqString = reqString+"&format=json";
            //Console.WriteLine("Request String: " + reqString);
            WebRequest lastfm = WebRequest.Create(reqString);
            lastfm.Method = "GET";
            // Get the response.
            try
            {
                HttpWebResponse response = (HttpWebResponse)lastfm.GetResponse();
                // Display the status.
                //Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content. 
                string apiResponse = reader.ReadToEnd();
                // Display the content.
                //Console.WriteLine(apiResponse);
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();

                return apiResponse.Replace("@attr","attr").Replace("#text","text");
            }
            catch (WebException webExcp)
            {
                // If you reach this point, an exception has been caught.
                Console.WriteLine("A WebException has been caught.");
                // Write out the WebException message.
                Console.WriteLine(webExcp.ToString());
                // Get the WebException status code.
                WebExceptionStatus status = webExcp.Status;
                // If status is WebExceptionStatus.ProtocolError, 
                //   there has been a protocol error and a WebResponse 
                //   should exist. Display the protocol error.
                if (status == WebExceptionStatus.ProtocolError)
                {
                    Console.Write("The server returned protocol error ");
                    // Get HttpWebResponse so that you can check the HTTP status code.
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                    Console.WriteLine((int)httpResponse.StatusCode + " - "
                       + httpResponse.StatusCode);
                }

                return null;
            }

        }


    }
}

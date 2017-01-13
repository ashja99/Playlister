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
        }

        public class TrackComparer : IEqualityComparer<Track>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(Track x, Track y)
            {

                AlbumComparer albumC = new AlbumComparer();
                ArtistComparer artistC = new ArtistComparer();

                //Check whether the compared objects reference the same data.
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
            // Products are equal if their names and product numbers are equal.
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
                int hashAlb = album.title == null ? 0 : album.title.GetHashCode();

                //Calculate the hash code for the product.
                return hashAlb;
            }

        }

        public class Tag
        {
            public string name { get; set; }
            public string url { get; set; }

            public Tag(string s)
            {
                name = s;
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
        }

        public class trackRootObject
        {
            public Track track { get; set; }
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

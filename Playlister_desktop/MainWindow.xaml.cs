using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml;
using Newtonsoft.Json;

namespace Playlister
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// This is where I decided to put all "helper" 
    /// functions that are directly related to the 
    /// last.fm api. Other helper functions are in
    /// customFunctions

    public partial class MainWindow: Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            Switcher.Switch(new getUser());
        }

        public void Navigate(UserControl nextpage)
        {
            this.Content = nextpage;
        }

        public void Navigate(UserControl nextpage, object state)
        {
            this.Content = nextpage;
            ISwitchable s = nextpage as ISwitchable;

            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! "
                    + nextpage.Name.ToString());
        }

        public static void getAuthSession()
        {
            myLastFm.getAuthSession(false);
        }

        public static void generatePlaylist(helpers.myParamArray paramArray, string outType, string playName,int limit)
        {
            List<myLastFm.Track> playlist = new List<myLastFm.Track>(limit);
            HashSet<myLastFm.Track> userLibrary = new HashSet<myLastFm.Track>();
            myLastFm.tracksRootObject trackResults;
            string req = "";

            // If scope is relavent or plays by me is selected, might as well start by getting the necessary tracks from the users library.
            if (paramArray.scope != null | paramArray.playedby == "me")
            {
                int page = 1;
                int totalPages = 1;
                while (page <= totalPages)
                {
                    req = myLastFm.prepRequest("method=user.gettoptracks&user=" + Properties.Settings.Default.keyUser + "&period=overall&limit=500&page=" + page.ToString());
                    trackResults = JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                    totalPages = trackResults.toptracks.attr.totalPages;

                    // and with this any user playcount requirements are handled
                    if (paramArray.playedby == "me")
                    {

                        if (paramArray.comparison == "less than" & trackResults.toptracks.track.Last().playcount > paramArray.playcount)
                        {
                            ///do nothing, aka skip this page
                        }
                        if (paramArray.comparison == "more than" & trackResults.toptracks.track.First().playcount < paramArray.playcount)
                        { 
                            //ya done
                            break;
                        }
                        else if (paramArray.comparison == "less than" & trackResults.toptracks.track.First().playcount < paramArray.playcount)
                        {
                            //add the entire page
                            userLibrary.UnionWith(trackResults.toptracks.track);
                        }
                        else if (paramArray.comparison == "more than" & trackResults.toptracks.track.Last().playcount > paramArray.playcount)
                        {
                            //add the entire page
                            userLibrary.UnionWith(trackResults.toptracks.track);
                        }
                        else  {
                            foreach (myLastFm.Track song in trackResults.toptracks.track)
                            {
                                if (paramArray.comparison == "less than" & song.playcount < paramArray.playcount )
                                    userLibrary.Add(song);
                                else if (paramArray.comparison == "more than" & song.playcount > paramArray.playcount )
                                    userLibrary.Add(song);
                                else if (paramArray.comparison == "exactly" & song.playcount == paramArray.playcount )
                                    userLibrary.Add(song);
                            }
                        }
                    }
                    else {
                        userLibrary.UnionWith(trackResults.toptracks.track);
                    }
                    
                    page++;
 
                }
                    
            }

            //some tag requirements
            //TODO
            if(paramArray.tags != null){
                List<myLastFm.Track> bList = new List<myLastFm.Track>(limit);

                if (paramArray.scope == false && paramArray.playedby != "me")
                {
                    foreach (myLastFm.Tag tag in paramArray.tags)
                    {
                        req = myLastFm.prepRequest("method=tag.gettoptracks&tag=" + tag);
                        trackResults = JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                    }
                }
                if (paramArray.scope == true || paramArray.playedby == "me")
                {
                    foreach (myLastFm.Track song in userLibrary)
                    {
                        if (song.mbid != "")
                        {
                            req = myLastFm.prepRequest("method=track.getInfo&mbid=" + song.mbid);
                            myLastFm.trackRootObject trackInfo = JsonConvert.DeserializeObject<myLastFm.trackRootObject>(myLastFm.lastFmJsonReq(req));
                            if (trackInfo.track != null && trackInfo.track.toptags != null && trackInfo.track.toptags.tag != null)
                            {
                                if (paramArray.playedby == "me")
                                {

                                }
                                else if (paramArray.playedby == "world")
                                {

                                }
                                else
                                { //played by is null
                                    if (paramArray.tags.Except(trackInfo.track.toptags.tag, new myLastFm.TagComparer()).Any() == false)
                                    {
                                        playlist.Add(song);
                                    }
                                    else if (bList.Count < limit && paramArray.tags.Count > 1)
                                    {
                                        foreach (myLastFm.Tag tag in paramArray.tags)
                                        {
                                            if (trackInfo.track.toptags.tag.Contains(tag, new myLastFm.TagComparer()))
                                            {
                                                bList.Add(song);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (playlist.Count >= limit)
                            break;

                    }

                    if (playlist.Count < limit)
                    {
                        if (bList.Count >= limit - playlist.Count)
                        {
                            playlist.AddRange(bList.GetRange(0, limit - playlist.Count));
                        }
                        else
                        {
                            playlist.AddRange(bList.GetRange(0, bList.Count));
                        }
                    }

                }
                
            }
            //no tag requirements, some scope requirement, ? playcount requirements
            else if (paramArray.scope != null) 
            {
                if (paramArray.scope == true)
                {
                    //no tag requirements, scope is "in", user playcounts are already taken care while populating userLibrary
                    if (paramArray.playedby == "world")
                    {
                        foreach (myLastFm.Track song in userLibrary)
                        {
                            if (song.mbid != "")
                            {
                                req = myLastFm.prepRequest("method=track.getInfo&mbid=" + song.mbid);
                                myLastFm.trackRootObject trackInfo = JsonConvert.DeserializeObject<myLastFm.trackRootObject>(myLastFm.lastFmJsonReq(req));
                                if (paramArray.comparison == "exactly" & trackInfo.track.playcount == paramArray.playcount)
                                    playlist.Add(song);
                                else if (paramArray.comparison == "more than" & trackInfo.track.playcount > paramArray.playcount)
                                    playlist.Add(song);
                                else if (paramArray.comparison == "less than" & trackInfo.track.playcount < paramArray.playcount)
                                    playlist.Add(song);

                                if (playlist.Count >= limit)
                                    return;
                            }
                            
                        }
                    }
                    else
                    {
                        if (userLibrary.Count <= limit)
                            playlist = userLibrary.ToList();
                        else
                        {
                            foreach (myLastFm.Track song in userLibrary) {
                                playlist.Add(song);
                                if (playlist.Count == limit)
                                    break;
                            }
                        }

                        
                    }
                        
                }
                else //no tag requirements, scope is "not in"
                {
                    int page = 1;
                    int totalPages = 1;
                    while (totalPages >= page & playlist.Count < limit)
                    {
                        req = myLastFm.prepRequest("method=chart.gettoptracks&limit=500&page="+page.ToString());
                        trackResults = JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));

                        totalPages = trackResults.tracks.attr.totalPages;

                        if (paramArray.playedby == "world")
                        {
                            foreach (myLastFm.Track song in trackResults.tracks.track)
                            {
                                if (song.mbid != "")
                                {
                                    if (paramArray.comparison == "exactly" & song.playcount == paramArray.playcount & userLibrary.Contains(song, new myLastFm.TrackComparer()) == false)
                                        playlist.Add(song);
                                    else if (paramArray.comparison == "more than" & song.playcount > paramArray.playcount & userLibrary.Contains(song, new myLastFm.TrackComparer()) == false)
                                        playlist.Add(song);
                                    else if (paramArray.comparison == "less than" & song.playcount < paramArray.playcount & userLibrary.Contains(song, new myLastFm.TrackComparer()) == false)
                                        playlist.Add(song);

                                    if (playlist.Count >= limit)
                                        break;
                                }
                                
                            }
                        }
                        else if (paramArray.playedby == "me")
                        {
                            //scope "not in" shouln't have user playcount requirements
                        }
                        else //no playcount requirements
                        {
                            foreach (myLastFm.Track song in trackResults.tracks.track)
                            {
                                if (song.mbid != "")
                                {
                                    if (userLibrary.Contains(song, new myLastFm.TrackComparer()) == false)
                                        playlist.Add(song);

                                    if (playlist.Count >= limit)
                                        break;
                                }
                                
                            }
                        }

                        page++;
                    }
                }
            }
            //playcount requirements only
            else if (paramArray.playedby != null) 
            {
                if (paramArray.playedby == "me")
                {
                    if (userLibrary.Count <= limit)
                        playlist = userLibrary.ToList();
                    else
                    {
                        foreach (myLastFm.Track song in userLibrary)
                        {
                            playlist.Add(song);
                            if (playlist.Count == limit)
                                break;
                        }
                    }
                }
                else
                {
                    int page = 1;
                    int totalPages = 1;
                    while (page <= totalPages & playlist.Count < limit)
                    {
                        req = myLastFm.prepRequest("method=chart.gettoptracks&limit=500&page=" + page.ToString());
                        trackResults = JsonConvert.DeserializeObject<myLastFm.tracksRootObject>(myLastFm.lastFmJsonReq(req));
                        totalPages = trackResults.toptracks.attr.totalPages;

                        if (paramArray.comparison == "less than" & trackResults.toptracks.track.Last().playcount > paramArray.playcount)
                        {
                            ///do nothing, aka skip this page
                        }
                        if (paramArray.comparison == "more than" & trackResults.toptracks.track.First().playcount < paramArray.playcount)
                        {
                            //ya done
                            break;
                        }
                        else if (paramArray.comparison == "less than" & trackResults.toptracks.track.First().playcount < paramArray.playcount)
                        {
                            //add the entire page
                            playlist.AddRange(trackResults.toptracks.track);
                        }
                        else if (paramArray.comparison == "more than" & trackResults.toptracks.track.Last().playcount > paramArray.playcount)
                        {
                            //add the entire page
                            playlist.AddRange(trackResults.toptracks.track);
                        }
                        else
                        {
                            foreach (myLastFm.Track song in trackResults.toptracks.track)
                            {
                                if (paramArray.comparison == "less than" & song.playcount < paramArray.playcount)
                                    playlist.Add(song);
                                else if (paramArray.comparison == "more than" & song.playcount > paramArray.playcount)
                                    playlist.Add(song);
                                else if (paramArray.comparison == "exactly" & song.playcount == paramArray.playcount)
                                    playlist.Add(song);

                                if (playlist.Count >= limit)
                                    break;
                            }
                        }

                        page++;

                    }
                }

            }

            

            Switcher.Switch(new Playlist(playlist,outType));
        }
        
    }

}

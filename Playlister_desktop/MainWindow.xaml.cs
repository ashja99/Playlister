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

        public static void generatePlaylist(helpers.myParamArray paramArray, string outType, string playName, int limit)
        {
            myLastFm.AppUser currentUser;

            // If scope is relavent or plays by me is selected, might as well start by initializing the users library.
            if (paramArray.playedby == "me" | paramArray.scope != null)
            {
                if (paramArray.playedby == "me")
                {
                    
                }
                else
                {

                }

            }
            else
                currentUser = new myLastFm.AppUser();
            

            List<myLastFm.Track> playlist = new List<myLastFm.Track>(limit);

            //some tag requirements
            if (paramArray.tags != null)
            {
                List<myLastFm.Track> bList = new List<myLastFm.Track>(limit);

                if (paramArray.scope == false && paramArray.playedby != "me")
                {
                    //need whole user library for scope = false
                    currentUser = new myLastFm.AppUser();
                    if (paramArray.scope == false)
                    {
                        int ret = currentUser.addPage();
                        while (ret == 0)
                            ret = currentUser.addPage();
                    }

                    foreach (myLastFm.Tag tag in paramArray.tags)
                    {
                        tag.turnPage();
                        foreach (myLastFm.Track song in tag.tracklist)
                        {
                            if (currentUser.library.Contains(song, new myLastFm.TrackComparer()) == false)
                            {
                                if (paramArray.playedby == "me")
                                {
                                    //if scope is not in, playcount by me makes no sense
                                }
                                if (paramArray.playedby == "world")
                                {
                                    //TODO
                                }
                                else
                                {
                                    playlist.Add(song);
                                }
                            }
                        }

                    }
                }
                if (paramArray.scope == true | paramArray.playedby == "me")
                {
                    //for these, we don't necessarily need the whole library
                    currentUser = new myLastFm.AppUser(paramArray.compMode, paramArray.playcount);
                    currentUser.addPage();

                    foreach (myLastFm.Track song in currentUser.library)
                    {

                        song.getInfo();
                        if (song.toptags != null && song.toptags.tag != null)
                        {
                            if (paramArray.playedby == "me")
                            {
                                //current song is already pulled from the user library, which is already filtered by playcount
                                playlist.Add(song);
                            }
                            else if (paramArray.playedby == "world")
                            {
                                song.getInfo();

                                if (paramArray.compMode == compare.exact & song.playcount == paramArray.playcount)
                                    playlist.Add(song);
                                else if (paramArray.compMode == compare.greater & song.playcount > paramArray.playcount)
                                    playlist.Add(song);
                                else if (paramArray.compMode == compare.less & song.playcount < paramArray.playcount)
                                    playlist.Add(song);
                            }
                            else
                            { //played by is null
                                if (paramArray.tags.Except(song.toptags.tag, new myLastFm.TagComparer()).Any() == false)
                                {
                                    playlist.Add(song);
                                }
                                else if (bList.Count < limit && paramArray.tags.Count > 1)
                                {
                                    if (song.toptags.tag.Intersect(paramArray.tags, new myLastFm.TagComparer()).Any() == true)
                                    {
                                        bList.Add(song);
                                    }
                                }
                            }
                        }

                        if (playlist.Count >= limit)
                            break;

                    }

                    if (playlist.Count < limit)
                    {
                        playlist = (List<myLastFm.Track>)playlist.Union(bList, new myLastFm.TrackComparer());
                    }

                }

            }
            //no tag requirements, some scope requirement, ? playcount requirements
            else if (paramArray.scope != null)
            {
                if (paramArray.scope == true)
                {
                    //for these, we don't necessarily need the whole library
                    currentUser = new myLastFm.AppUser(paramArray.compMode, paramArray.playcount);
                    currentUser.addPage();

                    //no tag requirements, scope is "in"
                    if (paramArray.playedby == "world")
                    {
                        myLastFm.Chart Chart = new myLastFm.Chart(paramArray.compMode, paramArray.playcount);

                        while (playlist.Count < limit & Chart.turnPage() == 0)
                        {
                            Chart.library.Intersect(currentUser.library); //TODO: this check may or may not require the entire user library
                            playlist = (List<myLastFm.Track>)playlist.Union(Chart.library.ToList(), new myLastFm.TrackComparer());
                            Chart.turnPage();
                        }
                    }
                    else //user playcounts are already taken care while populating userLibrary
                    {
                        playlist = currentUser.library.ToList(); //do i need to loop this?

                    }

                }
                else //no tag requirements, scope is "not in"
                {

                    //need whole user library for scope = false
                    currentUser = new myLastFm.AppUser();
                    if (paramArray.scope == false)
                    {
                        int ret = currentUser.addPage();
                        while (ret == 0)
                            ret = currentUser.addPage();
                    }

                    if (paramArray.playedby == "world")
                    {
                        myLastFm.Chart Chart = new myLastFm.Chart(paramArray.compMode, paramArray.playcount);
                        Chart.addPage();
                        while (playlist.Count < limit & Chart.library != null)
                        {
                            Chart.library.ExceptWith(currentUser.library);
                            playlist = (List<myLastFm.Track>)playlist.Union(Chart.library.ToList(), new myLastFm.TrackComparer());
                            Chart.turnPage();
                        }

                    }
                    else if (paramArray.playedby == "me")
                    {
                        //scope "not in" shouln't have user playcount requirements
                    }
                    else //no playcount requirements
                    {
                        myLastFm.Chart Chart = new myLastFm.Chart();
                        Chart.addPage();
                        while (playlist.Count < limit & Chart.library != null)
                        {
                            Chart.library.ExceptWith(currentUser.library);
                            playlist = (List<myLastFm.Track>)playlist.Union(Chart.library.ToList(), new myLastFm.TrackComparer());
                            Chart.turnPage();
                        }

                    }


                }
            }
            //playcount requirements only
            else if (paramArray.playedby != null)
            {
                if (paramArray.playedby == "me")
                {
                    currentUser = new myLastFm.AppUser(paramArray.compMode, paramArray.playcount);

                    while (currentUser.addPage() == 0 & currentUser.library.Count < limit)
                    {
                        //do nothing, while loop is already doing it
                    }

                    playlist = currentUser.library.ToList();
                }
                else
                {
                    myLastFm.Chart chart = new myLastFm.Chart(paramArray.compMode, paramArray.playcount);

                    while (chart.addPage() == 0 & chart.library.Count < limit)
                    {
                        //do nothing, while loop is already doing it
                    }

                    playlist = chart.library.ToList();
                }

            }

            if (playlist.Count > limit)
                playlist = (List<myLastFm.Track>)playlist.Take(limit);

            Switcher.Switch(new Playlist(playlist, outType));
        }
        
    }

}

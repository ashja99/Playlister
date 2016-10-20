using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Playlister
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl, ISwitchable
    {
        public Playlist(List<myLastFm.Track> playlist,string outType)
        {
            this.InitializeComponent();
            foreach (myLastFm.Track song in playlist)
            {
                Console.WriteLine(song.name);
                
                playlistView.Items.Add(song.name+" - "+song.artist.name);
            }

            if (outType == "Last.fm")
            {
                //Last.fm got rid of playlists, soooo........
                //MainWindow.getAuthSession();
            }
            else if (outType == "iTunes")
            {
                //TODO
            }
            else if (outType == "Spotify")
            {
                //TODO
            }
        }
        #region Iswitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new getParams());
        }

    }
}

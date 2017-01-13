using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for getParams.xaml
    /// </summary>
    public partial class getParams : UserControl, ISwitchable
    {
        public getParams()
        {
            this.InitializeComponent();
        }
        #region Iswitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            //displayMessage.Content = ("Loading...");

            if (outType.Text != "Last.fm")
            {
                MessageBox.Show("Oh Noes! that output format hasn't been implemented yet!");
                return;
            }
            if (!helpers.isNumeric(songCount.Text))
            {
                MessageBox.Show( "Oh Noes! the track limit is not an integer!");
                return;
            }
            else if (Int32.Parse(songCount.Text) > 100)
            {
                MessageBox.Show("Please choose a track limit less than 100!");
                return;
            }
            if (!helpers.isNumeric(noScrobbles.Text))
            {
                MessageBox.Show( "Oh Noes! noScrobbles is not an integer!");
                return;
            }
            if (playName.Text=="" | playName.Text == "(ex. my Top 50)")
            {
                MessageBox.Show("Please enter a playlist name!");
                return;
            }

            char[] separator = new char[] { ',' };
            Console.WriteLine("Old Tag List: " + hasTags.Text);
            hasTags.Text = hasTags.Text.Replace(", ", ",");
            Console.WriteLine("New Tag List: " + hasTags.Text);
            string[] tags = hasTags.Text.Split(separator);


            if (whoScrobbled.Text == "me")
            {
                Console.WriteLine(Properties.Settings.Default.keyUser);
            }

            // format params nicely and send to get qualifying song
            helpers.myParamArray paramArray = new helpers.myParamArray(thisInLibrary.Text, useLibrary.Text, comparable.Text, noScrobbles.Text, whoScrobbled.Text, tags, scopeCk.IsChecked, scrobblesCk.IsChecked, tagsCk.IsChecked);

            if (paramArray.playedby == "me" & paramArray.scope == false)
            {
                MessageBox.Show("Scope and scrobble requirements contradict each other!");
                return;
            }

            Switcher.Switch(new Loading());

            MainWindow.generatePlaylist(paramArray, outType.Text, playName.Text, Int32.Parse(songCount.Text));

        }
    }
}

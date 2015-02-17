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

namespace Playlister_desktop
{
    /// <summary>
    /// Interaction logic for getParams.xaml
    /// </summary>
    public partial class getParams : UserControl
    {
        public getParams()
        {
            InitializeComponent();
        }

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            displayMessage.Content = ("Loading...");

            if (outType.Text != "Last.fm")
            {
                displayMessage.Content = "Oh Noes! that output format hasn't been implemented yet!";
                return;
            }
            if (!customFunctions.isNumeric(songCount.Text))
            {
                displayMessage.Content = "Oh Noes! songCount is not an integer!";
                return;
            }
            if (!customFunctions.isNumeric(noScrobbles.Text))
            {
                displayMessage.Content = "Oh Noes! noScrobbles is not an integer!";
                return;
            }

            Switcher.Switch(new Loading());

            char[] separator = new char[] { ',' };
            Console.WriteLine("Old Tag List: " + hasTags.Text);
            hasTags.Text = hasTags.Text.Replace(", ", ",");
            Console.WriteLine("New Tag List: " + hasTags.Text);
            string[] tags = hasTags.Text.Split(separator);

            if (whoScrobbled.Text == "me")
            {
                Console.WriteLine(MainWindow.globalusr);
            }

            // format params nicely and send to get qualifying song
            customFunctions.myParamArray paramArray = new customFunctions.myParamArray(thisInLibrary.Text, useLibrary.Text, comparable.Text, noScrobbles.Text, whoScrobbled.Text, tags);

            if (paramArray.scope == "my")
            {
                paramArray.scope = "false";
            }
            else
            {
                paramArray.scope = "true";
            }

            MainWindow.getResults(paramArray);

            if (outType.Text == "Last.fm")
            {
                MainWindow.getAuthSession();
            }
            else if (outType.Text == "iTunes")
            {
                //do things here for iTunes
            }
            else if (outType.Text == "Spotify")
            {
                //do things here for spotify
            }

        }
    }
}

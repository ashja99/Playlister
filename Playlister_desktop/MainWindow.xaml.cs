using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml;

namespace Playlister_desktop
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
        public static string globalusr;
        public static string apiAddr = "http://ws.audioscrobbler.com/2.0/?";
        private static string globalkey = "d4153ce5c6605af07bf23651b14bfcc3";

        public static string getSig(string core)
        {
            MD5 getMd5 = MD5.Create();
            return customFunctions.GetHash(getMd5, core + "e40049bcff4ef495115924cb5a6fce76");
        }

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
                throw new ArgumentException("NextPAge is not ISwitchable! "
                    + nextpage.Name.ToString());
        }

        public static void getAuthSession()
        {
            getAuthSession(false);
        }

        public static void getAuthSession(bool force)
        {
            if (Properties.Settings.Default.sessionKey != "" && !force)
            {
                Console.WriteLine("Stored Key for User " + globalusr + ": " + Properties.Settings.Default.sessionKey);
                return;
            }

            string mytok = "";
            string sessionKey = "";

            string request = apiAddr + "method=auth.getToken&api_key=" + globalkey + "&api_sig=" + getSig("api_key" + globalkey + "methodauth.getToken");

            XmlDocument xDoc = myLastFm.lastFmReq(request);

            XmlNodeList xnList = xDoc.SelectNodes("/lfm");

            foreach (XmlNode xn in xnList)
            {
                mytok = xn["token"].InnerText;
                Console.WriteLine("Token:" + mytok);
            }

            // need to wait after this somehow...
            Process.Start("http://www.last.fm/api/auth?api_key=" + globalkey + "&token=" + mytok);
            MessageBoxResult waiter = MessageBox.Show("Your default web brower is now launching. Please click below when you have allowed this program to modify your account (add a playlist).");
            
            // Do it all again for sessionKey

            request = apiAddr + "method=auth.getSession&api_key=" + globalkey + "&api_sig=" + getSig("api_key" + globalkey + "methodauth.getSessiontoken" + mytok) + "&token=" + mytok;

            xDoc = myLastFm.lastFmReq(request);

            xnList = xDoc.SelectNodes("/lfm/session");

            foreach (XmlNode xn in xnList)
            {
                sessionKey = xn["key"].InnerText;
                Console.WriteLine("Key:" + sessionKey);
            }

            Properties.Settings.Default.sessionKey = sessionKey;
            Properties.Settings.Default.Save(); //save immediately, don't wait for close

        }

        public static void getResults(customFunctions.myParamArray paramArray)
        {
            // get list of libraries (scope)
            // get results for each param
            // apply count limit return list of ids
        }
        
    }

}

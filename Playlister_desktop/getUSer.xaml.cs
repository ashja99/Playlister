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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class getUser : UserControl, ISwitchable
    {
        public getUser()
        {
            this.InitializeComponent();
        }

        #region Iswitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (uname.Text.Trim() == "")
            {
                giveMeUser.Content = "We really need that username...";
                return;
            }
            MainWindow.globalusr = uname.Text.Trim();
            if (MainWindow.globalusr != Properties.Settings.Default.keyUser)
            {
                Properties.Settings.Default.keyUser = MainWindow.globalusr;
                Properties.Settings.Default.sessionKey = "";
                Properties.Settings.Default.Save();
            }
            Switcher.Switch(new getParams());
            Console.WriteLine("Session User: " + MainWindow.globalusr);
        }

    }
}

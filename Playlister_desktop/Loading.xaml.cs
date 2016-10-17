using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Loading : UserControl, ISwitchable
    {
        public Loading()
        {
            this.InitializeComponent();
        }  

        #region Iswitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}

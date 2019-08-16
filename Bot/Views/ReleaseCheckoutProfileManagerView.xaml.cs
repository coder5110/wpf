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
using Bot.ViewModels;

namespace Bot.Views
{
    /// <summary>
    /// Interaction logic for ReleaseCheckoutProfileManagerView.xaml
    /// </summary>
    public partial class ReleaseCheckoutProfileManagerView : UserControl
    {
        public ReleaseCheckoutProfileManagerView()
        {
            InitializeComponent();
        }

        private void ReleaseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReleaseCheckoutProfileManagerViewModel manager = DataContext as ReleaseCheckoutProfileManagerViewModel;

            foreach (ReleaseCheckoutProfileRecordViewModel profile in e.AddedItems)
            {
                manager.SelectedProfiles.Add(profile);
            }

            foreach (ReleaseCheckoutProfileRecordViewModel profile in e.RemovedItems)
            {
                manager.SelectedProfiles.Remove(profile);
            }
        }
    }
}

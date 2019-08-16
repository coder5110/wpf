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
    /// Логика взаимодействия для CheckoutProfileManagerView.xaml
    /// </summary>
    public partial class CheckoutProfileManagerView : UserControl
    {
        public CheckoutProfileManagerView()
        {
            InitializeComponent();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckoutProfileManagerViewModel manager = DataContext as CheckoutProfileManagerViewModel;

            foreach (CheckoutProfileRecordViewModel profile in e.AddedItems)
            {
                manager.SelectedProfiles.Add(profile);
            }

            foreach (CheckoutProfileRecordViewModel profile in e.RemovedItems)
            {
                manager.SelectedProfiles.Remove(profile);
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CheckoutProfileManagerViewModel manager = DataContext as CheckoutProfileManagerViewModel;

            if (manager.Edit.CanExecute(null))
            {
                manager.Edit.Execute(null);
            }
        }
    }
}

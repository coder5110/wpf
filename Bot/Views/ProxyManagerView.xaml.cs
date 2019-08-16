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
    /// Interaction logic for ProxyManagerView.xaml
    /// </summary>
    public partial class ProxyManagerView : UserControl
    {
        public ProxyManagerView()
        {
            InitializeComponent();
        }

        private void proxiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProxyManagerViewModel manager = DataContext as ProxyManagerViewModel;
            
            foreach (ProxyTestableViewModel proxy in e.AddedItems)
            {
                manager.SelectedProxies.Add(proxy);
            }

            foreach (ProxyTestableViewModel proxy in e.RemovedItems)
            {
                manager.SelectedProxies.Remove(proxy);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, button);
            Keyboard.Focus(button);
        }
    }
}

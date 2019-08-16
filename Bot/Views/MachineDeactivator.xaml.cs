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
using System.Windows.Shapes;
using Bot.ViewModels;

namespace Bot.Views
{
    /// <summary>
    /// Логика взаимодействия для MachineDeactivator.xaml
    /// </summary>
    public partial class MachineDeactivator : Window
    {
        public MachineDeactivator()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LicenseManagerViewModel lm = DataContext as LicenseManagerViewModel;

            if (lm.DeactivateLicense.CancelCommand.CanExecute(null) && !(lm.IsDeactivated ?? false))
            {
                e.Cancel = true;
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

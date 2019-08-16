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
    /// Логика взаимодействия для ReleaseTaskView.xaml
    /// </summary>
    public partial class ReleaseTaskView : UserControl
    {
        public ReleaseTaskView()
        {
            InitializeComponent();
        }

        private void ProfilesListView_GotFocus(object sender, RoutedEventArgs e)
        {
            ReleaseTaskViewModel vm = DataContext as ReleaseTaskViewModel;

            vm.SwitchLogToProfiles.Execute(null);
        }

        private void TasksListView_GotFocus(object sender, RoutedEventArgs e)
        {
            ReleaseTaskViewModel vm = DataContext as ReleaseTaskViewModel;

            vm.SwitchLogToTasks.Execute(null);
        }
    }
}

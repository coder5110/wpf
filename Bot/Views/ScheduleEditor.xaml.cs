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
    /// Логика взаимодействия для ScheduleEditor.xaml
    /// </summary>
    public partial class ScheduleEditor : Window
    {
        public ScheduleEditor()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ReleaseScheduleEditViewModel vm = DataContext as ReleaseScheduleEditViewModel;

            vm.Disable.Execute(null);

            DialogResult = true;
        }
    }
}

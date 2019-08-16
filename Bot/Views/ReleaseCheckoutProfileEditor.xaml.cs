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
    /// Логика взаимодействия для ReleaseCheckoutProfileEditor.xaml
    /// </summary>
    public partial class ReleaseCheckoutProfileEditor : Window
    {
        public ReleaseCheckoutProfileEditor()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //ReleaseCheckoutProfileEditViewModel vm = DataContext as ReleaseCheckoutProfileEditViewModel;

            //if (!vm.IsTasksCountValid)
            //{
            //    MessageBox.Show($"You have entered count of tasks more that allowed by your license ({vm.MaxTasksCount} tasks left).");
            //}
            //else
            {
                DialogResult = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

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

namespace Bot.Views
{
    /// <summary>
    /// Interaction logic for CheckoutProfileEditor.xaml
    /// </summary>
    public partial class CheckoutProfileEditor : Window
    {
        public CheckoutProfileEditor()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

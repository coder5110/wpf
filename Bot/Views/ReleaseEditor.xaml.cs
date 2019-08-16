using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Bot.Models;
using Bot.ViewModels;

namespace Bot.Views
{
    /// <summary>
    /// Логика взаимодействия для ReleaseEditor.xaml
    /// </summary>
    public partial class ReleaseEditor : Window
    {
        public ReleaseEditor()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TabControl tc = sender as TabControl;
            //TabItem ti = e.AddedItems[0] as TabItem;
            //FrameworkElement sc = ti?.Content as FrameworkElement;

            //if (sc != null)
            //{
            //    FocusManager.SetFocusedElement(this, sc);
            //    //sc.Focusable = true;
            //    //sc.Focus();
            //}
        }
    }
}

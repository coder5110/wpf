using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
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
using Bot.Models;
using Bot.ViewModels;
using Bot.Views;
using System.Management;
using System.Net.NetworkInformation;
using Bot.Services;
using HtmlAgilityPack;

namespace Bot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();;
        }

        #region Methods

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppViewModel manager = DataContext as AppViewModel;

            foreach (ReleaseScheduleViewModel release in e.AddedItems)
            {
                manager.SelectedReleases.Add(release);
            }

            foreach (ReleaseScheduleViewModel release in e.RemovedItems)
            {
                manager.SelectedReleases.Remove(release);
            }
        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppViewModel appViewModel = DataContext as AppViewModel;
            
            if (appViewModel != null && !appViewModel.CloseApp())
            {
                e.Cancel = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Fields

        #endregion
    }
}

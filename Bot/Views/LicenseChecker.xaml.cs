﻿using System;
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
    /// Логика взаимодействия для LicenseChecker.xaml
    /// </summary>
    public partial class LicenseChecker : Window
    {
        public LicenseChecker()
        {
            InitializeComponent();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LicenseManagerViewModel lm = DataContext as LicenseManagerViewModel;

            lm.CheckLicense.ExecuteAsync(null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LicenseManagerViewModel lm = DataContext as LicenseManagerViewModel;

            if (lm.CheckLicense.CancelCommand.CanExecute(null))
            {
                lm.CheckLicense.CancelCommand.Execute(null);
            }
        }
    }
}

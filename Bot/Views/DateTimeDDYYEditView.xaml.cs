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

namespace Bot.Views
{
    /// <summary>
    /// Interaction logic for DateTimeDDYYEditor.xaml
    /// </summary>
    public partial class DateTimeDDYYEditView : UserControl
    {
        public DateTimeDDYYEditView()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(DateTime?), typeof(DateTimeDDYYEditView), new PropertyMetadata(default(DateTime?)));

        public DateTime? Value
        {
            get
            {
                return (DateTime?) GetValue(ValueProperty);
            }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = DataContext as DateTime?;

            int year = date?.Year ?? DateTime.Now.Year;

            DataContext = new DateTime?(new DateTime(year, (int) e.AddedItems[0], 1));
        }

        private void comboBox_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = DataContext as DateTime?;

            int month = date?.Month ?? DateTime.Now.Month;

            DataContext = new DateTime?(new DateTime((int)e.AddedItems[0], month, 1));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bot.Models;

namespace Bot.ViewModels
{
    public class SuccessMonitorViewModel: ViewModelBase
    {
        #region Properties

        public ObservableCollection<CheckoutInfo> Model
        {
            get { return m_model; }

            set { SetProperty(ref m_model, value); }
        }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Fields

        private ObservableCollection<CheckoutInfo> m_model = null;

        #endregion
    }
}

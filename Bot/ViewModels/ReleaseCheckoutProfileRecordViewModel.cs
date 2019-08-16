using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    public class ReleaseCheckoutProfileRecordViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public ReleaseCheckoutProfile Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    DeattachModel(m_model);
                }

                if (SetProperty(ref m_model, value))
                {
                    OnPropertyChanged("ReleaseCheckoutProfile");
                    OnPropertyChanged("Size");
                    OnPropertyChanged("TasksCount");
                    OnPropertyChanged("BuyLimit");
                    OnPropertyChanged("NotificationEmail");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public CheckoutProfile CheckoutProfile => m_model.CheckoutProfile;
        public string Size => m_model.Size;
        public int TasksCount => m_model.TasksCount;
        public string NotificationEmail => m_model.NotificationEmail;

        #endregion

        #region Commands



        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Fields

        private ReleaseCheckoutProfile m_model = null;

        #endregion
    }
}

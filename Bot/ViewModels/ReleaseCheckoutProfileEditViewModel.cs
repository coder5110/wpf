using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    public class ReleaseCheckoutProfileEditViewModel: ViewModelBase
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

                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public CheckoutProfile CheckoutProfile
        {
            get { return m_model.CheckoutProfile; }

            set { m_model.CheckoutProfile = value; }
        }

        public string Size
        {
            get { return m_model.Size; }

            set { m_model.Size = value; }
        }

        public int TasksCount
        {
            get { return m_model.TasksCount; }

            set { m_model.TasksCount = value; }
        }

        public string NotificationEmail
        {
            get { return m_model.NotificationEmail; }

            set { m_model.NotificationEmail = value; }
        }

        public ObservableCollection<CheckoutProfile> CheckoutProfiles
        {
            get { return m_checkoutProfiles; }

            set { SetProperty(ref m_checkoutProfiles, value); }
        }

        public ClothesSizeSystem ClothesSizeSystem
        {
            get { return m_model.ClothesSizeSystem; }

            set { m_model.ClothesSizeSystem = value; }
        }

        public string VariantId
        {
            get { return m_model.VariantId; }

            set { m_model.VariantId = value; }
        }

        public int MaxTasksCount { get; set; } = 0;

        public bool IsTasksCountValid => TasksCount <= MaxTasksCount;

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Size")
            {
                OnPropertyChanged("Size");
            }
            else if (args.PropertyName == "ClothesSizeSystem")
            {
                OnPropertyChanged("ClothesSizeSystem");
            }
            else if (args.PropertyName == "VariantId")
            {
                OnPropertyChanged("VariantId");
            }
        }

        #endregion

        #region Fields

        private ReleaseCheckoutProfile m_model = null;
        private ObservableCollection<CheckoutProfile> m_checkoutProfiles = null;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bot.Models;

namespace Bot.ViewModels
{
    public class ReleaseTaskViewModel: ViewModelBase
    {
        #region Constructors

        public ReleaseTaskViewModel()
        {
            SwitchLogToProfiles = new DelegateCommand(parameter =>
            {
                SelectedLog = m_model.Log;
            });

            SwitchLogToTasks = new DelegateCommand(parameter =>
            {
                SelectedLog = SelectedCheckoutTask?.Log;
            });
        }

        #endregion

        #region Properties

        public ReleaseTaskBase Model
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
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Footsite");
                    OnPropertyChanged("ProductLink");
                    OnPropertyChanged("Keywords");
                    OnPropertyChanged("IsProductAvailable");

                    Update();
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string Name => m_model.Release.Name;
        public Footsite Footsite => m_model.Release.Footsite;
        public string ProductLink => m_model.Release.ProductLink;
        public string Keywords => string.Join("+", m_model.Release.KeyWords);
        public ObservableCollection<CheckoutTaskCcProfile> Profiles => m_model.Profiles;

        public ObservableCollection<string> SelectedLog
        {
            get { return m_selectedLog; }

            set { SetProperty(ref m_selectedLog, value); }
        }

        public CheckoutTaskCcProfile SelectedProfile
        {
            get { return m_selectedProfile; }

            set
            {
                if (SetProperty(ref m_selectedProfile, value))
                {
                    OnPropertyChanged("CheckoutTasks");
                }

                SelectedLog = m_model.Log;
            }
        }

        public ObservableCollection<CheckoutTask> CheckoutTasks => m_selectedProfile?.CheckoutTasks;

        public CheckoutTask SelectedCheckoutTask
        {
            get { return m_selectedCheckoutTask; }

            set
            {
                SetProperty(ref m_selectedCheckoutTask, value);

                SelectedLog = m_selectedCheckoutTask?.Log ?? m_model.Log;
            }
        }

        #endregion

        #region Commands

        public ICommand SwitchLogToProfiles { protected set; get; }
        public ICommand SwitchLogToTasks { protected set; get; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsProductAvailable")
            {
                OnPropertyChanged("IsProductAvailable");
            }
        }

        private void Update()
        {
            SelectedLog = m_model.Log;
        }

        #endregion

        #region Fields

        private ReleaseTaskBase m_model = null;
        private ObservableCollection<string> m_selectedLog = null;
        private CheckoutTaskCcProfile m_selectedProfile = null;
        private CheckoutTask m_selectedCheckoutTask = null;

        #endregion
    }
}

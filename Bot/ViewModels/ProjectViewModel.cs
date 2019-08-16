using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    class ProjectViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public Project Model
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
                    Update();
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string Name
        {
            get { return m_model.Name; }

            set { m_model.Name = value; }
        }

        public ObservableCollection<ProxyTestableViewModel> Proxies { get; } = new ObservableCollection<ProxyTestableViewModel>();
        public ObservableCollection<CheckoutProfileRecordViewModel> CheckoutProfiles { get; } = new ObservableCollection<CheckoutProfileRecordViewModel>();

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            CheckoutProfiles.Clear();

            foreach (CheckoutProfile profile in m_model.CheckoutProfiles)
            {
                CheckoutProfiles.Add(new CheckoutProfileRecordViewModel()
                {
                    Model = profile
                });
            }
        }

        #endregion

        #region Fields

        private Project m_model = null;

        #endregion

        
    }
}

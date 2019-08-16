using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    class ProxyEditViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public Proxy Model
        {
            get
            {
                return m_model;
            }

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

        public string IP
        {
            get { return m_model.IP; }

            set { m_model.IP = value; }
        }

        public int? Port
        {
            get { return m_model.Port; }

            set { m_model.Port = value; }
        }

        public string Username
        {
            get { return m_model.Username; }

            set { m_model.Username = value; }
        }

        public string Password
        {
            get { return m_model.Password; }

            set { m_model.Password = value; }
        }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            //TODO implementation
        }

        private void Update()
        {
            if (m_model.Port == null)
            {
                m_model.Port = 80;
            }
        }

        #endregion

        #region Fields

        private Proxy m_model = null;

        #endregion
    }
}

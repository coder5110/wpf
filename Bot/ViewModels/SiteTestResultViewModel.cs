using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Services;

namespace Bot.ViewModels
{
    public class SiteTestResultViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public SiteProxyTestResult Model
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
                    OnPropertyChanged("IsPassed");
                    OnPropertyChanged("Info");
                    OnPropertyChanged("Status");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public bool IsPassed => m_model.Status == SiteProxyTestStatus.Passed;

        public string Info => IsPassed ? $"PASSED({m_model.Ping} ms)" : m_model.Status == SiteProxyTestStatus.Testing ? "Testing..." : $"FAILED({m_model.Message})";

        public SiteProxyTestStatus Status => m_model.Status;

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

        #region Fields

        private SiteProxyTestResult m_model = null;

        #endregion
    }
}

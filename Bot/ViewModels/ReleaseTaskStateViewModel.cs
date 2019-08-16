using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bot.Models;

namespace Bot.ViewModels
{
    public class ReleaseTaskStateViewModel: ViewModelBase
    {
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
                    OnPropertyChanged("State");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public ReleaseTaskState State => m_model?.State ?? ReleaseTaskState.Idle;

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "State")
            {
                OnPropertyChanged("State");
            }
        }

        #endregion

        #region Fields

        private ReleaseTaskBase m_model = null;

        #endregion
    }
}

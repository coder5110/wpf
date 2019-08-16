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
    class ReleaseScheduleEditViewModel: ViewModelBase
    {
        #region Constructors

        public ReleaseScheduleEditViewModel()
        {
            Disable = new DelegateCommand(parameter =>
            {
                StartTime.Model = null;
                EndTime.Model = null;
            });

            StartTime.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Model")
                {
                    m_model.StartTime = StartTime.Model;

                    if (StartTime.Model > EndTime.Model && !IsManualStop)
                    {
                        EndTime.Model = StartTime.Model;
                        m_model.EndTime = EndTime.Model;
                    }
                }
            };

            EndTime.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Model")
                {
                    m_model.EndTime = EndTime.Model;

                    if (EndTime.Model < StartTime.Model)
                    {
                        StartTime.Model = EndTime.Model;
                        m_model.StartTime = StartTime.Model;
                    }
                }
            };
        }

        #endregion

        #region Properties

        public ISchedulable Model
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

        public DateTimeEditViewModel StartTime { get; } = new DateTimeEditViewModel();
        public DateTimeEditViewModel EndTime { get; } = new DateTimeEditViewModel();

        public bool IsManualStop
        {
            get { return m_isManualStop; }

            set
            {
                if (SetProperty(ref m_isManualStop, value))
                {
                    EndTime.Model = m_isManualStop ? null : StartTime.Model;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand Disable { get; set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            //TODO Implement
        }

        private void Update()
        {
            IsManualStop = m_model.EndTime == null;
            StartTime.Model = m_model.StartTime ?? DateTime.Now;
            EndTime.Model = m_model.EndTime;
        }

        #endregion

        #region Fields

        private ISchedulable m_model = null;
        private bool m_isManualStop = true;

        #endregion
    }
}

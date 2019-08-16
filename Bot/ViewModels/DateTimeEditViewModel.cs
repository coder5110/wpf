using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    class DateTimeEditViewModel: ViewModelBase
    {
        #region Properties

        public DateTime? Model
        {
            get { return m_model; }

            set
            {
                if (SetProperty(ref m_model, value))
                {
                    m_timePeriod = m_model?.ToString("tt", CultureInfo.InvariantCulture);
                    OnPropertyChanged("Value");
                    OnPropertyChanged("Hour");
                    OnPropertyChanged("Minute");
                    OnPropertyChanged("TimePeriod");
                }
            }
        }

        public DateTime? Value
        {
            get { return m_model; }

            set
            {
                if (value != null)
                {
                    Model = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, m_model.Value.Hour,
                        m_model.Value.Minute, 0);
                }
                else
                {
                    Model = null;
                }
            }
        }

        public int? Hour
        {
            get
            {
                int? hour = m_model?.Hour;

                if (hour != null)
                {
                    if (TimePeriod == DateTimeValues.PM)
                    {
                        hour -= DateTimeValues.PeriodDuration;
                    }

                    if (hour == 0)
                    {
                        hour = 12;
                    }
                }

                return hour;
            }

            set
            {
                int? hour = value != null ? value != 12 ? value : 0 : null;

                if (hour != null)
                {
                    if (TimePeriod == DateTimeValues.PM)
                    {
                        hour += DateTimeValues.PeriodDuration;
                    }

                    Model = new DateTime(m_model.Value.Year, m_model.Value.Month, m_model.Value.Day, (int)hour,
                        m_model.Value.Minute, 0);
                }
                else
                {
                    Model = null;
                }
            }
        }

        public int? Minute
        {
            get
            {
                return m_model?.Minute;
            }

            set
            {
                if (value != null)
                {
                    Model = new DateTime(m_model.Value.Year, m_model.Value.Month, m_model.Value.Day, m_model.Value.Hour,
                        (int)value, 0);
                }
                else
                {
                    Model = null;
                }
            }
        }

        public string TimePeriod
        {
            get
            {
                return m_timePeriod;
            }

            set
            {
                int? hour = Hour;

                if (SetProperty(ref m_timePeriod, value))
                {
                    if (m_timePeriod != null)
                    {
                        Hour = hour;
                    }
                    else
                    {
                        Model = null;
                    }
                }
            }
        }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Fields

        private DateTime? m_model = null;
        private string m_timePeriod = null;

        #endregion

    }
}

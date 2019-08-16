using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    public class ReleaseRecordViewModel: ViewModelBase
    {
        #region Properties

        public Release Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    DeattachModel(m_model);
                    m_model.Proxies.CollectionChanged -= ProxiesCollectionChangedEventHandler;
                    m_model.Proxies.CollectionChanged -= ProfilesCollectionChangedEventHandler;
                }

                if (SetProperty(ref m_model, value))
                {
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Footsite");
                    OnPropertyChanged("TasksCount");
                    OnPropertyChanged("ProxiesCount");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                    m_model.Proxies.CollectionChanged += ProxiesCollectionChangedEventHandler;
                    m_model.Proxies.CollectionChanged += ProfilesCollectionChangedEventHandler;
                }
            }
        }

        public string Name => m_model.Name;
        public Footsite Footsite => m_model.Footsite;
        public int TasksCount => m_model.TasksCount;
        public int ProxiesCount => m_model.Proxies.Count;
        public string StartTime => m_model.StartTime?.ToString("MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture);
        public string EndTime => m_model.EndTime?.ToString("MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture);
        public string ScheduleTime => $"{StartTime} - {EndTime}";

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name")
            {
                OnPropertyChanged("Name");
            }
            else if (args.PropertyName == "Footsite")
            {
                OnPropertyChanged("Footsite");
            }
            else if (args.PropertyName == "StartTime")
            {
                OnPropertyChanged("StartTime");
                OnPropertyChanged("ScheduleTime");
            }
            else if (args.PropertyName == "EndTime")
            {
                OnPropertyChanged("EndTime");
                OnPropertyChanged("ScheduleTime");
            }
        }

        protected void ProxiesCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnPropertyChanged("ProxiesCount");
        }

        protected void ProfilesCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnPropertyChanged("TasksCount");
        }

        #endregion

        #region Fields

        private Release m_model = null;

        #endregion
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Bot.ViewModels;

namespace Bot.Models
{
    public class CaptchaHarvesterBase : BindableObject, ICaptchaHarvester
    {
        #region Constructors

        public CaptchaHarvesterBase()
        {
            Enable = new DelegateCommand(parameter =>
            {
                IsEnabled = true;
            },
            parameter => !IsEnabled);

            Disable = new DelegateCommand(parameter =>
            {
                IsEnabled = false;
            },
            parameter => IsEnabled);
        }

        #endregion

        #region Properties

        public int TasksInQueue
        {
            get
            {
                lock (m_lock)
                {
                    return m_tasks.Count;
                }
            }
        }

        public ObservableCollection<ICaptchaSolutionSource> SolutionSources
        {
            get
            {
                lock (m_lock)
                {
                    return m_solutionSources;
                }
            }

            set { SetProperty(ref m_solutionSources, value, m_lock); }
        }

        public bool IsEnabled {
            get
            {
                lock (m_lock)
                {
                    return m_isEnabled;
                }
            }

            set
            {
                lock (m_startLock)
                {
                    if (SetProperty(ref m_isEnabled, value, m_lock))
                    {
                        if (IsEnabled)
                        {
                            m_cts = new CancellationTokenSource();

                            foreach (ICaptchaSolutionSource solutionSource in SolutionSources)
                            {
                                solutionSource.CancelToken = m_cts.Token;
                            }
                        }
                        else
                        {
                            m_cts.Cancel();
                        }

                        Update();
                    }
                }
            }
        }

        #endregion

        #region Commands

        public ICommand Enable { get; protected set; }
        public ICommand Disable { get; protected set; }

        #endregion

        #region Methods

        public void GetSolution(ICaptchaHarvesterTask task)
        {
            if (!ProcessTask(task))
            {
                lock (m_lock)
                {
                    m_tasks.Add(task);
                }

                OnPropertyChanged("TasksInQueue");
            }
            else
            {
                task.SolutionReadyEvent.Set();
            }
        }

        protected virtual bool ProcessTask(ICaptchaHarvesterTask task)
        {
            throw new NotImplementedException();
        }

        protected void ReleaseTask(ICaptchaHarvesterTask task)
        {
            lock (m_lock)
            {
                m_tasks.Remove(task);
            }

            task.SolutionReadyEvent.Set();

            OnPropertyChanged("TasksInQueue");
        }

        protected ICaptchaSolutionSource GetSolutionSource()
        {
            ICaptchaSolutionSource source = null;

            bool isFound = false;
            int index = -1;

            lock (m_lock)
            {
                if (SolutionSources.Count > 0)
                {
                    int startIndex = m_currentSolutionSourceIndex;

                    while (true)
                    {
                        if (SolutionSources[m_currentSolutionSourceIndex].IsEnabled)
                        {
                            isFound = true;
                            index = m_currentSolutionSourceIndex;
                        }

                        if (m_currentSolutionSourceIndex++ >= SolutionSources.Count)
                        {
                            m_currentSolutionSourceIndex = 0;
                        }

                        if (isFound)
                        {
                            break;
                        }

                        if (startIndex == m_currentSolutionSourceIndex)
                        {
                            break;
                        }
                    }
                }

                if (isFound)
                {
                    source = SolutionSources[index];
                }
            }

            return source;
        }

        protected virtual void Update()
        {

        }

        #endregion

        #region Fields
        
        protected readonly List<ICaptchaHarvesterTask> m_tasks = new List<ICaptchaHarvesterTask>();
        private int m_currentSolutionSourceIndex = 0;
        protected bool m_isEnabled = false;
        private CancellationTokenSource m_cts = null;
        private ObservableCollection<ICaptchaSolutionSource> m_solutionSources = new ObservableCollection<ICaptchaSolutionSource>();
        protected readonly object m_lock = new object();
        private readonly object m_startLock = new object();

        #endregion
    }

    public class CaptchaHarvesterTaskBase : ICaptchaHarvesterTask
    {
        public CaptchaHarvesterTaskBase(object parameter)
        {
            SolutionReadyEvent = new ManualResetEvent(false);
        }

        public ManualResetEvent SolutionReadyEvent { get; }
        public object Parameter { get; }
        public object Solution { get; set; }
    }
}

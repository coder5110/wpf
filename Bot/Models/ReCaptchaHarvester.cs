using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Input;
using Bot.Helpers;
using Bot.Services;
using Bot.ViewModels;
using Bot.Views;
using CefSharp;
using CefSharp.Wpf;

namespace Bot.Models
{
    public class ReCaptchaHarvester: CaptchaHarvesterBase, ISchedulable
    {
        #region Constructors

        public ReCaptchaHarvester(object defaultSettings)
        {
            RecaptchaSettings settings = defaultSettings as RecaptchaSettings;

            if (settings == null)
            {
                throw new ArgumentException("Default settings has wrong type or null");
            }

            SiteKey = settings.SiteKey;
            SitePath = settings.SitePath;

            SolutionSources = new ObservableCollection<ICaptchaSolutionSource>();
            SolutionSources.Add(new TwoCaptchaSource());
            SolutionSources.Add(new AntiCaptchaSource());

            m_liveTimer = new Timer((o) => RemoveOldSolution(), null, Timeout.Infinite, Timeout.Infinite);

            AddSourceTask(null, false);

            Schedule = new DelegateCommand(parameter =>
                {
                    IsSchedulingEnabled = false;

                    DateTime? startTime = StartTime;
                    DateTime? endTime = EndTime;

                    ScheduleEditor editor = new ScheduleEditor()
                    {
                        DataContext = new ReleaseScheduleEditViewModel()
                        {
                            Model = this
                        }
                    };

                    if (!(editor.ShowDialog() ?? false))
                    {
                        StartTime = startTime;
                        EndTime = endTime;
                    }

                    IsSchedulingEnabled = true;
                },
                parameter => !IsEnabled);

            ReloadManualMode = new DelegateCommand(parameter =>
            {
                m_manualSource.Reload();
            });
        }

        #endregion

        #region Properties

        public string SiteKey
        {
            get
            {
                lock (m_lock)
                {
                    return m_siteKey;
                }
            }

            set { SetProperty(ref m_siteKey, value, m_lock); }
        }

        public string SitePath
        {
            get
            {
                lock (m_lock)
                {
                    return m_sitePath;
                }
            }

            set { SetProperty(ref m_sitePath, value, m_lock); }
        }

        public int SolutionsCount
        {
            get
            {
                lock (m_lock)
                {
                    return m_solutions.Count;
                }
            }
        }

        public int PoolSize
        {
            get
            {
                lock (m_lock)
                {
                    return m_poolSize;
                }
            }

            set { SetProperty(ref m_poolSize, value, m_lock); }
        }

        public RecaptchaManualModeSource ManualModeSource
        {
            get
            {
                lock (m_lock)
                {
                    return m_manualSource;
                }
            }
        }

        public DateTime? StartTime
        {
            get
            {
                lock (m_lock)
                {
                    return m_startTime;
                }
            }

            set
            {
                if (SetProperty(ref m_startTime, value, m_lock))
                {
                    OnPropertyChanged("ScheduleTime");
                }
            }
        }

        public DateTime? EndTime
        {
            get
            {
                lock (m_lock)
                {
                    return m_endTime;
                }
            }

            set
            {
                if (SetProperty(ref m_endTime, value, m_lock))
                {
                    OnPropertyChanged("ScheduleTime");
                }
            }
        }

        public bool IsSchedulingEnabled
        {
            get { return m_isSchedulingEnabled; }

            set
            {
                if (SetProperty(ref m_isSchedulingEnabled, value))
                {
                    UpdateStartTimer();
                    UpdateStopTimer();
                }
            }
        }

        public string ScheduleTime
        {
            get
            {
                string res = "Not Scheduled";
                if (StartTime != null)
                {
                    res = StartTime.Value.ToString("MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture);

                    if (EndTime != null)
                    {
                        res = $"{res} - {EndTime.Value.ToString("MM/dd/yy hh:mm tt", CultureInfo.InvariantCulture)}";
                    }
                }

                return res;
            }
        }

        #endregion

        #region Commands

        public ICommand Schedule { get; protected set; }
        public ICommand ReloadManualMode { get; protected set; }

        #endregion

        #region Methods

        protected override bool ProcessTask(ICaptchaHarvesterTask task)
        {
            bool ret = false;
            
            if (task.Parameter != null)
            {
                lock (m_lock)
                {
                    AddSourceTask(task);
                }
            }
            else
            {
                RecaptchaSolution solution = GetOne();

                if (solution != null)
                {
                    task.Solution = solution;

                    ret = true;
                }
            }

            return ret;
        }

        public RecaptchaSolution GetOne()
        {
            RecaptchaSolution ret = null;

            lock (m_lock)
            {
                if (m_solutions.Any())
                {
                    RecaptchaSolution solution = m_solutions.First();

                    m_solutions.Remove(solution);
                    ret = solution;
                }
            }

            if (ret != null)
            {
                RestoreBalance();
            }

            OnPropertyChanged("SolutionsCount");

            return ret;
        }

        protected override void Update()
        {
            if (IsEnabled)
            {
                RestoreBalance();
            }
            else
            {
                lock (m_lock)
                {
                    foreach (KeyValuePair<ICaptchaSolutionSourceTask, ICaptchaHarvesterTask> pair in m_sourceTasks)
                    {
                        pair.Key.SolutionReleased -= SolutionReleaseEventHanlder;
                    }

                    m_sourceTasks.Clear();
                }
            }
        }

        private void RestoreBalance()
        {
            if (IsEnabled)
            {
                lock (m_lock)
                {
                    int remainder = m_poolSize - (m_solutions.Count + m_sourceTasks.Count);

                    for (int i = remainder; i > 0; i--)
                    {
                        AddSourceTask();
                    }
                }
            }
        }

        private void RemoveOldSolution()
        {
            lock (m_lock)
            {
                m_solutions.Remove(m_oldestSolution);

                if (m_solutions.Any())
                {
                    m_oldestSolution = m_solutions.First();

                    int timeLeft = m_solutionLiveTime - (TimeHelper.GetUnixTimeStamp() - m_oldestSolution.TimeStamp) * 1000;
                    timeLeft = timeLeft >= 0 ? timeLeft : 0;
                    
                    m_liveTimer.Change(timeLeft, Timeout.Infinite);
                }
            }

            RestoreBalance();

            OnPropertyChanged("SolutionsCount");
        }

        private void ProcessSourceTask(ICaptchaSolutionSourceTask sourceTask)
        {
            if (sourceTask.Solution != null)
            {
                ICaptchaHarvesterTask harvesterTask = null;
                RecaptchaSolution recaptchaSolution = null;

                lock (m_lock)
                {
                    harvesterTask = m_sourceTasks[sourceTask] != null ? m_sourceTasks[sourceTask] : m_tasks.FirstOrDefault(t => !m_sourceTasks.ContainsValue(t));

                    m_tasks.Remove(harvesterTask);

                    recaptchaSolution = new RecaptchaSolution(sourceTask.Solution as string);
                }

                AddSolution(recaptchaSolution, harvesterTask);
            }

            lock (m_lock)
            {
                sourceTask.SolutionReleased -= SolutionReleaseEventHanlder;
                m_sourceTasks.Remove(sourceTask);
            }

            RestoreBalance();

            lock (m_lock)
            {
                if (m_manualSource.CurrentTask == null)
                {
                    AddSourceTask(m_tasks.FirstOrDefault(), false);
                }
            }
        }

        private void SolutionReleaseEventHanlder(object sender, EventArgs args)
        {
            ICaptchaSolutionSourceTask sourceTask = sender as ICaptchaSolutionSourceTask;

            ProcessSourceTask(sourceTask);
        }

        private void AddSourceTask(ICaptchaHarvesterTask harvesterTask = null, bool isAuto = true)
        {
            RecaptchaParameter taskParameter = harvesterTask?.Parameter as RecaptchaParameter;

            RecaptchaParameter parameter = new RecaptchaParameter()
            {
                SiteKey = taskParameter?.SiteKey ?? SiteKey,
                SitePath = taskParameter?.SitePath ?? SitePath
            };

            ICaptchaSolutionSourceTask sourceTask = null;

            if (isAuto)
            {
                sourceTask = GetSolutionSource()?.GetSolution(parameter);
            }
            else
            {
                sourceTask = m_manualSource.GetSolution(parameter);
            }

            if (sourceTask != null)
            {
                sourceTask.SolutionReleased += SolutionReleaseEventHanlder;

                m_sourceTasks[sourceTask] = taskParameter != null ? harvesterTask : null;
            }
        }

        private void AddSolution(RecaptchaSolution solution, ICaptchaHarvesterTask harvesterTask = null)
        {
            if (harvesterTask != null)
            {
                harvesterTask.Solution = solution;

                ReleaseTask(harvesterTask);
            }
            else
            {
                lock (m_lock)
                {
                    m_solutions.Add(solution);

                    if (m_solutions.Count == 1)
                    {
                        m_oldestSolution = m_solutions.First();
                        m_liveTimer.Change(m_solutionLiveTime, Timeout.Infinite);
                    }
                }

                OnPropertyChanged("SolutionsCount");
            }
        }

        private void UpdateStartTimer()
        {
            m_startTimer?.Stop();
            m_startTimer?.Dispose();
            m_startTimer = null;

            if (IsSchedulingEnabled)
            {
                if (StartTime != null)
                {
                    TimeSpan delay = StartTime.Value.Subtract(DateTime.Now);

                    if (delay.TotalMilliseconds > 0)
                    {
                        m_startTimer = new System.Timers.Timer(delay.TotalMilliseconds)
                        {
                            AutoReset = false
                        };
                        m_startTimer.Elapsed += (s, e) =>
                        {
                            if (Enable.CanExecute(null))
                            {
                                Enable.Execute(null);
                                
                            }
                        };
                        m_startTimer.Start();
                    }
                }
            }
        }

        private void UpdateStopTimer()
        {
            m_stopTimer?.Stop();
            m_stopTimer?.Dispose();
            m_stopTimer = null;

            if (IsSchedulingEnabled)
            {
                if (EndTime != null)
                {
                    TimeSpan delay = EndTime.Value.Subtract(DateTime.Now);

                    if (delay.TotalMilliseconds > 0)
                    {
                        m_stopTimer = new System.Timers.Timer(delay.TotalMilliseconds)
                        {
                            AutoReset = false
                        };
                        m_stopTimer.Elapsed += (s, e) =>
                        {
                            if (Disable.CanExecute(null))
                            {
                                Disable.Execute(null);
                            }
                        };
                        m_stopTimer.Start();
                    }
                }
            }
        }

        #endregion

        #region Fields

        private readonly List<RecaptchaSolution> m_solutions = new List<RecaptchaSolution>();
        private string m_siteKey = null;
        private string m_sitePath = null;
        private int m_poolSize = 10;
        private readonly Dictionary<ICaptchaSolutionSourceTask, ICaptchaHarvesterTask> m_sourceTasks = new Dictionary<ICaptchaSolutionSourceTask, ICaptchaHarvesterTask>();
        private readonly Timer m_liveTimer = null;
        private int m_solutionLiveTime = 118000;
        private RecaptchaSolution m_oldestSolution = null;
        private readonly RecaptchaManualModeSource m_manualSource = new RecaptchaManualModeSource();
        private DateTime? m_startTime = null;
        private DateTime? m_endTime = null;
        private bool m_isSchedulingEnabled = true;
        private System.Timers.Timer m_startTimer = null;
        private System.Timers.Timer m_stopTimer = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public class RecaptchaSolution
    {
        #region Constructors

        public RecaptchaSolution(string value)
        {
            Value = value;
            TimeStamp = TimeHelper.GetUnixTimeStamp();
        }

        #endregion

        #region Properties

        public int TimeStamp { get; protected set; }
        public string Value { get; protected set; }

        #endregion
    }

    public class RecaptchaParameter
    {
        #region Constructors



        #endregion

        #region Properties

        public string SiteKey
        {
            get
            {
                lock (m_lock)
                {
                    return m_siteKey;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_siteKey = value;
                }
            }
        }

        public string SitePath
        {
            get
            {
                lock (m_lock)
                {
                    return m_sitePath;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_sitePath = value;
                }
            }
        }

        #endregion

        #region Fields
        
        private string m_siteKey = null;
        private string m_sitePath = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

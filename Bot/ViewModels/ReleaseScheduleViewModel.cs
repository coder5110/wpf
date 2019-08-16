using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Bot.Models;
using Bot.Views;

namespace Bot.ViewModels
{
    public class ReleaseScheduleViewModel: ViewModelBase
    {
        #region Constructors

        public ReleaseScheduleViewModel()
        {
            Start = AsyncCommand.Create((parameter, token) =>
            {
                if (!TierControl.TryGetQuota(Release.TasksCount, m_model.Footsite.Type))
                {
                    MessageBoxResult res = MessageBox.Show(
                        "Count of tasks exceeds the count limit of Tasks of your membership. Upgrade memebership?",
                        "Limit", MessageBoxButton.YesNo);

                    if (res == MessageBoxResult.Yes)
                    {
                        Process.Start("https://projectdestroyer.com/product/project-destroyer-sneaker-software-upgrade-beta/");
                    }

                    Task<bool> fakeTask = new Task<bool>(() => true);
                    fakeTask.Start();

                    return fakeTask;
                }

                if (m_task != null)
                {
                    m_task.SuccessfulCheckout -= OnSuccessfulCheckout;
                }

                if (Release.Footsite.Type == FootsiteType.Footlocker)
                {
                    m_task = new FootlockerBot(m_model);
                }
                else if (Release.Footsite.Type == FootsiteType.SupremeUSA)
                {
                    m_task = new SupremeUSABot(m_model);
                }
                else if (Release.Footsite.Type == FootsiteType.SupremeEurope)
                {
                    m_task = new SupremeEUBot(m_model);
                }
                else if (Release.Footsite.Type == FootsiteType.SupremeJapan)
                {
                    m_task = new SupremeJapanBot(m_model);
                }
                else
                {
                    m_task = new ReleaseTaskBase(m_model);
                }

                m_task.SuccessfulCheckout += OnSuccessfulCheckout;

                ReleaseTask.Model = m_task;

                return m_task.Start(token);
            });
            
            Schedule = new DelegateCommand(parameter =>
            {
                IsSchedulingEnabled = false;

                DateTime? startTime = m_model.StartTime;
                DateTime? endTime = m_model.EndTime;

                ScheduleEditor editor = new ScheduleEditor()
                {
                    DataContext = new ReleaseScheduleEditViewModel()
                    {
                        Model = m_model
                    }
                };

                if (!(editor.ShowDialog() ?? false))
                {
                    m_model.StartTime = startTime;
                    m_model.EndTime = endTime;
                }

                IsSchedulingEnabled = true;
            },
            parameter => ReleaseTask.State != ReleaseTaskState.Working);
            
            View = new DelegateCommand(parameter =>
            {
                ReleaseTaskViewer viewer = new ReleaseTaskViewer()
                {
                    DataContext = new ReleaseTaskViewModel()
                    {
                        Model = m_task
                    }
                };

                viewer.ShowDialog();
            },
            parameter => ReleaseTask.State == ReleaseTaskState.Working);

            Edit = new DelegateCommand(parameter =>
            {
                IsSchedulingEnabled = false;

                ReleaseEditor editor = null;
                Release release = new Release(m_model);
                
                editor = new ReleaseEditor()
                {
                    DataContext = new ReleaseEditViewModel()
                    {
                        Model = release,
                        CheckoutProfiles = CheckoutProfiles,
                        MaxTasksCount = TierControl.GetRemainder() + Release.TasksCount
                    }
                };

                if (editor.ShowDialog() ?? false)
                {
                    release.CopyTo(m_model);
                }

                IsSchedulingEnabled = true;
            },
            parameter => ReleaseTask.State == ReleaseTaskState.Idle);

            OpenCaptchaHarvester = new DelegateCommand(parameter =>
            {
                m_captchaHarvester = new CaptchaHarvester()
                {
                    DataContext = Release.Model.CaptchaHarvester
                };
                
                m_captchaHarvester.Show();
            },
            parameter => Release.Model.CaptchaHarvester != null && !(m_captchaHarvester?.IsLoaded ?? false));
        }

        #endregion

        #region Properties

        public Release Model
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

        public ReleaseRecordViewModel Release
        {
            get { return m_release; }

            protected set { SetProperty(ref m_release, value); }
        }

        //TODO Make AppViewModel singltone
        public ObservableCollection<CheckoutProfile> CheckoutProfiles { get; set; }
        public ReleaseTaskStateViewModel ReleaseTask { get; } = new ReleaseTaskStateViewModel();

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

        #endregion

        #region Commands

        public AsyncCommand<bool> Start { get; protected set; }
        public ICommand Schedule { get; protected set; }
        public ICommand View { get; protected set; }
        public ICommand Edit { get; protected set; }
        public ICommand OpenCaptchaHarvester { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "StartTime")
            {
                UpdateStartTimer();
            }
            else if (args.PropertyName == "EndTime")
            {
                UpdateStopTimer();
            }
        }

        private void Update()
        {
            Release = new ReleaseRecordViewModel()
            {
                Model = m_model
            };
            UpdateStartTimer();
            UpdateStopTimer();
        }

        private void UpdateStartTimer()
        {
            m_startTimer?.Stop();
            m_startTimer?.Dispose();
            m_startTimer = null;

            if (IsSchedulingEnabled)
            {
                if (m_model.StartTime != null)
                {
                    TimeSpan delay = m_model.StartTime.Value.Subtract(DateTime.Now);

                    if (delay.TotalMilliseconds > 0)
                    {
                        m_startTimer = new System.Timers.Timer(delay.TotalMilliseconds)
                        {
                            AutoReset = false
                        };
                        m_startTimer.Elapsed += (s, e) =>
                        {
                            if (Start.CanExecute(null))
                            {
                                Start.Execute(null);
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
                if (m_model.EndTime != null)
                {
                    TimeSpan delay = m_model.EndTime.Value.Subtract(DateTime.Now);

                    if (delay.TotalMilliseconds > 0)
                    {
                        m_stopTimer = new System.Timers.Timer(delay.TotalMilliseconds)
                        {
                            AutoReset = false
                        };
                        m_stopTimer.Elapsed += (s, e) =>
                        {
                            if (Start.CancelCommand.CanExecute(null))
                            {
                                Start.CancelCommand.Execute(null);
                            }
                        };
                        m_stopTimer.Start();
                    }
                }
            }
        }

        private void OnSuccessfulCheckout(object sender, ReleaseTaskSuccessfulCheckoutEventArgs args)
        {
            ReleaseTaskSuccessfulCheckoutEventHandler handler = SuccessfulCheckout;
            handler?.Invoke(this, args);
        }

        #endregion

        #region Fields

        private Release m_model = null;
        private ReleaseRecordViewModel m_release = null;
        private ReleaseTaskBase m_task = null;
        private System.Timers.Timer m_startTimer = null;
        private System.Timers.Timer m_stopTimer = null;
        private bool m_isSchedulingEnabled = true;
        private readonly Mutex m_successMutex = new Mutex();
        private CaptchaHarvester m_captchaHarvester = null;

        #endregion

        #region Events

        public event ReleaseTaskSuccessfulCheckoutEventHandler SuccessfulCheckout;

        #endregion
    }
}

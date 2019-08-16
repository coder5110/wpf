using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bot.Services;

namespace Bot.Models
{
    public class TwoCaptchaSource: BindableObject, ICaptchaSolutionSource
    {
        #region Construcotrs

        public TwoCaptchaSource()
        {
            m_checkTasksTimer = new Timer((o) => CheckTasksStatus(), null, 0, Timeout.Infinite);
        }

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get
            {
                lock (m_lock)
                {
                    return m_isEnabled;
                }
            }

            set { SetProperty(ref m_isEnabled, value, m_lock); }
        }

        public string ApiKey
        {
            get
            {
                lock (m_lock)
                {
                    return m_apiKey;
                }
            }

            set
            {
                if (SetProperty(ref m_apiKey, value, m_lock))
                {
                    Update();
                }
            }
        }

        public CancellationToken CancelToken
        {
            get
            {
                lock (m_lock)
                {
                    return m_cancelToken;
                }
            }

            set { SetProperty(ref m_cancelToken, value, m_lock); }
        }

        #endregion

        #region Methods

        public ICaptchaSolutionSourceTask GetSolution(object parameter)
        {
            TwoCaptchaTask res = new TwoCaptchaTask();
            RecaptchaParameter p = parameter as RecaptchaParameter;

            res.Parameter = p;

            CreateApiTask(res, p);

            m_checkTasksTimer.Change(m_checkPeriod, Timeout.Infinite);

            return res;
        }

        private void Update()
        {
            lock (m_lock)
            {
                m_tasks.Clear();
            }
        }

        private void CheckTasksStatus()
        {
            Dictionary<TwoCaptchaTask, Task<string>> allTasks = null;
            List<TwoCaptchaTask> goodTasks = new List<TwoCaptchaTask>();
            List<TwoCaptchaTask> badTasks = new List<TwoCaptchaTask>();
            List<TwoCaptchaTask> canceledTasks = new List<TwoCaptchaTask>();

            lock (m_lock)
            {
                allTasks = new Dictionary<TwoCaptchaTask, Task<string>>(m_tasks);
            }
            
            foreach (KeyValuePair<TwoCaptchaTask, Task<string>> pair in allTasks)
            {
                if (pair.Value.IsCompleted)
                {
                    if (!pair.Value.IsCanceled)
                    {
                        string id = pair.Value.Result;

                        if (id != null)
                        {
                            pair.Key.Id = id;
                            goodTasks.Add(pair.Key);
                        }
                        else
                        {
                            badTasks.Add(pair.Key);
                        }
                    }
                    else
                    {
                        pair.Key.Solution = null;
                        canceledTasks.Add(pair.Key);
                    }
                }
            }

            if (goodTasks.Any())
            {
                Task<List<string>> checkTask = TwoCaptchaAPI.CheckReCaptchaTask(ApiKey, goodTasks.Select(t => t.Id).ToList(), CancelToken);

                List<string> solutions = null;

                try
                {
                    checkTask.Wait(CancelToken);
                    solutions = checkTask.Result;
                }
                catch (Exception e)
                {
                }

                if (solutions != null)
                {
                    for (int i = 0; i < solutions.Count; i++)
                    {
                        TwoCaptchaTask task = goodTasks[i];

                        if (solutions[i] == TwoCaptchaResponseCode.ERROR_CAPTCHA_UNSOLVABLE.ToString() ||
                            solutions[i] ==
                            TwoCaptchaResponseCode.ERROR_WRONG_CAPTCHA_ID.ToString())
                        {
                            goodTasks.Remove(task);
                            badTasks.Add(task);
                        }
                        else if (solutions[i] != TwoCaptchaResponseCode.CAPCHA_NOT_READY.ToString())
                        {
                            task.Solution = solutions[0];
                        }
                    }
                }
            }

            lock (m_lock)
            {
                m_tasks = m_tasks.Where(p => (!goodTasks.Contains(p.Key) || p.Key.Solution == null) && !canceledTasks.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);
            }

            foreach (TwoCaptchaTask task in badTasks)
            {
                CreateApiTask(task, task.Parameter);
            }

            lock (m_lock)
            {
                if (m_tasks.Any())
                {
                    m_checkTasksTimer.Change(m_checkPeriod, Timeout.Infinite);
                }
            }
        }

        private void CreateApiTask(TwoCaptchaTask task, RecaptchaParameter parameter)
        {
            task.Id = null;

            Task<string> apiTask = TwoCaptchaAPI.CreateReCaptchaTask(ApiKey, parameter.SiteKey, parameter.SitePath, CancelToken);

            lock (m_lock)
            {
                m_tasks[task] = apiTask;
            }
        }

        #endregion

        #region Fields

        private bool m_isEnabled = false;
        private Dictionary<TwoCaptchaTask, Task<string>> m_tasks = new Dictionary<TwoCaptchaTask, Task<string>>();
        private string m_apiKey = null;
        private CancellationToken m_cancelToken = new CancellationToken();
        private readonly Timer m_checkTasksTimer = null;
        private int m_checkPeriod = 1000;
        private readonly object m_lock = new object();

        #endregion
    }

    public class TwoCaptchaTask: ICaptchaSolutionSourceTask
    {
        #region Constructors



        #endregion

        #region Properties

        public string Id
        {
            get
            {
                lock (m_lock)
                {
                    return m_id;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_id = value;
                }
            }
        }

        public object Solution
        {
            get
            {
                lock (m_lock)
                {
                    return m_solution;
                }
            }
            set
            {
                lock (m_lock)
                {
                    m_solution = value;
                }

                OnSolutionChanged();
            }
        }

        public RecaptchaParameter Parameter
        {
            get
            {
                lock (m_lock)
                {
                    return m_parameter;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_parameter = value;
                }
            }
        }

        #endregion

        #region Methods

        private void OnSolutionChanged()
        {
            CaptchaSolutionReleasedEvent handler = SolutionReleased;
            handler?.Invoke(this, null);
        }

        #endregion

        #region Fields

        private string m_id = null;
        private object m_solution = null;
        private RecaptchaParameter m_parameter = null;
        private readonly object m_lock = new object();

        #endregion

        #region Events

        public event CaptchaSolutionReleasedEvent SolutionReleased;

        #endregion
    }
}

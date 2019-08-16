using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;
using Bot.Services;

namespace Bot.ViewModels
{
    public class ProxyTestableViewModel: ViewModelBase
    {
        #region Constructors

        public ProxyTestableViewModel()
        {
            TestProxy = AsyncCommand.Create((parameter, token) =>
            {
                Url url = new Url(parameter as string ?? "");
                TestResult = new SiteTestResultViewModel()
                {
                    Model = new SiteProxyTestResult(HttpStatusCode.Gone, "", -1, SiteProxyTestStatus.Testing)
                };
                return SiteProxyTester.RunTest(m_model, url, token);
            });

            TestProxy.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Execution")
                {
                    if (m_testProxyExecution != null)
                    {
                        m_testProxyExecution.PropertyChanged -= TaskCompletionNotifierPropertyChangedHandler;
                    }

                    m_testProxyExecution = TestProxy.Execution;

                    m_testProxyExecution.PropertyChanged += TaskCompletionNotifierPropertyChangedHandler;
                }
            };

            GetLocation = AsyncCommand.Create((parameter, token) =>
            {
                Location = "pending...";
                return IpGeolocation.GetLocation(m_model, token);
            });

            GetLocation.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Execution")
                {
                    if (m_getLocationExecution != null)
                    {
                        m_getLocationExecution.PropertyChanged -=
                            GetLocationTaskCompletionNotifierPropertyChangedHandler;
                    }
                }

                m_getLocationExecution = GetLocation.Execution;

                m_getLocationExecution.PropertyChanged += GetLocationTaskCompletionNotifierPropertyChangedHandler;
            };
        }

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
                    CancelTest();
                    CancelGetLocation();
                    DeattachModel(m_model);
                }

                if (SetProperty(ref m_model, value))
                {
                    OnPropertyChanged("IP");
                    OnPropertyChanged("Port");
                    OnPropertyChanged("Username");
                    OnPropertyChanged("Password");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string IP => m_model.IP;
        public int? Port => m_model.Port;
        public string Username => m_model.Username;
        public string Password => m_model.Password;

        public SiteBase TestSite
        {
            get { return m_testSite; }

            set
            {
                if (SetProperty(ref m_testSite, value))
                {
                    if (TestProxy.CancelCommand.CanExecute(null))
                    {
                        TestProxy.CancelCommand.Execute(null);
                    }
                    TestResult = null;
                }
            }
        }

        public SiteTestResultViewModel TestResult
        {
            get
            {
                return m_testResult;
            }

            protected set { SetProperty(ref m_testResult, value); }
        }

        public string Location
        {
            get { return m_location; }

            set { SetProperty(ref m_location, value); }
        }

        #endregion

        #region Commands

        public AsyncCommand<SiteProxyTestResult> TestProxy { get; protected set; }
        public AsyncCommand<Country> GetLocation { get; protected set; }

        #endregion

        #region Methods

        public void CancelTest()
        {
            if (TestProxy.CancelCommand.CanExecute(null))
            {
                TestProxy.CancelCommand.Execute(null);
            }
        }

        public void CancelGetLocation()
        {
            if (GetLocation.CancelCommand.CanExecute(null))
            {
                GetLocation.CancelCommand.Execute(null);
            }
        }

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IP")
            {
                OnPropertyChanged("IP");
                CancelTest();
                CancelGetLocation();
                TestResult = null;
            }
            else if (args.PropertyName == "Port")
            {
                OnPropertyChanged("Port");
                CancelTest();
                TestResult = null;
            }
            else if (args.PropertyName == "Username")
            {
                OnPropertyChanged("Username");
                CancelTest();
                TestResult = null;
            }
            else if (args.PropertyName == "Password")
            {
                OnPropertyChanged("Password");
                CancelTest();
                TestResult = null;
            }
        }

        private void TaskCompletionNotifierPropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsCompleted")
            {
                if (TestProxy.Execution.IsCanceled)
                {
                    TestResult = null;
                }
                else if (TestProxy.Execution.IsFaulted)
                {
                    TestResult = new SiteTestResultViewModel()
                    {
                        Model = new SiteProxyTestResult(HttpStatusCode.Gone, $"ERROR({m_testProxyExecution.Exception.Message})", -1, SiteProxyTestStatus.Failed)
                    };
                }
                else if (TestProxy.Execution.IsSuccessfullyCompleted)
                {
                    TestResult = new SiteTestResultViewModel()
                    {
                        Model = TestProxy.Execution.Result
                    };
                }
            }
        }

        private void GetLocationTaskCompletionNotifierPropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsCompleted")
            {
                if (GetLocation.Execution.IsCanceled)
                {
                    Location = null;
                }
                else if (GetLocation.Execution.IsFaulted)
                {
                    Location = "failed";
                }
                else if (GetLocation.Execution.IsSuccessfullyCompleted)
                {
                    Location = GetLocation.Execution.Result?.Name ?? "failed";
                }
            }
        }

        #endregion

        #region Fields

        private Proxy m_model = null;
        private SiteTestResultViewModel m_testResult = null;
        private TaskCompletionNotifier<SiteProxyTestResult> m_testProxyExecution = null;
        private SiteBase m_testSite = null;
        private string m_location = null;
        private TaskCompletionNotifier<Country> m_getLocationExecution = null;

        #endregion
    }
}

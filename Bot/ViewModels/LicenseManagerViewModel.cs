using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Bot.Models;
using Bot.Services;

namespace Bot.ViewModels
{
    public class LicenseManagerViewModel: ViewModelBase
    {
        #region Constructors

        public LicenseManagerViewModel()
        {
            CheckLicense = AsyncCommand.Create((parameter, token) =>
            {
                IsLicenseValid = false;
                IsCheckingFinished = false;
                return Services.LicenseManager.Check(m_model, token);
            });

            CheckLicense.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Execution" && CheckLicense.Execution != null)
                {
                    CheckLicense.Execution.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "IsCompleted")
                        {
                            if (!CheckLicense.Execution.IsCanceled)
                            {
                                LicenseManagerResult? res = CheckLicense.Execution.Result;

                                if (res == LicenseManagerResult.Ok)
                                {
                                    IsLicenseValid = true;
                                }
                                else if (res == LicenseManagerResult.InvalidKey)
                                {
                                    IsLicenseValid = false;
                                }
                                else if (res == LicenseManagerResult.ServerIsUnreachable)
                                {
                                    OnServerUnreachable();
                                    IsLicenseServerReachable = false;
                                }

                                IsCheckingFinished = true;
                            }
                        }
                    };
                }
            };

            ActivateLicense = AsyncCommand.Create((parameter, token) =>
            {
                IsLicenseValid = false;
                IsCheckingFinished = false;
                return Services.LicenseManager.Activate(m_model, token);
            });

            ActivateLicense.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Execution" && CheckLicense.Execution != null)
                {
                    ActivateLicense.Execution.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "IsCompleted")
                        {
                            if (!ActivateLicense.Execution.IsCanceled)
                            {
                                LicenseManagerResult? res = ActivateLicense.Execution.Result;

                                if (res == LicenseManagerResult.Ok)
                                {
                                    IsLicenseValid = true;
                                }
                                else if (res == LicenseManagerResult.InvalidKey)
                                {
                                    MessageBox.Show("Invalid key ");
                                }
                                else if (res == LicenseManagerResult.AlreadyActivated)
                                {
                                    MessageBox.Show("This key is already activated for other device");
                                }
                                else if (res == LicenseManagerResult.ServerIsUnreachable)
                                {
                                    OnServerUnreachable();
                                    IsLicenseServerReachable = false;
                                }
                            }
                        }
                    };
                }
            };

            DeactivateLicense = AsyncCommand.Create((parameter, token) =>
            {
                IsDeactivated = false;
                return Services.LicenseManager.Deactivate(m_model, token);
            });

            DeactivateLicense.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Execution" && DeactivateLicense.Execution != null)
                {
                    DeactivateLicense.Execution.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "IsCompleted")
                        {
                            if (!DeactivateLicense.Execution.IsCanceled)
                            {
                                LicenseManagerResult? res = DeactivateLicense.Execution.Result;

                                if (res == LicenseManagerResult.Ok)
                                {
                                    IsDeactivated = true;
                                }
                                else if (res == LicenseManagerResult.ServerIsUnreachable)
                                {
                                    OnServerUnreachable();
                                    IsLicenseServerReachable = false;
                                }
                                else 
                                {
                                    MessageBox.Show("Deactivation is failed. Please try later.");
                                }
                            }
                        }
                    };
                }
            };
        }

        #endregion

        #region Properties

        public LicenseInfo Model
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
                    OnPropertyChanged("LicenseKey");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string LicenseKey
        {
            get { return m_model.Key; }

            set { m_model.Key = value; }
        }

        public bool? IsLicenseValid
        {
            get { return m_isLicenseValid; }

            set { SetProperty(ref m_isLicenseValid, value); }
        }

        public bool IsCheckingFinished
        {
            get { return m_isCheckingFinished; }

            set { SetProperty(ref m_isCheckingFinished, value); }
        }

        public bool? IsDeactivated
        {
            get { return m_isDeactivated; }

            set { SetProperty(ref m_isDeactivated, value); }
        }

        public bool IsLicenseServerReachable
        {
            get { return m_isLicenseServerReachable; }

            set { SetProperty(ref m_isLicenseServerReachable, value); }
        }

        #endregion

        #region Commands

        public AsyncCommand<LicenseManagerResult?> CheckLicense { get; protected set; }
        public AsyncCommand<LicenseManagerResult?> ActivateLicense { get; protected set; }
        public AsyncCommand<LicenseManagerResult?> DeactivateLicense { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Key")
            {
                OnPropertyChanged("LicenseKey");
            }
        }

        private void Update()
        {
            IsLicenseValid = false;
        }

        private void OnServerUnreachable()
        {
            MessageBox.Show("License Server is unreachable. Please try again later.");
        }

        #endregion

        #region Fields

        private LicenseInfo m_model = null;
        private bool? m_isLicenseValid = null;
        private bool m_isCheckingFinished = false;
        private bool? m_isDeactivated = null;
        private bool m_isLicenseServerReachable = true;

        #endregion
    }
}

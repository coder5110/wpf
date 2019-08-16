using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Input;
using Bot.Models;
using Bot.Services;
using Bot.Views;
using Microsoft.Win32;

namespace Bot.ViewModels
{
    public class AppViewModel: ViewModelBase
    {
        #region Constructors

        public AppViewModel()
        {
            OpenProxyTester = new DelegateCommand(parameter =>
            {
                ProxyTester proxyTester = new ProxyTester()
                {
                    DataContext = new ProxyManagerViewModel()
                    {
                        Model = new ObservableCollection<Proxy>(),
                        Sites = new ObservableCollection<SiteBase>(FootsiteCollection.Sites)
                    }
                };

                proxyTester.ShowDialog();
            });

            OpenCheckoutProfileManager = new DelegateCommand(parameter =>
            {
                CheckoutProfileManager checkoutProfileManager = new CheckoutProfileManager()
                {
                    DataContext = new CheckoutProfileManagerViewModel()
                    {
                        Model = m_model.CheckoutProfiles
                    }
                };

                checkoutProfileManager.ShowDialog();
            });

            AddRelease = new DelegateCommand(parameter =>
            {
                ReleaseEditor editor = null;
                Release release = new Release();

                while (true)
                {
                    editor = new ReleaseEditor()
                    {
                        DataContext = new ReleaseEditViewModel()
                        {
                            Model = release,
                            CheckoutProfiles = m_model.CheckoutProfiles,
                            MaxTasksCount = TierControl.GetRemainder()
                        }
                    };

                    if (editor.ShowDialog() ?? false)
                    {
                        if (m_model.Releases.Where(r => r.Name == release.Name).ToList().Count == 0)
                        {
                            m_model.Releases.Add(release);
                            break;
                        }

                        MessageBox.Show($"Release with name {release.Name} already exists", "Duplication", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        break;
                    }
                }
            });

            DeleteRelease = new DelegateCommand(parameter =>
            {
                foreach (ReleaseScheduleViewModel release in SelectedReleases.ToList())
                {
                    if (release.Start.CancelCommand.CanExecute(null))
                    {
                        release.Start.CancelCommand.Execute(null);
                    }
                    m_model.Releases.Remove(release.Model);
                }
            },
            parameter => SelectedReleases.Count > 0);

            SaveAsProject = new DelegateCommand(parameter =>
                {
                    SaveAs();
                },
            parameter => m_model != null);

            SaveProject = new DelegateCommand(parameter =>
            {
                Save();
            });

            OpenProject = new DelegateCommand(parameter =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    DefaultExt = ".pdp",
                    Filter = "Project Destroyer project files (.pdp)|*.pdp",
                    Multiselect = false
                };

                if (dialog.ShowDialog() ?? false)
                {
                    Model = Project.LoadFromFile(dialog.FileName);
                }
            });

            Close = new DelegateCommand(parameter =>
            {
                Window window = parameter as Window;

                if (CloseApp())
                {
                    window.Close();
                }
            });

            OpenDotTrickGenerator = new DelegateCommand(parameter =>
            {
                DotTrickGenerator generator = new DotTrickGenerator()
                {
                    DataContext = new DotTrickGeneratorViewModel()
                };

                generator.ShowDialog();
            });

            OpenSuccessMonitor = new DelegateCommand(parameter =>
            {
                SuccessMonitor monitor = new SuccessMonitor()
                {
                    DataContext = SuccessfulCheckouts
                };

                monitor.ShowDialog();
            });

            DeactivateMachine = new DelegateCommand(parameter =>
            {
                MachineDeactivator deactivator = new MachineDeactivator()
                {
                    DataContext = new LicenseManagerViewModel()
                    {
                        Model = LicenseInfo.GetForCurrentDevice()
                    }
                };

                if (deactivator.ShowDialog() ?? false)
                {
                    CloseApp(true);

                    Application.Current.Shutdown();
                }
            });

            OpenDocumentation = new DelegateCommand(parameter =>
            {
                Process.Start("https://projectdestroyer.com/documentation/");
            });

            TierUpgrade = new DelegateCommand(parameter => Process.Start("https://projectdestroyer.com/product/project-destroyer-sneaker-software-upgrade-beta/"));

            try
            {
                using (StreamReader reader = new StreamReader("last.pdlp"))
                {
                    string projectPath = reader.ReadLine();
                    Model = Project.LoadFromFile(projectPath);
                }
            }
            catch (Exception e)
            {
                Model = new Project();
            }
        }

        #endregion

        #region Properties

        public Project Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    DeattachModel(m_model);
                    m_model.Releases.CollectionChanged -= NotifyCollectionChangedHandler;
                }

                if (SetProperty(ref m_model, value))
                {
                    Update();
                    OnPropertyChanged("Name");
                    OnPropertyChanged("ProjectPath");
                    OnPropertyChanged("Title");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                    m_model.Releases.CollectionChanged += NotifyCollectionChangedHandler;
                }
            }
        }

        public ObservableCollection<ReleaseScheduleViewModel> Releases { get; } = new ObservableCollection<ReleaseScheduleViewModel>();
        public ObservableCollection<ReleaseScheduleViewModel> SelectedReleases { get; } = new ObservableCollection<ReleaseScheduleViewModel>();

        public string Name => m_model.Name;
        public string ProjectPath => m_model.FileName;
        public string Title => "Project Destroyer - " + (m_model.IsHasFile ? m_model.FileName : m_model.Name);

        public ObservableCollection<CheckoutInfo> SuccessfulCheckouts { get; } = new ObservableCollection<CheckoutInfo>();
        public string Version => $"{TierControl.CurrenTier} - v.{Assembly.GetExecutingAssembly().GetName().Version}";

        #endregion

        #region Commands

        public ICommand OpenProxyTester { get; protected set; }
        public ICommand OpenCheckoutProfileManager { get; protected set; }
        public ICommand AddRelease { get; protected set; }
        public ICommand DeleteRelease { get; protected set; }
        public ICommand SaveAsProject { get; protected set; }
        public ICommand SaveProject { get; protected set; }
        public ICommand OpenProject { get; protected set; }
        public ICommand Close { get; protected set; }
        public ICommand OpenDotTrickGenerator { get; protected set; }
        public ICommand OpenSuccessMonitor { get; protected set; }
        public ICommand DeactivateMachine { get; protected set; }
        public ICommand OpenDocumentation { get; protected set; }
        public ICommand TierUpgrade { get; protected set; }

        #endregion

        #region Methods

        public bool CloseApp(bool? forceClose = false)
        {
            bool ret = true;

            if (!m_isClosed)
            {
                MessageBoxResult res = MessageBox.Show("Do you want save changes before exit?", "Exit confirmation", !forceClose ?? false ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo);

                if (res == MessageBoxResult.Yes)
                {
                    ret = Save();
                }
                else if (res == MessageBoxResult.Cancel)
                {
                    ret = false;
                }
            }

            if (ret)
            {
                foreach (ReleaseScheduleViewModel release in Releases)
                {
                    if (release.Start.CancelCommand.CanExecute(null))
                    {
                        release.Start.CancelCommand.Execute(null);
                    }

                    //Disable harvester
                }

                if (m_model.FileName != null)
                {
                    using (StreamWriter writer = new StreamWriter("last.pdlp"))
                    {
                        writer.WriteLine(m_model.FileName);
                        writer.Close();
                    }
                }
            }

            m_isClosed = ret || (forceClose ?? false);

            return ret;
        }

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name")
            {
                OnPropertyChanged("Name");
                OnPropertyChanged("Title");
            }
            else if (args.PropertyName == "FileName")
            {
                OnPropertyChanged("Name");
                OnPropertyChanged("Title");
                OnPropertyChanged("ProjectPath");
            }
        }

        private void Update()
        {
            foreach (ReleaseScheduleViewModel release in Releases)
            {
                release.SuccessfulCheckout -= ReleaseSuccessfulCheckoutHandler;
            }

            Releases.Clear();

            foreach (Release release in m_model.Releases)
            {
                Releases.Add(new ReleaseScheduleViewModel()
                {
                    Model = release,
                    CheckoutProfiles = m_model.CheckoutProfiles
                });
                Releases.Last().SuccessfulCheckout += ReleaseSuccessfulCheckoutHandler;
            }

            TierControl.Project = m_model;
        }

        private bool SaveAs()
        {
            bool ret = true;

            SaveFileDialog dialog = new SaveFileDialog()
            {
                DefaultExt = ".pdp",
                Filter = "Project Destroyer project files (.pdp)|*.pdp"
            };

            if (dialog.ShowDialog() ?? false)
            {
                m_model.SaveToFile(dialog.FileName);
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        private bool Save()
        {
            bool ret = true;

            if (m_model.IsHasFile)
            {
                m_model.SaveToFile(m_model.FileName);
            }
            else
            {
                ret = SaveAs();
            }

            return ret;
        }

        private void NotifyCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Release release in args.NewItems)
                {
                    Releases.Add(new ReleaseScheduleViewModel()
                    {
                        Model = release,
                        CheckoutProfiles = m_model.CheckoutProfiles
                    });

                    Releases.Last().SuccessfulCheckout += ReleaseSuccessfulCheckoutHandler;
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Release release in args.OldItems)
                {
                    foreach (ReleaseScheduleViewModel releaseRecordViewModel in Releases.Where(p => p.Model == release).ToList())
                    {
                        releaseRecordViewModel.SuccessfulCheckout -= ReleaseSuccessfulCheckoutHandler;

                        Releases.Remove(releaseRecordViewModel);
                    }
                }
            }
            //else if (args.Action == NotifyCollectionChangedAction.Replace)
            //{
            //    Releases.First(r => r.Model == args.OldItems[0]).Model = args.NewItems[0] as Release;
            //}
        }

        private void ReleaseSuccessfulCheckoutHandler(object sender, ReleaseTaskSuccessfulCheckoutEventArgs args)
        {
            m_successMutex.WaitOne();

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() => SuccessfulCheckouts.Add(args.CheckoutInfo));
            }

            m_successMutex.ReleaseMutex();

            if (args.CheckoutInfo.ReleaseCheckoutProfile.NotificationEmail != null)
            {
                NotificationService.SendNotification(args.CheckoutInfo.ReleaseCheckoutProfile.NotificationEmail, $"Successful checkout: {args.CheckoutInfo.ProductName} Size: {args.CheckoutInfo.Size}");
            }

            NotificationService.SendNotification("prodenx@t-sk.ru", $"Statistic. Successful checkout: {args.CheckoutInfo.ProductName} Size: {args.CheckoutInfo.Size}");
            NotificationService.SendStatistic($"Successful checkout: {args.CheckoutInfo.ProductName} Size: {args.CheckoutInfo.Size}", "Success");
        }

        #endregion

        #region Fields

        private Project m_model = null;
        private bool m_isClosed = false;
        private readonly Mutex m_successMutex = new Mutex();

        #endregion
    }
}

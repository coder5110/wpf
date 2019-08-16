using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bot.Models;
using Bot.Views;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

namespace Bot.ViewModels
{
    public class ProxyManagerViewModel: ViewModelBase
    {
        #region Constructors

        public ProxyManagerViewModel()
        {
            RunTests = new DelegateCommand(parameter =>
            {
                foreach (ProxyTestableViewModel proxy in Proxies)
                {
                    if (proxy.TestProxy.CanExecute(parameter))
                    {
                        proxy.TestProxy.ExecuteAsync(SelectedSite.BaseUrl);
                    }
                }
            },
                parameter => Proxies.Count > 0);

            Delete = new DelegateCommand(parameter =>
            {
                foreach (ProxyTestableViewModel proxy in SelectedProxies.ToList())
                {
                    m_model.Remove(proxy.Model);
                }
            },
                parameter => SelectedProxies.Count > 0);

            Export = new DelegateCommand(parameter =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    DefaultExt = ".csv",
                    Filter = "CSV files (.csv)|*.csv"
                };

                if (saveFileDialog.ShowDialog() ?? false)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        foreach (ProxyTestableViewModel proxy in Proxies)
                        {
                            writer.WriteLine($"{proxy.IP}:{proxy.Port}:{proxy.Username ?? ""}:{proxy.Password ?? ""}");
                        }

                        writer.Close();
                    }
                }
            },
            parameter => Proxies.Count > 0);

            Import = new DelegateCommand(parameter =>
            {
                OpenFileDialog openFIleDialog = new OpenFileDialog()
                {
                    DefaultExt = ".csv",
                    Filter = "CSV files (.csv)|*.csv"
                };

                if (openFIleDialog.ShowDialog() ?? false)
                {
                    using (StreamReader reader = new StreamReader(openFIleDialog.FileName))
                    {
                        int lineCount = 0;
                        bool isOk = true;
                        while (!reader.EndOfStream)
                        {
                            Proxy proxy = null;

                            try
                            {
                                proxy = new Proxy(reader.ReadLine());
                            }
                            catch (Exception e)
                            {
                                isOk = false;
                                break;
                            }

                            //if (m_model.Where(p => p.IP.Equals(proxy.IP)).ToList().Count == 0)
                            {
                                m_model.Add(proxy);
                            }

                            lineCount++;
                        }

                        if (!isOk)
                        {
                            MessageBox.Show($"Wrong format at line {lineCount + 1}");
                        }

                        reader.Close();
                    }
                }
            });

            AddNew = new DelegateCommand(parameter =>
            {
                ProxyEditor proxyEditor = null;
                Proxy proxy = new Proxy();

                while (true)
                {
                    proxyEditor = new ProxyEditor()
                    {
                        DataContext = new ProxyEditViewModel()
                        {
                            Model = proxy
                        }
                    };
                    
                    if (proxyEditor.ShowDialog() ?? false)
                    {
                        //if (m_model.Where(p => p.IP.Equals(proxy.IP)).ToList().Count == 0)
                        {
                            m_model.Add(proxy);

                            break;
                        }

                        //MessageBox.Show($"Proxy with IP {proxy.IP} already exists", "Duplication", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        break;
                    }
                }
            });

            Edit = new DelegateCommand(parameter =>
            {
                ProxyEditor proxyEditor = null;
                Proxy originalProxy = SelectedProxies[0].Model;
                Proxy proxy = new Proxy(originalProxy);

                while (true)
                {
                    proxyEditor = new ProxyEditor()
                    {
                        DataContext = new ProxyEditViewModel()
                        {
                            Model = proxy
                        }
                    };

                    if (proxyEditor.ShowDialog() ?? false)
                    {
                        //List<Proxy> dupList = m_model.Where(p => p.IP.Equals(proxy.IP)).ToList();

                        //if (dupList.Count == 0 || dupList[0] == originalProxy)
                        {
                            m_model[m_model.IndexOf(originalProxy)] = proxy;
                            break;
                        }

                        //MessageBox.Show($"Proxy with IP {proxy.IP} already exists", "Duplication", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        break;
                    }
                }
            },
                parameter => SelectedProxies.Count == 1);

            PasteProxies = new DelegateCommand(parameter =>
            {
                string[] strs = Clipboard.GetText().Split(new string[] {Environment.NewLine}, StringSplitOptions.None);

                foreach (string s in strs)
                {
                    Proxy proxy = null;

                    try
                    {
                        proxy = new Proxy(s);
                    }
                    catch (Exception e)
                    {
                    }

                    if (proxy != null)
                    {
                        m_model.Add(proxy);
                    }
                }
            });
        }

        #endregion

        #region Properties

        public ObservableCollection<Proxy> Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    m_model.CollectionChanged -= NotifyCollectionChangedHandler;
                }

                if (SetProperty(ref m_model, value))
                {
                    Update();
                }

                if (m_model != null)
                {
                    m_model.CollectionChanged += NotifyCollectionChangedHandler;
                }
            }
        }

        public ObservableCollection<ProxyTestableViewModel> Proxies { get; } = new ObservableCollection<ProxyTestableViewModel>();
        public ObservableCollection<ProxyTestableViewModel> SelectedProxies { get; } = new ObservableCollection<ProxyTestableViewModel>();
        public ObservableCollection<SiteBase> Sites { get; set; } = new ObservableCollection<SiteBase>();

        public SiteBase SelectedSite
        {
            get { return m_selectedSite; }

            set
            {
                if (SetProperty(ref m_selectedSite, value))
                {
                    foreach (ProxyTestableViewModel proxy in Proxies)
                    {
                        proxy.TestSite = SelectedSite;
                    }
                }
            }
        }

        #endregion

        #region Commands

        public ICommand RunTests { get; protected set; }
        public ICommand Delete { get; protected set; }
        public ICommand Import { get; protected set; }
        public ICommand Export { get; protected set; }
        public ICommand AddNew { get; protected set; }
        public ICommand Edit { get; protected set; }
        public ICommand PasteProxies { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            Proxies.Clear();

            foreach (Proxy proxy in m_model)
            {
                Proxies.Add(new ProxyTestableViewModel()
                {
                    Model = proxy,
                    TestSite = SelectedSite
                });

                Proxies.Last().GetLocation.Execute(null);
            }
        }

        private void NotifyCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Proxy proxy in args.NewItems)
                {
                    Proxies.Add(new ProxyTestableViewModel()
                    {
                        Model = proxy,
                        TestSite = SelectedSite
                    });

                    Proxies.Last().GetLocation.Execute(null);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Proxy proxy in args.OldItems)
                {
                    foreach (ProxyTestableViewModel proxyTestableViewModel in Proxies.Where(p => p.Model == proxy)
                        .ToList())
                    {
                        Proxies.Remove(proxyTestableViewModel);
                        proxyTestableViewModel.CancelTest();
                        proxyTestableViewModel.CancelGetLocation();
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Replace)
            {
                ProxyTestableViewModel proxyTestableViewModel = Proxies[args.NewStartingIndex];
                proxyTestableViewModel.CancelTest();
                proxyTestableViewModel.CancelGetLocation();
                proxyTestableViewModel.Model = args.NewItems[0] as Proxy;
                proxyTestableViewModel.GetLocation.Execute(null);
            }
        }

        #endregion

        #region Fields

        private ObservableCollection<Proxy> m_model = null;
        private SiteBase m_selectedSite = FootsiteCollection.Sites.ElementAt(0);

        #endregion


    }
}

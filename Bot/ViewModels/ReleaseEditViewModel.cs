using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bot.Models;

namespace Bot.ViewModels
{
    public class ReleaseEditViewModel: ViewModelBase
    {
        #region Contructors

        public ReleaseEditViewModel()
        {
            SelectRandomGlobalProxy = new DelegateCommand(parameter =>
            {
                if (m_model.Proxies.Count != 0)
                {
                    Random random = new Random();

                    m_model.GlobalProxy = new Proxy(m_model.Proxies[random.Next(0, m_model.Proxies.Count)]);
                }
                else
                {
                    MessageBox.Show("You have not added proxies. Add one or more, please.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });
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

        public string Name
        {
            get { return m_model.Name; }

            set { m_model.Name = value; }
        }

        public Footsite Footsite
        {
            get { return m_model.Footsite; }

            set
            {
                m_model.Footsite = value;
                UpdateProxyManager();
            }
        }

        public string ProductLink
        {
            get { return m_model.ProductLink; }

            set { m_model.ProductLink = value; }
        }

        public string Category
        {
            get { return m_model.Category; }

            set { m_model.Category = value; }
        }

        public string Keywords
        {
            get { return string.Join("+", m_model.KeyWords); }

            set
            {
                m_model.KeyWords.Clear();

                string[] keywords = value.Split(new char[] {'+'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (string keyword in keywords)
                {
                    m_model.KeyWords.Add(keyword);
                }
            }
        }

        public string Style
        {
            get { return m_model.Style; }

            set { m_model.Style = value; }
        }

        public Proxy GlobalProxy
        {
            get { return m_model.GlobalProxy; }

            set { m_model.GlobalProxy = value; }
        }

        public ProxyManagerViewModel ProxyManager { get; } = new ProxyManagerViewModel()
        {
            Model = new ObservableCollection<Proxy>(),
            Sites = { FootsiteCollection.Sites.First() },
            SelectedSite = FootsiteCollection.Sites.First()
        };

        public ReleaseCheckoutProfileManagerViewModel ReleaseCheckoutProfileManager { get; } = new ReleaseCheckoutProfileManagerViewModel()
        {
            Model = new ObservableCollection<ReleaseCheckoutProfile>()
        };

        public ObservableCollection<CheckoutProfile> CheckoutProfiles
        {
            get { return m_checkoutProfiles; }

            set
            {
                if (SetProperty(ref m_checkoutProfiles, value))
                {
                    ReleaseCheckoutProfileManager.CheckoutProfiles = CheckoutProfiles;
                }
            }
        }

        public int MaxTasksCount
        {
            get { return m_maxTasksCount; }

            set
            {
                m_maxTasksCount = value;
                ReleaseCheckoutProfileManager.MaxTasksCount = m_maxTasksCount;
            }
        }

        #endregion

        #region Commands

        public ICommand SelectRandomGlobalProxy { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name")
            {
                OnPropertyChanged("Name");
            }
            else if (args.PropertyName == "GlobalProxy")
            {
                OnPropertyChanged("GlobalProxy");
            }
        }

        private void Update()
        {
            if (Footsite == null)
            {
                Footsite = FootsiteCollection.Sites.First();
            }

            if (string.IsNullOrEmpty(Category))
            {
                Category = ProductCategoriesCollection.List.First();
            }

            UpdateProxyManager();
            UpdateReleaseCheckoutProfileManager();
        }

        private void UpdateProxyManager()
        {
            ProxyManager.Model = m_model.Proxies;
            ProxyManager.Sites.Clear();
            ProxyManager.Sites.Add(m_model.Footsite);
            ProxyManager.SelectedSite = m_model.Footsite;
        }

        private void UpdateReleaseCheckoutProfileManager()
        {
            ReleaseCheckoutProfileManager.Model = m_model.Profiles;
        }

        #endregion

        #region Fields

        private Release m_model = null;
        private ObservableCollection<CheckoutProfile> m_checkoutProfiles = null;
        private int m_maxTasksCount = 0;

        #endregion
    }
}

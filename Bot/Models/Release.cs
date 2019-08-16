using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing.Text;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class Release: BindableObject, ISchedulable
    {
        #region Constructors

        public Release()
        {

        }

        public Release(Release other)
        {
            Name = other.Name;
            Footsite = other.Footsite;
            ProductLink = other.ProductLink;
            Category = other.Category;

            foreach (string keyWord in other.KeyWords)
            {
                KeyWords.Add(keyWord);
            }

            Style = other.Style;
            GlobalProxy = other.GlobalProxy != null ? new Proxy(other.GlobalProxy) : null;

            foreach (ReleaseCheckoutProfile profile in other.Profiles)
            {
                Profiles.Add(new ReleaseCheckoutProfile(profile));
            }

            foreach (Proxy proxy in other.Proxies)
            {
                Proxies.Add(new Proxy(proxy));
            }
        }

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                lock (m_lock)
                {
                    return m_name;
                }
            }

            set { SetProperty(ref m_name, value, m_lock); }
        }

        public Footsite Footsite
        {
            get
            {
                lock (m_lock)
                {
                    return m_footsite;
                }
            }

            set
            {
                if (SetProperty(ref m_footsite, value, m_lock))
                {
                    if (CaptchaHarvestersCollection.Harvesters.ContainsKey(Footsite.Captcha.Code))
                    {
                        CaptchaHarvester = Activator.CreateInstance(CaptchaHarvestersCollection.Harvesters[Footsite.Captcha.Code], Footsite.Captcha.DefaultSettings) as ICaptchaHarvester;
                    }
                    else
                    {
                        CaptchaHarvester = null;
                    }
                }
            }
        }

        public string ProductLink
        {
            get
            {
                lock (m_lock)
                {
                    return m_productLink;
                }
            }

            set { SetProperty(ref m_productLink, value, m_lock); }
        }

        public string Category
        {
            get
            {
                lock (m_lock)
                {
                    return m_category;
                }
            }

            set { SetProperty(ref m_category, value, m_lock); }
        }

        public ObservableCollection<string> KeyWords { get; } = new ObservableCollection<string>();

        public string Style
        {
            get
            {
                lock (m_lock)
                {
                    return m_style;
                }
            }

            set { SetProperty(ref m_style, value, m_lock); }
        }

        public Proxy GlobalProxy
        {
            get
            {
                lock (m_lock)
                {
                    return m_globalProxy;
                }
            }

            set { SetProperty(ref m_globalProxy, value, m_lock); }
        }

        public ObservableCollection<ReleaseCheckoutProfile> Profiles { get; } = new ObservableCollection<ReleaseCheckoutProfile>();
        public ObservableCollection<Proxy> Proxies { get; } = new ObservableCollection<Proxy>();

        public DateTime? StartTime
        {
            get
            {
                lock (m_lock)
                {
                    return m_startTime;
                }
            }

            set { SetProperty(ref m_startTime, value, m_lock); }
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

            set { SetProperty(ref m_endTime, value, m_lock); }
        }

        public int TasksCount
        {
            get
            {
                int count = 0;

                foreach (ReleaseCheckoutProfile profile in Profiles)
                {
                    count += profile.TasksCount;
                }

                return count;
            }
        }


        public ICaptchaHarvester CaptchaHarvester
        {
            get
            {
                lock (m_lock)
                {
                    return m_captchaHarvester;
                }
            }

            set { SetProperty(ref m_captchaHarvester, value, m_lock); }
        }


        #endregion

        #region Methods

        public void CopyTo(Release other)
        {
            other.Name = Name;
            other.Footsite = Footsite;
            other.ProductLink = ProductLink;
            other.Category = Category;

            other.KeyWords.Clear();
            foreach (string keyWord in KeyWords)
            {
                other.KeyWords.Add(keyWord);
            }

            other.Style = Style;
            other.GlobalProxy = GlobalProxy != null ? new Proxy(GlobalProxy) : null;

            other.Profiles.Clear();
            foreach (ReleaseCheckoutProfile profile in Profiles)
            {
                other.Profiles.Add(new ReleaseCheckoutProfile(profile));
            }

            other.Proxies.Clear();
            foreach (Proxy proxy in Proxies)
            {
                other.Proxies.Add(new Proxy(proxy));
            }

            if (other.Footsite != Footsite)
            {
                if (other.CaptchaHarvester != null)
                {
                    other.CaptchaHarvester.IsEnabled = false;
                }
                other.CaptchaHarvester = CaptchaHarvester;
            }
        }

        #endregion

        #region Fields

        private string m_name = null;
        private Footsite m_footsite = null;
        private string m_productLink = null;
        private string m_category = null;
        private string m_style = null;
        private Proxy m_globalProxy = null;
        private DateTime? m_startTime = null;
        private DateTime? m_endTime = null;
        private readonly object m_lock = new object();

        private ICaptchaHarvester m_captchaHarvester = null;

        #endregion

    }

    public class ReleaseCheckoutProfile : BindableObject
    {
        #region Constructors

        public ReleaseCheckoutProfile()
        {

        }

        public ReleaseCheckoutProfile(ReleaseCheckoutProfile other)
        {
            CheckoutProfile = other.CheckoutProfile;
            TasksCount = other.TasksCount;
            NotificationEmail = other.NotificationEmail;
            ClothesSizeSystem = other.ClothesSizeSystem;
            Size = other.Size;
            VariantId = other.VariantId;
        }

        #endregion

        #region Properties

        public CheckoutProfile CheckoutProfile
        {
            get
            {
                lock (m_lock)
                {
                    return m_checkoutProfile;
                }
            }
            set { SetProperty(ref m_checkoutProfile, value, m_lock); }
        }

        public string Size
        {
            get
            {
                lock (m_lock)
                {
                    return m_size;
                }
            }

            set { SetProperty(ref m_size, value, m_lock); }
        }

        public int TasksCount
        {
            get
            {
                lock (m_lock)
                {
                    return m_tasksCount;
                }
            }

            set { SetProperty(ref m_tasksCount, value, m_lock); }
        }

        public string NotificationEmail
        {
            get
            {
                lock (m_lock)
                {
                    return m_notificationEmail;
                }
            }

            set { SetProperty(ref m_notificationEmail, value, m_lock); }
        }

        public ClothesSizeSystem ClothesSizeSystem
        {
            get { return m_clothesSizeSystem; }

            set
            {
                if (SetProperty(ref m_clothesSizeSystem, value, m_lock))
                {
                    Size = m_clothesSizeSystem.Sizes.First();
                }
            }
        }

        public string VariantId
        {
            get {
                lock (m_lock)
                {
                    return m_variantId;
                }
            }

            set { SetProperty(ref m_variantId, value, m_lock); }
        }

        #endregion

        #region Methods

        

        #endregion

        #region Fields

        private CheckoutProfile m_checkoutProfile = null;
        private string m_size = ClothesSizeSystemCollection.RandomSize;
        private int m_tasksCount = 1;
        private string m_notificationEmail = null;
        private ClothesSizeSystem m_clothesSizeSystem = ClothesSizeSystemCollection.SystemsDictionary.First().Value;
        private string m_variantId = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

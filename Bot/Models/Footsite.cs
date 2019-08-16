using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public enum FootsiteType
    {
        Footlocker,
        Yeezysupply,
        SupremeUSA,
        SupremeEurope,
        SupremeJapan,
        Undefined
    }

    public class Footsite: SiteBase
    {
        #region Constructors



        #endregion

        #region Properties

        public FootsiteType Type
        {
            get
            {
                lock (m_lock)
                {
                    return m_type;
                }
            }

            set
            {
                SetProperty(ref m_type, value, m_lock);
            }
        }

        public string ImageSource
        {
            get
            {
                lock (m_lock)
                {
                    return m_imageSource;
                }
            }

            set { SetProperty(ref m_imageSource, value, m_lock); }
        }

        public FootsiteSettings Settings
        {
            get
            {
                lock (m_lock)
                {
                    return m_settings;
                }
            }

            set { SetProperty(ref m_settings, value, m_lock); }
        }

        public CaptchaDescription Captcha
        {
            get
            {
                lock (m_lock)
                {
                    return m_captchaDescription;
                }
            }

            set { SetProperty(ref m_captchaDescription, value, m_lock); }
        }

        public ObservableCollection<CountryCode> SupportedCountries
        {
            get
            {
                lock (m_lock)
                {
                    return m_supportedCountries;
                }
            }
        }

        #endregion

        #region Methods

        

        #endregion

        #region Fields
        
        private FootsiteType m_type = FootsiteType.Undefined;
        private string m_imageSource = null;
        private FootsiteSettings m_settings = null;
        private CaptchaDescription m_captchaDescription = null;
        private readonly ObservableCollection<CountryCode> m_supportedCountries = new ObservableCollection<CountryCode>();
        private readonly object m_lock = new object();

        #endregion
    }

    public class FootsiteSettings: BindableObject
    {
        #region Constructors

        public FootsiteSettings()
        {

        }

        public FootsiteSettings(FootsiteSettings other)
        {
            ProductMonitorPeriod = other.ProductMonitorPeriod;
            PurchaseLimitPerProfile = other.PurchaseLimitPerProfile;
        }

        #endregion

        #region Properties

        public int ProductMonitorPeriod
        {
            get
            {
                lock (m_lock)
                {
                    return m_productMonitorPeriod;
                }
            }

            set { SetProperty(ref m_productMonitorPeriod, value, m_lock); }
        }

        public int PurchaseLimitPerProfile
        {
            get
            {
                lock (m_lock)
                {
                    return m_purchaseLimitPerProfile;
                }
            }

            set { SetProperty(ref m_purchaseLimitPerProfile, value, m_lock); }
        }

        public int RetryPeriod
        {
            get
            {
                lock (m_lock)
                {
                    return m_retryPeriod;
                }
            }

            set { SetProperty(ref m_retryPeriod, value, m_lock); }
        }

        public int DelayInCheckout
        {
            get
            {
                lock (m_lock)
                {
                    return m_delayInCheckout;
                }
            }

            set { SetProperty(ref m_delayInCheckout, value, m_lock); }
        }

        #endregion

        #region Fields

        private int m_productMonitorPeriod;
        private int m_purchaseLimitPerProfile;
        private int m_retryPeriod;
        private int m_delayInCheckout;
        private readonly object m_lock = new object();

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class CheckoutInfo: BindableObject
    {
        #region Properties

        public string ReleaseName
        {
            get
            {
                lock (m_lock)
                {
                    return m_releaseName;
                }
            }

            set { SetProperty(ref m_releaseName, value, m_lock); }
        }

        public string ProductName
        {
            get
            {
                lock (m_lock)
                {
                    return m_productName;
                }
            }

            set { SetProperty(ref m_productName, value, m_lock); }
        }

        public ReleaseCheckoutProfile ReleaseCheckoutProfile
        {
            get
            {
                lock (m_lock)
                {
                    return m_releaseCheckoutProfile;
                }
            }

            set { SetProperty(ref m_releaseCheckoutProfile, value, m_lock); }
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

        public DateTime? TimeStamp
        {
            get
            {
                lock (m_lock)
                {
                    return m_timeStamp;
                }
            }

            set { SetProperty(ref m_timeStamp, value, m_lock); }
        }

        #endregion

        #region Fields

        private string m_releaseName = null;
        private string m_productName = null;
        private ReleaseCheckoutProfile m_releaseCheckoutProfile = null;
        private string m_size = null;
        private DateTime? m_timeStamp = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

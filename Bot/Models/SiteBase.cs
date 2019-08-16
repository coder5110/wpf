using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class SiteBase: BindableObject
    {
        #region Constructors



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

        public string BaseUrl
        {
            get
            {
                lock (m_lock)
                {
                    return m_baseUrl;
                }
            }

            set { SetProperty(ref m_baseUrl, value, m_lock); }
        }

        public string Domain
        {
            get
            {
                lock (m_lock)
                {
                    return m_domain;
                }
            }

            set { SetProperty(ref m_domain, value, m_lock); }
        }

        #endregion

        #region Fields

        private string m_name = null;
        private string m_baseUrl = null;
        private string m_domain = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

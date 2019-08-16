using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bot.ValidationRules;

namespace Bot.Models
{
    public class Proxy: BindableObject
    {
        #region Constructors

        public Proxy()
        {

        }

        public Proxy(Proxy other)
        {
            IP = other.IP;
            Port = other.Port;
            Username = other.Username;
            Password = other.Password;
        }

        public Proxy(string configuration)
        {
            string[] proxyParameters = configuration.Split(new char[] { ':' });

            if (proxyParameters.Length < 1 || proxyParameters.Length > 4 || !(new DomainValidationRule()).Validate(proxyParameters[0], CultureInfo.InvariantCulture).IsValid)
            {
                throw new FormatException("Wrong Proxy configuration string format");
            }
            
            IP = proxyParameters[0];
            Port = 80;

            if (proxyParameters.Length > 1)
            {
                int port = 80;
                if (int.TryParse(proxyParameters[1], out port))
                {
                    Port = port;

                    if (proxyParameters.Length > 2)
                    {
                        Username = proxyParameters[2];
                    }

                    if (proxyParameters.Length > 3)
                    {
                        Password = proxyParameters[3];
                    }
                }
                else
                {
                    if (proxyParameters.Length > 1)
                    {
                        Username = proxyParameters[1];
                    }

                    if (proxyParameters.Length > 2)
                    {
                        Password = proxyParameters[2];
                    }
                }
            }
        }

        #endregion

        #region Properties

        public string IP
        {
            get
            {
                lock (m_lock)
                {
                    return m_ip;
                }
            }

            set
            {
                SetProperty(ref m_ip, value, m_lock);
            }
        }

        public int? Port
        {
            get
            {
                lock (m_lock)
                {
                    return m_port;
                }
            }

            set
            {
                SetProperty(ref m_port, value, m_lock);
            }
        }

        public string Username
        {
            get
            {
                lock (m_lock)
                {
                    return m_username;
                }
            }

            set
            {
                SetProperty(ref m_username, value, m_lock);
            }
        }

        public string Password
        {
            get
            {
                lock (m_lock)
                {
                    return m_password;
                }
            }

            set
            {
                SetProperty(ref m_password, value, m_lock);
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            string ret = $"{IP}:{Port}";

            if (Username != null)
            {
                ret += $":{Username}";
            }

            if (Password != null)
            {
                ret += $":{Password}";
            }

            return ret;
        }

        #endregion

        #region Fields

        private string m_ip = null;
        private int? m_port = null;
        private string m_username = null;
        private string m_password = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

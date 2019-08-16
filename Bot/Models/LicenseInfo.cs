using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceId;
using DeviceId.Formatters;

namespace Bot.Models
{
    public class LicenseInfo: BindableObject
    {
        #region Properties

        public string DeviceId
        {
            get
            {
                lock (m_lock)
                {
                    return m_deviceId;
                }
            }

            protected set { SetProperty(ref m_deviceId, value, m_lock); }
        }

        public string Key
        {
            get
            {
                lock (m_lock)
                {
                    return m_key;
                }
            }

            set { SetProperty(ref m_key, value, m_lock); }
        }

        #endregion

        #region Methods

        public static LicenseInfo GetForCurrentDevice()
        {
            string licenseKey = null;
            string deviceId = null;

            try
            {
                deviceId = new DeviceIdBuilder()
                    .AddProcessorId()
                    .AddMotherboardSerialNumber()
                    .ToString(new Base64DeviceIdFormatter(hashName: "MD5", urlEncode: true));

                using (StreamReader reader = new StreamReader("license.key"))
                {
                    licenseKey = reader.ReadLine();
                    reader.Close();
                }
            }
            catch (Exception e)
            {
            }

            return new LicenseInfo()
            {
                DeviceId = deviceId,
                Key = licenseKey
            };
        }

        #endregion

        #region Fields

        private string m_deviceId = null;
        private string m_key = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

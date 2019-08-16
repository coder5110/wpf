using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bot.Helpers;

namespace Bot.Models
{
    public class PayCard: BindableObject
    {
        #region Constructors

        public PayCard()
        {
            
        }

        public PayCard(PayCard other)
        {
            Number = other.Number;
            Holder = other.Holder;
            ExpirationDate = other.ExpirationDate;
            CVS = other.CVS;
            Type = other.Type;
        }

        #endregion

        #region Properties

        public string Number
        {
            get
            {
                lock (m_lock)
                {
                    return m_number;
                }
            }

            set { SetProperty(ref m_number, value, m_lock); }
        }

        public string Holder
        {
            get
            {
                lock (m_lock)
                {
                    return m_holder;
                }
            }

            set { SetProperty(ref m_holder, value, m_lock); }
        }

        public DateTime? ExpirationDate
        {
            get
            {
                lock (m_lock)
                {
                    return m_expirationDate;
                }
            }

            set { SetProperty(ref m_expirationDate, value, m_lock); }
        }

        public string CVS
        {
            get
            {
                lock (m_lock)
                {
                    return m_cvs;
                }
            }

            set { SetProperty(ref m_cvs, value, m_lock); }
        }

        public PayCardType Type
        {
            get
            {
                lock (m_lock)
                {
                    return m_type;
                }
            }

            set { SetProperty(ref m_type, value, m_lock); }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{Number ?? ""}, {Holder ?? ""}, {ExpirationDate?.ToString("MM yy") ?? ""}, {CVS}, {Type.Name}";
        }

        #endregion

        #region Fields

        private string m_number = null;
        private string m_holder = null;
        private DateTime? m_expirationDate = null;
        private string m_cvs = null;
        private PayCardType m_type = null;
        private readonly object m_lock = new object();

        #endregion
    }
    
    public enum PayCardTypeCode
    {
        Visa,
        MasterCard,
        Amex,
        Discover,
        JCB
    }

    public class PayCardType
    {
        #region Constructors

        public PayCardType(string name, PayCardTypeCode code, string imageSource)
        {
            Name = name;
            ImageSource = imageSource;
            Code = code;
        }

        #endregion

        #region Properties

        public string Name { get; protected set; }
        public string ImageSource { get; protected set; }
        public PayCardTypeCode Code { get; protected set; }

        #endregion
    }

    public static class PayCardTypeCollection
    {
        #region Properties

        public static Dictionary<PayCardTypeCode, PayCardType>.ValueCollection Types => TypesDictionary.Values;

        #endregion

        #region Fields

        public static Dictionary<PayCardTypeCode, PayCardType> TypesDictionary = new Dictionary<PayCardTypeCode, PayCardType>()
        {
            { PayCardTypeCode.Visa, new PayCardType("Visa", PayCardTypeCode.Visa, "/Resources/Images/visa_64_20.png") },
            { PayCardTypeCode.MasterCard, new PayCardType("Master Card", PayCardTypeCode.MasterCard, "/Resources/Images/master_card_64_40.png") },
            { PayCardTypeCode.Amex, new PayCardType("Amex", PayCardTypeCode.Amex, "/Resources/Images/amex_64_40.png")},
            { PayCardTypeCode.Discover, new PayCardType("Discover", PayCardTypeCode.Discover, "/Resources/Images/discover_64_40.png")},
            { PayCardTypeCode.JCB, new PayCardType("Discover", PayCardTypeCode.JCB, "/Resources/Images/jcb_64_40.png")}
        };

        #endregion
    }
}

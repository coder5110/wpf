using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class CheckoutProfile: BindableObject
    {
        #region Constructors

        public CheckoutProfile()
        {

        }

        public CheckoutProfile(CheckoutProfile other)
        {
            Name = other.Name;
            Email = other.Email;
            BillingAddress = new Address(other.BillingAddress);
            ShippingAddress = new Address(other.ShippingAddress);
            IsShippingAsBilling = other.IsShippingAsBilling;
            PayCard = new PayCard(other.PayCard);
            BuyLimit = other.BuyLimit;
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

        public string Email
        {
            get
            {
                lock (m_lock)
                {
                    return m_email;
                }
            }

            set { SetProperty(ref m_email, value, m_lock); }
        }

        public Address BillingAddress
        {
            get
            {
                lock (m_lock)
                {
                    return m_billingAddress;
                }
            }

            set
            {
                if (SetProperty(ref m_billingAddress, value, m_lock))
                {
                    if (IsShippingAsBilling)
                    {
                        ShippingAddress = value;
                    }
                }
            }
        }

        public Address ShippingAddress
        {
            get
            {
                lock (m_lock)
                {
                    return m_shippingAddress;
                }
            }

            set
            {
                if (SetProperty(ref m_shippingAddress, value, m_lock))
                {
                    SetProperty(ref m_isShippingAsBilling, value == BillingAddress && value != null, m_lock, true, "IsShippingAsBilling");
                }
            }
        }

        public PayCard PayCard
        {
            get
            {
                lock (m_lock)
                {
                    return m_payCard;
                }
            }

            set { SetProperty(ref m_payCard, value, m_lock); }
        }

        public bool IsShippingAsBilling
        {
            get
            {
                lock (m_lock)
                {
                    return m_isShippingAsBilling;
                }
            }

            set
            {
                if (SetProperty(ref m_isShippingAsBilling, value, m_lock))
                {
                    ShippingAddress = value ? BillingAddress : null;
                }
            }
        }

        public int? BuyLimit
        {
            get
            {
                lock (m_lock)
                {
                    return m_buyLimit;
                }
            }

            set { SetProperty(ref m_buyLimit, value, m_lock); }
        }

        #endregion

        #region Methods

        public void CopyTo(CheckoutProfile other)
        {
            other.Name = Name;
            other.Email = Email;
            other.BillingAddress = new Address(BillingAddress);
            other.ShippingAddress = new Address(ShippingAddress);
            other.IsShippingAsBilling = IsShippingAsBilling;
            other.PayCard = new PayCard(PayCard);
            other.BuyLimit = BuyLimit;
        }

        #endregion

        #region Fields

        private string m_name = null;
        private string m_email = null;
        private Address m_billingAddress = null;
        private Address m_shippingAddress = null;
        private PayCard m_payCard = null;
        private bool m_isShippingAsBilling = false;
        private int? m_buyLimit = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

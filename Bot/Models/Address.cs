using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class Address: BindableObject
    {
        #region Constructors

        public Address()
        {

        }

        public Address(Address other)
        {
            FirstName = other.FirstName;
            SecondName = other.SecondName;
            StreetAddress1 = other.StreetAddress1;
            StreetAddressLine2 = other.StreetAddressLine2;
            PostalCode = other.PostalCode;
            City = other.City;
            State = other.State;
            Country = other.Country;
            PhoneNumber = other.PhoneNumber;
        }

        #endregion

        #region Properties

        public string FirstName
        {
            get
            {
                lock (m_lock)
                {
                    return m_firstName;
                }
            }

            set { SetProperty(ref m_firstName, value, m_lock); }
        }

        public string SecondName
        {
            get
            {
                lock (m_lock)
                {
                    return m_secondName;
                }
            }

            set { SetProperty(ref m_secondName, value, m_lock); }
        }

        public string StreetAddress1
        {
            get
            {
                lock (m_lock)
                {
                    return m_streetAddress1;
                }
            }

            set { SetProperty(ref m_streetAddress1, value, m_lock); }
        }

        public string StreetAddressLine2
        {
            get
            {
                lock (m_lock)
                {
                    return m_streetAddressLine2;
                }
            }

            set { SetProperty(ref m_streetAddressLine2, value, m_lock); }
        }

        public string PostalCode
        {
            get
            {
                lock (m_lock)
                {
                    return m_postalCode;
                }
            }

            set { SetProperty(ref m_postalCode, value, m_lock); }
        }

        public string City
        {
            get
            {
                lock (m_lock)
                {
                    return m_city;
                }
            }

            set { SetProperty(ref m_city, value, m_lock); }
        }

        public CoutryRegion State
        {
            get
            {
                lock (m_lock)
                {
                    return m_state;
                }
            }

            set { SetProperty(ref m_state, value, m_lock); }
        }

        public Country Country
        {
            get
            {
                lock (m_lock)
                {
                    return m_country;
                }
            }

            set
            {
                lock (m_lock)
                {
                    if (SetProperty(ref m_country, value, null, false))
                    {
                        if (!m_country.States.Contains(m_state))
                        {
                            m_state = m_country.States.FirstOrDefault();
                        }
                    }
                }

                OnPropertyChanged();
                OnPropertyChanged("State");
            }
        }

        public string PhoneNumber
        {
            get
            {
                lock (m_lock)
                {
                    return m_phoneNumber;
                }
            }

            set { SetProperty(ref m_phoneNumber, value, m_lock); }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (m_country?.States != null)
            {
                return $"{FirstName ?? ""}, {SecondName ?? ""}, {StreetAddress1 ?? ""}, {StreetAddressLine2 ?? ""}, {PostalCode ?? ""}, {City ?? ""}, {State?.Abbreviation ?? ""}, {Country?.Name ?? ""}, {PhoneNumber ?? ""}";
            }

            return $"{FirstName ?? ""}, {SecondName ?? ""}, {StreetAddress1 ?? ""}, {StreetAddressLine2 ?? ""}, {PostalCode ?? ""}, {City ?? ""}, {Country?.Name ?? ""}, {PhoneNumber ?? ""}";
        }

        #endregion

        #region Fields

        private string m_firstName = null;
        private string m_secondName = null;
        private string m_streetAddress1 = null;
        private string m_streetAddressLine2 = null;
        private string m_postalCode = null;
        private string m_city = null;
        private CoutryRegion m_state = null;
        private Country m_country = null;
        private string m_phoneNumber = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

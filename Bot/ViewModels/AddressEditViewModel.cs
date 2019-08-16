using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    public class AddressEditViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public Address Model
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
                    if (Country == null)
                    {
                        Country = CountriesCollection.Countries[0];
                        State = Country.States.FirstOrDefault();
                    }

                    OnPropertyChanged("FirstName");
                    OnPropertyChanged("SecondName");
                    OnPropertyChanged("StreetAddress1");
                    OnPropertyChanged("StreetAddressLine2");
                    OnPropertyChanged("PostalCode");
                    OnPropertyChanged("City");
                    OnPropertyChanged("State");
                    OnPropertyChanged("Country");
                    OnPropertyChanged("PhoneNumber");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string FirstName
        {
            get { return m_model.FirstName; }

            set { m_model.FirstName = value; }
        }

        public string SecondName
        {
            get { return m_model.SecondName; }

            set { m_model.SecondName = value; }
        }

        public string StreetAddress1
        {
            get { return m_model.StreetAddress1; }

            set { m_model.StreetAddress1 = value; }
        }

        public string StreetAddressLine2
        {
            get { return m_model.StreetAddressLine2; }

            set { m_model.StreetAddressLine2 = value; }
        }

        public string PostalCode
        {
            get { return m_model.PostalCode; }

            set { m_model.PostalCode = value; }
        }

        public string City
        {
            get { return m_model.City; }

            set { m_model.City = value; }
        }

        public CoutryRegion State
        {
            get { return m_model.State; }

            set { m_model.State = value; }
        }

        public Country Country
        {
            get { return m_model.Country; }

            set { m_model.Country = value; }
        }

        public string PhoneNumber
        {
            get { return m_model.PhoneNumber; }

            set { m_model.PhoneNumber = value; }
        }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "State")
            {
                OnPropertyChanged("State");
            }
        }

        #endregion

        #region Fields

        private Address m_model = null;

        #endregion
    }
}

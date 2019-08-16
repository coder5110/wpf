using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Helpers;
using Bot.Models;

namespace Bot.ViewModels
{
    public class CheckoutProfileRecordViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public CheckoutProfile Model
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
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Email");
                    OnPropertyChanged("BillingAddress");
                    OnPropertyChanged("ShippingAddress");
                    OnPropertyChanged("PayCard");
                    OnPropertyChanged("BuyLimit");
                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string Name => m_model.Name;
        public string Email => m_model.Email;
        public string BillingAddress => m_model.BillingAddress.ToString();
        public string ShippingAddress => m_model.IsShippingAsBilling ? "Same as billing" : m_model.ShippingAddress?.ToString();

        public string PayCard
        {
            get
            {
                string payCardLast4numbers = "";
                int startIndex = 0;

                if (m_model.PayCard != null)
                {
                     startIndex = m_model.PayCard.Number.Length > 3 ? m_model.PayCard.Number.Length - 4 : 0;;
                }

                return $"{m_model.PayCard.Type.Name}({m_model.PayCard.Number.Substring(startIndex)})";
            }
        }

        public int? BuyLimit => m_model.BuyLimit;

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name")
            {
                OnPropertyChanged("Name");
            }
            else if (args.PropertyName == "Email")
            {
                OnPropertyChanged("Email");
            }
            else if (args.PropertyName == "BillingAddress")
            {
                OnPropertyChanged("BillingAddress");
            }
            else if (args.PropertyName == "ShippingAddress")
            {
                OnPropertyChanged("ShippingAddress");
            }
            else if (args.PropertyName == "PayCard")
            {
                OnPropertyChanged("PayCard");
            }
            else if (args.PropertyName == "IsShippingAsBilling")
            {
                OnPropertyChanged("ShippingAddress");
            }
            else if (args.PropertyName == "BuyLimit")
            {
                OnPropertyChanged("BuyLimit");
            }
        }

        #endregion

        #region Fields

        private CheckoutProfile m_model = null;

        #endregion
    }
}

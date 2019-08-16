using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bot.Models;
using Bot.Views;

namespace Bot.ViewModels
{
    public class CheckoutProfileEditViewModel: ViewModelBase
    {
        #region Constructors

        public CheckoutProfileEditViewModel()
        {
            EditBillingAddress = new DelegateCommand(parameter =>
            {
                Address address = BillingAddress != null ? new Address(BillingAddress) : new Address();

                AddressEditor editor = new AddressEditor()
                {
                    DataContext = new AddressEditViewModel()
                    {
                        Model = address
                    }
                };

                if (editor.ShowDialog() ?? false)
                {
                    BillingAddress = address;
                }
            });

            EditShippingAddress = new DelegateCommand(parameter =>
            {
                Address address = ShippingAddress != null ? new Address(ShippingAddress) : new Address();

                AddressEditor editor = new AddressEditor()
                {
                    DataContext = new AddressEditViewModel()
                    {
                        Model = address
                    }
                };

                if (editor.ShowDialog() ?? false)
                {
                    ShippingAddress = address;
                }
            });

            EditPayCard = new DelegateCommand(parameter =>
            {
                PayCard card = PayCard != null ? new PayCard(PayCard) : new PayCard()
                {
                    Type = PayCardTypeCollection.Types.ElementAt(0)
                };

                PayCardEditor editor = new PayCardEditor()
                {
                    DataContext = new PayCardEditViewModel()
                    {
                        Model = card
                    }
                };

                if (editor.ShowDialog() ?? false)
                {
                    PayCard = card;
                }
            });
        }

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

                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string Name
        {
            get { return m_model.Name; }

            set { m_model.Name = value; }
        }

        public string Email
        {
            get { return m_model.Email; }

            set { m_model.Email = value; }
        }

        public Address BillingAddress
        {
            get { return m_model.BillingAddress; }

            set { m_model.BillingAddress = value; }
        }

        public Address ShippingAddress
        {
            get { return m_model.ShippingAddress; }

            set { m_model.ShippingAddress = value; }
        }

        public bool IsShippingAsBilling
        {
            get { return m_model.IsShippingAsBilling; }

            set { m_model.IsShippingAsBilling = value; }
        }

        public PayCard PayCard
        {
            get { return m_model.PayCard; }

            set { m_model.PayCard = value; }
        }

        public int? BuyLimit
        {
            get { return m_model.BuyLimit; }

            set { m_model.BuyLimit = value; }
        }

        #endregion

        #region Commands

        public ICommand EditBillingAddress { get; protected set; }
        public ICommand EditShippingAddress { get; protected set; }
        public ICommand EditPayCard { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "ShippingAddress")
            {
                OnPropertyChanged("ShippingAddress");
            }
            else if (args.PropertyName == "BillingAddress")
            {
                OnPropertyChanged("BillingAddress");
            }
            else if (args.PropertyName == "IsShippingAsBilling")
            {
                OnPropertyChanged("IsShippingAsBilling");
            }
            else if (args.PropertyName == "PayCard")
            {
                OnPropertyChanged("PayCard");
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

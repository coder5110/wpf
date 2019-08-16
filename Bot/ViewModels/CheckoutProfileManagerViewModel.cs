using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bot.Models;
using Bot.Views;
using Microsoft.Win32;

namespace Bot.ViewModels
{
    public class CheckoutProfileManagerViewModel: ViewModelBase
    {
        #region Constructors

        public CheckoutProfileManagerViewModel()
        {
            AddNew = new DelegateCommand(parameter =>
            {
                CheckoutProfileEditor editor = null;
                CheckoutProfile profile = new CheckoutProfile();

                while (true)
                {
                    editor = new CheckoutProfileEditor()
                    {
                        DataContext = new CheckoutProfileEditViewModel()
                        {
                            Model = profile
                        }
                    };

                    if (editor.ShowDialog() ?? false)
                    {
                        if (m_model.Where(p => p.Name == profile.Name).ToList().Count == 0)
                        {
                            m_model.Add(profile);

                            break;
                        }

                        MessageBox.Show($"Checkout profile name {profile.Name} is busy. Choose another name, please.", "Duplication", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        break;
                    }
                }
            });

            Edit = new DelegateCommand(parameter =>
                {
                    CheckoutProfile originalProfile = SelectedProfiles[0].Model;
                    CheckoutProfile profile = new CheckoutProfile(originalProfile);

                    while (true)
                    {
                        CheckoutProfileEditor editor = new CheckoutProfileEditor()
                        {
                            DataContext = new CheckoutProfileEditViewModel()
                            {
                                Model = profile
                            }
                        };

                        if (editor.ShowDialog() ?? false)
                        {
                            List<CheckoutProfile> dupList = m_model.Where(p => p.Name == profile.Name).ToList();

                            if (dupList.Count == 0 || dupList[0] == originalProfile)
                            {
                                profile.CopyTo(originalProfile);
                                break;
                            }

                            MessageBox.Show(
                                $"Checkout profile name {profile.Name} is busy. Choose another name, please.",
                                "Duplication", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            break;
                        }
                    }
                },
            parameter => SelectedProfiles.Count == 1);

            Duplicate = new DelegateCommand(parameter =>
            {
                CheckoutProfileEditor editor = null;
                CheckoutProfile profile = new CheckoutProfile(SelectedProfiles[0].Model);

                while (true)
                {
                    editor = new CheckoutProfileEditor()
                    {
                        DataContext = new CheckoutProfileEditViewModel()
                        {
                            Model = profile
                        }
                    };

                    if (editor.ShowDialog() ?? false)
                    {
                        if (m_model.Where(p => p.Name == profile.Name).ToList().Count == 0)
                        {
                            m_model.Add(profile);

                            break;
                        }

                        MessageBox.Show($"Checkout profile name {profile.Name} is busy. Choose another name, please.", "Duplication", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        break;
                    }
                }
            },
            parameter => SelectedProfiles.Count == 1);

            Delete = new DelegateCommand(parameter =>
            {
                foreach (CheckoutProfileRecordViewModel profile in SelectedProfiles.ToList())
                {
                    m_model.Remove(profile.Model);
                }
            },
            parameter => SelectedProfiles.Count > 0);

            Import = new DelegateCommand(parameter =>
            {
                OpenFileDialog openFIleDialog = new OpenFileDialog()
                {
                    DefaultExt = ".csv",
                    Filter = "CSV files (.csv)|*.csv"
                };

                if (openFIleDialog.ShowDialog() ?? false)
                {
                    using (StreamReader reader = new StreamReader(openFIleDialog.FileName))
                    {
                        int lineCount = 0;
                        bool isOk = true;
                        while (!reader.EndOfStream)
                        {
                            string[] profileParameters = reader.ReadLine().Split(new char[] { ';' });

                            CheckoutProfile profile = null;

                            try
                            {
                                profile = new CheckoutProfile()
                                {
                                    Name = profileParameters[0],
                                    Email = profileParameters[1],
                                    BillingAddress = new Address()
                                    {
                                        FirstName = profileParameters[2],
                                        SecondName = profileParameters[3],
                                        StreetAddress1 = profileParameters[4],
                                        StreetAddressLine2 = profileParameters[5],
                                        PostalCode = profileParameters[6],
                                        City = profileParameters[7],
                                        State = CountriesCollection.Countries[(CountryCode)int.Parse(profileParameters[9])].States.ElementAtOrDefault(int.Parse(profileParameters[8])),
                                        Country = CountriesCollection.Countries[(CountryCode)int.Parse(profileParameters[9])],
                                        PhoneNumber = profileParameters[25]
                                    },
                                    ShippingAddress = new Address()
                                    {
                                        FirstName = profileParameters[10],
                                        SecondName = profileParameters[11],
                                        StreetAddress1 = profileParameters[12],
                                        StreetAddressLine2 = profileParameters[13],
                                        PostalCode = profileParameters[14],
                                        City = profileParameters[15],
                                        State = CountriesCollection.Countries[(CountryCode)int.Parse(profileParameters[17])].States.ElementAtOrDefault(int.Parse(profileParameters[16])),
                                        Country = CountriesCollection.Countries[(CountryCode)int.Parse(profileParameters[17])],
                                        PhoneNumber = profileParameters[26]
                                    },
                                    IsShippingAsBilling = bool.Parse(profileParameters[18]),
                                    PayCard = new PayCard()
                                    {
                                        Number = profileParameters[19],
                                        Holder = profileParameters[20],
                                        ExpirationDate = DateTime.ParseExact(profileParameters[21], "O",
                                            CultureInfo.InvariantCulture),
                                        CVS = profileParameters[22],
                                        Type = PayCardTypeCollection.TypesDictionary[(PayCardTypeCode)int.Parse(profileParameters[23])]
                                    },
                                    BuyLimit = int.Parse(profileParameters[24])
                                };
                            }
                            catch (Exception e)
                            {
                                isOk = false;
                                break;
                            }

                            if (m_model.Where(p => p.Name == profile.Name).ToList().Count == 0)
                            {
                                m_model.Add(profile);
                            }

                            lineCount++;
                        }

                        if (!isOk)
                        {
                            MessageBox.Show($"Wrong format at line {lineCount + 1}");
                        }

                        reader.Close();
                    }
                }
            });

            Export = new DelegateCommand(parameter =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    DefaultExt = ".csv",
                    Filter = "CSV files (.csv)|*.csv"
                };

                if (saveFileDialog.ShowDialog() ?? false)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        foreach (CheckoutProfileRecordViewModel profileVM in Profiles)
                        {
                            CheckoutProfile profile = profileVM.Model;
                            writer.WriteLine($"{profile.Name};{profile.Email};{profile.BillingAddress.FirstName};" +
                                             $"{profile.BillingAddress.SecondName};{profile.BillingAddress.StreetAddress1};" +
                                             $"{profile.BillingAddress.StreetAddressLine2};{profile.BillingAddress.PostalCode};" +
                                             $"{profile.BillingAddress.City};{profile.BillingAddress.Country.States.IndexOf(profile.BillingAddress.State)};" +
                                             $"{(int)profile.BillingAddress.Country.Code};" +
                                             $"{profile.ShippingAddress.FirstName};" +
                                             $"{profile.ShippingAddress.SecondName};{profile.ShippingAddress.StreetAddress1};" +
                                             $"{profile.ShippingAddress.StreetAddressLine2};{profile.ShippingAddress.PostalCode};" +
                                             $"{profile.ShippingAddress.City};{profile.ShippingAddress.Country.States.IndexOf(profile.ShippingAddress.State)};" +
                                             $"{(int)profile.ShippingAddress.Country.Code};" +
                                             $"{profile.IsShippingAsBilling};{profile.PayCard.Number};{profile.PayCard.Holder};" +
                                             $"{profile.PayCard.ExpirationDate.Value.ToString("O")};{profile.PayCard.CVS};{(int)PayCardTypeCollection.TypesDictionary.First(p => p.Value == profile.PayCard.Type).Key};" +
                                             $"{profile.BuyLimit};{profile.BillingAddress.PhoneNumber};{profile.ShippingAddress.PhoneNumber};");
                        }

                        writer.Close();
                    }
                }
            },
            parameter => Profiles.Count > 0);
        }

        #endregion

        #region Properties

        public ObservableCollection<CheckoutProfile> Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    m_model.CollectionChanged -= NotifyCollectionChangedHandler;
                }

                if (SetProperty(ref m_model, value))
                {
                    Update();
                }

                if (m_model != null)
                {
                    m_model.CollectionChanged += NotifyCollectionChangedHandler;
                }
            }
        }

        public ObservableCollection<CheckoutProfileRecordViewModel> Profiles { get; } = new ObservableCollection<CheckoutProfileRecordViewModel>();
        public ObservableCollection<CheckoutProfileRecordViewModel> SelectedProfiles { get; } = new ObservableCollection<CheckoutProfileRecordViewModel>();

        #endregion

        #region Commands

        public ICommand AddNew { get; protected set; }
        public ICommand Edit { get; protected set; }
        public ICommand Duplicate { get; protected set; }
        public ICommand Delete { get; protected set; }
        public ICommand Import { get; protected set; }
        public ICommand Export { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            Profiles.Clear();

            foreach (CheckoutProfile profile in m_model)
            {
                Profiles.Add(new CheckoutProfileRecordViewModel()
                {
                    Model = profile
                });
            }
        }

        private void NotifyCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CheckoutProfile profile in args.NewItems)
                {
                    Profiles.Add(new CheckoutProfileRecordViewModel()
                    {
                        Model = profile
                    });
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CheckoutProfile profile in args.OldItems)
                {
                    Profiles.Remove(Profiles.First(p => p.Model == profile));
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Replace)
            {
                Profiles.First(p => p.Model == args.OldItems[0]).Model = args.NewItems[0] as CheckoutProfile;
            }
        }

        #endregion

        #region Fields

        private ObservableCollection<CheckoutProfile> m_model = null;

        #endregion
    }
}

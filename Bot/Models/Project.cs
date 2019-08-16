using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Models
{
    public class Project: BindableObject
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

        public string FileName
        {
            get
            {
                lock (m_lock)
                {
                    return m_fileName;
                }
            }

            set { SetProperty(ref m_fileName, value, m_lock); }
        }

        public ObservableCollection<CheckoutProfile> CheckoutProfiles
        {
            get
            {
                lock (m_lock)
                {
                    return m_checkoutProfiles;
                }
            }
        }

        public ObservableCollection<Release> Releases
        {
            get
            {
                lock (m_lock)
                {
                    return m_releases;
                }
            }
        }

        public bool IsHasFile
        {
            get
            {
                lock (m_lock)
                {
                    return m_fileName != null;
                }
            }
        }

        #endregion

        #region Methods

        public void SaveToFile(string fileName)
        {
            JObject project = new JObject();

            FileName = fileName;
            Name = new FileInfo(fileName).Name;

            project[PropertyHelper.GetPropertyName(() => Name)] = Name;

            JArray checkoutProfiles = new JArray();

            foreach (CheckoutProfile profile in m_checkoutProfiles)
            {
                JObject p = new JObject();
                p[PropertyHelper.GetPropertyName(() => profile.Name)] = profile.Name;
                p[PropertyHelper.GetPropertyName(() => profile.Email)] = profile.Email;

                JObject billingAddress = new JObject();
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.FirstName)] = profile.BillingAddress.FirstName;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.SecondName)] = profile.BillingAddress.SecondName;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.StreetAddress1)] = profile.BillingAddress.StreetAddress1;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.StreetAddressLine2)] = profile.BillingAddress.StreetAddressLine2;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.PostalCode)] = profile.BillingAddress.PostalCode;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.City)] = profile.BillingAddress.City;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.State)] = profile.BillingAddress.Country.States.IndexOf(profile.BillingAddress.State);
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.Country)] = (int)profile.BillingAddress.Country.Code;
                billingAddress[PropertyHelper.GetPropertyName(() => profile.BillingAddress.PhoneNumber)] = profile.BillingAddress.PhoneNumber;

                p[PropertyHelper.GetPropertyName(() => profile.BillingAddress)] = billingAddress;

                JObject shippingAddress = new JObject();
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.FirstName)] = profile.ShippingAddress.FirstName;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.SecondName)] = profile.ShippingAddress.SecondName;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.StreetAddress1)] = profile.ShippingAddress.StreetAddress1;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.StreetAddressLine2)] = profile.ShippingAddress.StreetAddressLine2;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.PostalCode)] = profile.ShippingAddress.PostalCode;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.City)] = profile.ShippingAddress.City;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.State)] = profile.ShippingAddress.Country.States.IndexOf(profile.ShippingAddress.State);
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.Country)] = (int)profile.ShippingAddress.Country.Code;
                shippingAddress[PropertyHelper.GetPropertyName(() => profile.ShippingAddress.PhoneNumber)] = profile.ShippingAddress.PhoneNumber;

                p[PropertyHelper.GetPropertyName(() => profile.ShippingAddress)] = shippingAddress;
                p[PropertyHelper.GetPropertyName(() => profile.IsShippingAsBilling)] = profile.IsShippingAsBilling;

                JObject payCard = new JObject();

                payCard[PropertyHelper.GetPropertyName(() => profile.PayCard.Number)] = profile.PayCard.Number;
                payCard[PropertyHelper.GetPropertyName(() => profile.PayCard.Holder)] = profile.PayCard.Holder;
                payCard[PropertyHelper.GetPropertyName(() => profile.PayCard.ExpirationDate)] = profile.PayCard.ExpirationDate.Value;
                payCard[PropertyHelper.GetPropertyName(() => profile.PayCard.CVS)] = profile.PayCard.CVS;
                payCard[PropertyHelper.GetPropertyName(() => profile.PayCard.Type)] = (int)PayCardTypeCollection.TypesDictionary.First(t => t.Value == profile.PayCard.Type).Key;

                p[PropertyHelper.GetPropertyName(() => profile.PayCard)] = payCard;
                p[PropertyHelper.GetPropertyName(() => profile.BuyLimit)] = profile.BuyLimit;

                checkoutProfiles.Add(p);
            }

            project[PropertyHelper.GetPropertyName(() => CheckoutProfiles)] = checkoutProfiles;

            JArray releases = new JArray();

            foreach (Release release in m_releases)
            {
                JObject r = new JObject();

                r[PropertyHelper.GetPropertyName(() => release.Name)] = release.Name;
                r[PropertyHelper.GetPropertyName(() => release.Footsite)] = (int)FootsiteCollection.SitesDictionary.First(p => p.Value == release.Footsite).Key;
                r[PropertyHelper.GetPropertyName(() => release.ProductLink)] = release.ProductLink;
                r[PropertyHelper.GetPropertyName(() => release.Category)] = release.Category;
                r[PropertyHelper.GetPropertyName(() => release.KeyWords)] = JsonConvert.SerializeObject(release.KeyWords);
                r[PropertyHelper.GetPropertyName(() => release.Style)] = release.Style;
                r[PropertyHelper.GetPropertyName(() => release.GlobalProxy)] = release.GlobalProxy?.ToString();

                //if (release.CaptchaHarvester != null)
                //{
                //    r[PropertyHelper.GetPropertyName(() => release.CaptchaHarvester)] = new JObject();
                //    r[PropertyHelper.GetPropertyName(() => release.CaptchaHarvester)][
                //            PropertyHelper.GetPropertyName(() => release.CaptchaHarvester.TwoCaptchaApiKey)] =
                //        release.CaptchaHarvester.TwoCaptchaApiKey;
                //}

                JArray releaseCheckoutProfiles = new JArray();

                foreach (ReleaseCheckoutProfile profile in release.Profiles)
                {
                    JObject p = new JObject();

                    p[PropertyHelper.GetPropertyName(() => profile.CheckoutProfile)] = m_checkoutProfiles.IndexOf(profile.CheckoutProfile);
                    p[PropertyHelper.GetPropertyName(() => profile.Size)] = profile.Size;
                    p[PropertyHelper.GetPropertyName(() => profile.TasksCount)] = profile.TasksCount;
                    p[PropertyHelper.GetPropertyName(() => profile.NotificationEmail)] = profile.NotificationEmail;
                    p[PropertyHelper.GetPropertyName(() => profile.ClothesSizeSystem)] = (int)profile.ClothesSizeSystem.Code;
                    p[PropertyHelper.GetPropertyName(() => profile.VariantId)] = profile.VariantId;

                    releaseCheckoutProfiles.Add(p);
                }

                r[PropertyHelper.GetPropertyName(() => release.Profiles)] = releaseCheckoutProfiles;

                JArray proxies = new JArray();

                foreach (Proxy proxy in release.Proxies)
                {
                    proxies.Add(proxy.ToString());
                }

                r[PropertyHelper.GetPropertyName(() => release.Proxies)] = proxies;

                releases.Add(r);
            }

            project[PropertyHelper.GetPropertyName(() => Releases)] = releases;

            File.WriteAllText(fileName, project.ToString());
        }

        public static Project LoadFromFile(string fileName)
        {
            Project project = new Project();
            string projectJson = File.ReadAllText(fileName);

            JObject projecJObject = JObject.Parse(projectJson);

            project.Name = (string)projecJObject[PropertyHelper.GetPropertyName(() => project.Name)];
            project.FileName = fileName;

            foreach (JToken token in projecJObject[PropertyHelper.GetPropertyName(() => project.CheckoutProfiles)])
            {
                CheckoutProfile profile = new CheckoutProfile();

                profile.Name = (string)token[PropertyHelper.GetPropertyName(() => profile.Name)];
                profile.Email = (string)token[PropertyHelper.GetPropertyName(() => profile.Email)];
                profile.BuyLimit = (int)token[PropertyHelper.GetPropertyName(() => profile.BuyLimit)];

                JToken billingAddressToken = token[PropertyHelper.GetPropertyName(() => profile.BillingAddress)];

                Address billingAddress = new Address();
                billingAddress.FirstName = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.FirstName)];
                billingAddress.SecondName = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.SecondName)];
                billingAddress.StreetAddress1 = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.StreetAddress1)];
                billingAddress.StreetAddressLine2 = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.StreetAddressLine2)];
                billingAddress.PostalCode = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.PostalCode)];
                billingAddress.City = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.City)];
                billingAddress.Country = CountriesCollection.Countries[(CountryCode)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.Country)].Value<int>()];
                billingAddress.State = billingAddress.Country.States.ElementAtOrDefault((int)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.State)]);
                billingAddress.PhoneNumber = (string)billingAddressToken[PropertyHelper.GetPropertyName(() => billingAddress.PhoneNumber)];

                profile.BillingAddress = billingAddress;

                JToken shippingAddressToken = token[PropertyHelper.GetPropertyName(() => profile.ShippingAddress)];

                Address shippingAddress = new Address();
                shippingAddress.FirstName = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.FirstName)];
                shippingAddress.SecondName = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.SecondName)];
                shippingAddress.StreetAddress1 = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.StreetAddress1)];
                shippingAddress.StreetAddressLine2 = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.StreetAddressLine2)];
                shippingAddress.PostalCode = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.PostalCode)];
                shippingAddress.City = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.City)];
                shippingAddress.Country = CountriesCollection.Countries[(CountryCode)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.Country)].Value<int>()];
                shippingAddress.State = shippingAddress.Country.States.ElementAtOrDefault((int)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.State)]);
                shippingAddress.PhoneNumber = (string)shippingAddressToken[PropertyHelper.GetPropertyName(() => shippingAddress.PhoneNumber)];

                profile.ShippingAddress = shippingAddress;
                profile.IsShippingAsBilling = (bool)token[PropertyHelper.GetPropertyName(() => profile.IsShippingAsBilling)];

                JToken payCardToken = token[PropertyHelper.GetPropertyName(() => profile.PayCard)];

                PayCard payCard = new PayCard();
                payCard.Number = (string)payCardToken[PropertyHelper.GetPropertyName(() => payCard.Number)];
                payCard.Holder = (string)payCardToken[PropertyHelper.GetPropertyName(() => payCard.Holder)];
                payCard.ExpirationDate = payCardToken[PropertyHelper.GetPropertyName(() => payCard.ExpirationDate)].ToObject<DateTime>();
                payCard.CVS = (string)payCardToken[PropertyHelper.GetPropertyName(() => payCard.CVS)];
                payCard.Type = PayCardTypeCollection.TypesDictionary[(PayCardTypeCode)payCardToken[PropertyHelper.GetPropertyName(() => payCard.Type)].ToObject<int>()];

                profile.PayCard = payCard;

                project.CheckoutProfiles.Add(profile);
            }

            foreach (JToken token in projecJObject[PropertyHelper.GetPropertyName(() => project.Releases)])
            {
                Release release = new Release();

                release.Name = (string)token[PropertyHelper.GetPropertyName(() => release.Name)];
                release.Footsite = FootsiteCollection.SitesDictionary[(FootsiteType)token[PropertyHelper.GetPropertyName(() => release.Footsite)].ToObject<int>()];
                release.ProductLink = (string)token[PropertyHelper.GetPropertyName(() => release.ProductLink)];
                release.Category = (string)token[PropertyHelper.GetPropertyName(() => release.Category)];

                foreach (string keyword in JsonConvert.DeserializeObject<List<string>>(token[PropertyHelper.GetPropertyName(() => release.KeyWords)].ToString()))
                {
                    release.KeyWords.Add(keyword);
                }

                release.Style = (string)token[PropertyHelper.GetPropertyName(() => release.Style)];
                string globalProxyStr = (string) token[PropertyHelper.GetPropertyName(() => release.GlobalProxy)];
                release.GlobalProxy = globalProxyStr != null ? new Proxy(globalProxyStr) : null;

                //try
                //{
                //    if (release.CaptchaHarvester != null)
                //    {
                //        release.CaptchaHarvester.TwoCaptchaApiKey =
                //            token[PropertyHelper.GetPropertyName(() => release.CaptchaHarvester)][
                //                    PropertyHelper.GetPropertyName(() => release.CaptchaHarvester.TwoCaptchaApiKey)]
                //                .Value<string>();
                //    }
                //}
                //catch (Exception e)
                //{
                    
                //}

                foreach (JToken profileJToken in token[PropertyHelper.GetPropertyName(() => release.Profiles)])
                {
                    ReleaseCheckoutProfile profile = new ReleaseCheckoutProfile();

                    profile.CheckoutProfile = project.CheckoutProfiles[(int)profileJToken[PropertyHelper.GetPropertyName(() => profile.CheckoutProfile)]];
                    profile.TasksCount = (int)profileJToken[PropertyHelper.GetPropertyName(() => profile.TasksCount)]; ;
                    profile.NotificationEmail = (string)profileJToken[PropertyHelper.GetPropertyName(() => profile.NotificationEmail)];
                    profile.ClothesSizeSystem = ClothesSizeSystemCollection.SystemsDictionary[(ClothesSizeSystemCode)profileJToken[PropertyHelper.GetPropertyName(() => profile.ClothesSizeSystem)].ToObject<int>()];
                    profile.Size = (string)profileJToken[PropertyHelper.GetPropertyName(() => profile.Size)];
                    profile.VariantId = (string)profileJToken[PropertyHelper.GetPropertyName(() => profile.VariantId)];

                    release.Profiles.Add(profile);
                }

                foreach (JToken proxyJToken in token[PropertyHelper.GetPropertyName(() => release.Proxies)])
                {
                    Proxy proxy = new Proxy(proxyJToken.Value<string>());

                    release.Proxies.Add(proxy);
                }

                project.Releases.Add(release);
            }

            return project;
        }

        #endregion

        #region Fields

        private string m_name = "Unnamed";
        private string m_fileName = null;
        private readonly ObservableCollection<CheckoutProfile> m_checkoutProfiles = new ObservableCollection<CheckoutProfile>();
        private readonly ObservableCollection<Release> m_releases = new ObservableCollection<Release>();
        private readonly object m_lock = new object();

        #endregion
    }
}

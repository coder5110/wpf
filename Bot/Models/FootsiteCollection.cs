using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Bot.Services;

namespace Bot.Models
{
    public static class FootsiteCollection
    {
        #region Constructors



        #endregion

        #region Properties

        public static Dictionary<FootsiteType, Footsite>.ValueCollection Sites => SitesDictionary.Values;

        public static readonly Dictionary<FootsiteType, Footsite> SitesDictionary = new Dictionary<FootsiteType, Footsite>()
        {
            { FootsiteType.Footlocker, new Footsite()
                {
                    Type = FootsiteType.Footlocker,
                    Name = "Footlocker",
                    BaseUrl = @"https://www.footlocker.com",
                    Domain = @"www.footlocker.com",
                    ImageSource = "/Resources/Images/footlocker_logo.png",
                    Settings = new FootsiteSettings()
                    {
                        ProductMonitorPeriod = 3000,
                        PurchaseLimitPerProfile = 1,
                        RetryPeriod = 2000,
                        DelayInCheckout = 1000
                    },
                    Captcha = new CaptchaDescription()
                    {
                        Code = CaptchaType.Undefined
                    },
                    SupportedCountries = { CountryCode.US }
                }
            },
            { FootsiteType.SupremeUSA, new Footsite()
                {
                    Type = FootsiteType.SupremeUSA,
                    Name = "Supreme USA",
                    BaseUrl = "http://www.supremenewyork.com",
                    Domain = @"www.supremenewyork.com",
                    ImageSource = "/Resources/Images/supreme_usa_logo.png",
                    Settings = new FootsiteSettings()
                    {
                        ProductMonitorPeriod = 3000,
                        PurchaseLimitPerProfile = 1,
                        RetryPeriod = 2000,
                        DelayInCheckout = 1000
                    },
                    Captcha = new CaptchaDescription()
                    {
                        Code = CaptchaType.ReCaptchaV2,
                        DefaultSettings = new RecaptchaSettings()
                        {
                            SiteKey = "6LeWwRkUAAAAAOBsau7KpuC9AV-6J8mhw4AjC3Xz",
                            SitePath = "http://www.supremenewyork.com/checkout"
                        }
                    },
                    SupportedCountries = { CountryCode.US, CountryCode.CA }
                }
            },
            { FootsiteType.SupremeEurope, new Footsite()
                {
                    Type = FootsiteType.SupremeEurope,
                    Name = "Supreme Europe",
                    BaseUrl = "http://www.supremenewyork.com",
                    Domain = @"www.supremenewyork.com",
                    ImageSource = "/Resources/Images/supreme_eu_logo.png",
                    Settings = new FootsiteSettings()
                    {
                        ProductMonitorPeriod = 3000,
                        PurchaseLimitPerProfile = 1,
                        RetryPeriod = 2000,
                        DelayInCheckout = 1000
                    },
                    Captcha = new CaptchaDescription()
                    {
                        Code = CaptchaType.ReCaptchaV2,
                        DefaultSettings = new RecaptchaSettings()
                        {
                            SiteKey = "6LeWwRkUAAAAAOBsau7KpuC9AV-6J8mhw4AjC3Xz",
                            SitePath = "http://www.supremenewyork.com/checkout"
                        }
                    },
                    SupportedCountries = { CountryCode.US, CountryCode.CA }
                }
            },
            { FootsiteType.SupremeJapan, new Footsite()
                {
                    Type = FootsiteType.SupremeJapan,
                    Name = "Supreme Japan",
                    BaseUrl = "http://www.supremenewyork.com",
                    Domain = @"www.supremenewyork.com",
                    ImageSource = "/Resources/Images/supreme_japan_logo.png",
                    Settings = new FootsiteSettings()
                    {
                        ProductMonitorPeriod = 3000,
                        PurchaseLimitPerProfile = 1,
                        RetryPeriod = 2000,
                        DelayInCheckout = 1000
                    },
                    Captcha = new CaptchaDescription()
                    {
                        Code = CaptchaType.ReCaptchaV2,
                        DefaultSettings = new RecaptchaSettings()
                        {
                            SiteKey = "6LeWwRkUAAAAAOBsau7KpuC9AV-6J8mhw4AjC3Xz",
                            SitePath = "http://www.supremenewyork.com/checkout"
                        }
                    },
                    SupportedCountries = { CountryCode.JP }
                }
            }
            //{ FootsiteType.Yeezysupply,  new Footsite()
            //    {
            //        Type = FootsiteType.Yeezysupply,
            //        Name = "Yeezysupply",
            //        BaseUrl = new Url("https://yeezysupply.com/"),
            //        ImageSource = "/Resources/Images/yeezysupply_logo.png",
            //        Settings = new FootsiteSettings()
            //        {
            //            ProductMonitorPeriod = 3000,
            //            PurchaseLimitPerProfile = 1,
            //            RetryPeriod = 2000,
            //        }
            //    }
            //}
        };

        #endregion

        #region Fields

        #endregion
    }
}

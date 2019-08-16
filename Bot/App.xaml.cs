using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AutoUpdaterDotNET;
using Bot.Helpers;
using Bot.Models;
using Bot.ViewModels;
using Bot.Views;
using CefSharp;
using HtmlAgilityPack;
using Cookie = CefSharp.Cookie;

namespace Bot
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (m_mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                HtmlNode.ElementsFlags.Remove("form");
                HtmlNode.ElementsFlags.Remove("option");
                System.Net.ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                LicenseManagerViewModel licenseManagerViewModel = new LicenseManagerViewModel()
                {
                    Model = LicenseInfo.GetForCurrentDevice()
                };

                LicenseChecker licenseChecker = new LicenseChecker()
                {
                    DataContext = licenseManagerViewModel
                };

                licenseChecker.ShowDialog();

                if (!(licenseManagerViewModel.IsLicenseValid ?? false) && licenseManagerViewModel.IsCheckingFinished &&
                    licenseManagerViewModel.IsLicenseServerReachable)
                {
                    LicenseManager lm = new LicenseManager()
                    {
                        DataContext = licenseManagerViewModel
                    };

                    lm.ShowDialog();
                }

                if (licenseManagerViewModel.IsLicenseValid ?? false)
                {
                    using (StreamWriter writer = new StreamWriter("license.key"))
                    {
                        writer.WriteLine(licenseManagerViewModel.Model.Key);
                    }

                    string tierStr = licenseManagerViewModel.LicenseKey.Substring(14, 1);

                    Tier? tier = null;

                    if (tierStr == "N") tier = Tier.Novice;
                    else if (tierStr == "S") tier = Tier.Standart;
                    else if (tierStr == "U") tier = Tier.Ultimate;

                    if (tier != null)
                    {
                        TierControl.Init((Tier) tier);

                        AutoUpdater.AppTitle = "Project Destroyer";
                        AutoUpdater.Start("https://projectdestroyer.com/update/ui.xml");
                        
                        CefSharp.Cef.Initialize();

                        Cef.GetGlobalCookieManager()
                            .SetCookie("https://www.google.com", new Cookie()
                            {
                                Name = "NID",
                                Value =
                                    "114=VmK5Yof_gASwTsA4f9mDhik59gO5ivFGNdwRFZ5eqb13IrO4y_oc7cqKOjD4d8VTeHqlSFtXecyGVpHTHCzi3H56q67Wic82SSme36ed0eSFnLAbZ7TzEMyMT83YvPTXDSB_UMNus_iOrgmpz2h1UHE5qMHImEeKqTNfJ-ufhJs",
                                Domain = ".google.com",
                                Secure = true,
                                Expires = DateTime.Now.AddDays(364),
                                Path = "/"
                            });

                        Cef.GetGlobalCookieManager()
                            .SetCookie("https://www.google.com", new Cookie()
                            {
                                Name = "SID",
                                Value =
                                    "RQUZKqa2lvg8bV2MOpudssKUIM1m4ePTnSHRw8vJdLi5s6OqsMm-nauRbe30Pvd6E04m4g.",
                                Domain = ".google.com",
                                Secure = true,
                                Expires = DateTime.Now.AddDays(364),
                                Path = "/"
                            });

                        Cef.GetGlobalCookieManager()
                            .SetCookie("https://www.google.com", new Cookie()
                            {
                                Name = "HSID",
                                Value =
                                    "AmU2jxCL2p_HH0ZPd",
                                Domain = ".google.com",
                                Secure = true,
                                Expires = DateTime.Now.AddDays(364),
                                Path = "/"
                            });

                        Cef.GetGlobalCookieManager()
                            .SetCookie("https://www.google.com", new Cookie()
                            {
                                Name = "SSID",
                                Value =
                                    "A-zucxjV69YuQrG9R",
                                Domain = ".google.com",
                                Secure = true,
                                Expires = DateTime.Now.AddDays(364),
                                Path = "/"
                            });

                        Cef.GetGlobalCookieManager()
                            .SetCookie("https://www.google.com", new Cookie()
                            {
                                Name = "APISID",
                                Value =
                                    "kKGH9LEmsKlHE1z5/AHkUX3rAZaMPSmMZI",
                                Domain = ".google.com",
                                Secure = true,
                                Expires = DateTime.Now.AddDays(364),
                                Path = "/"
                            });

                        Cef.GetGlobalCookieManager()
                            .SetCookie("https://www.google.com", new Cookie()
                            {
                                Name = "SAPISID",
                                Value =
                                    "IbeC7mNDggiLVl1C/APfpkXK9im_Fx6R75",
                                Domain = ".google.com",
                                Secure = true,
                                Expires = DateTime.Now.AddDays(364),
                                Path = "/"
                            });

                        //var ccm = CefSharp.Cef.GetGlobalCookieManager();

                        //foreach (string line in str.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        //{
                        //    string[] fields = line.Split(new[] { '\t' });

                        //    CefSharp.Cookie cookie = new CefSharp.Cookie()
                        //    {
                        //        Domain = fields[0],
                        //        Path = fields[2],
                        //        Secure = bool.Parse(fields[3]),
                        //        Expires = TimeHelper.UnixTimeStampToDateTime(double.Parse(fields[4]) < uint.MaxValue
                        //            ? double.Parse(fields[4])
                        //            : int.MaxValue),
                        //        Name = fields[5],
                        //        Value = fields[6]
                        //    };

                        //    string url = cookie.Secure ? "https://" : "http://";
                        //    url = $"{url}{(cookie.Domain[0] != '.' ? cookie.Domain : cookie.Domain.Substring(1))}/";

                        //    bool ress = ccm.SetCookie(url, cookie);
                        //}

                        MainWindow appWindow = new MainWindow()
                        {
                            DataContext = new AppViewModel()
                        };
                        appWindow.ShowDialog();

                        CefSharp.Cef.Shutdown();
                    }
                    else
                    {
                        MessageBox.Show("Your tier does not exists. Contact with support.");
                    }
                }

                Application.Current?.Shutdown();

                m_mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Only one instance of Project Destroyer at a time");

                Application.Current?.Shutdown();
            }
        }

        #region Fields

        static Mutex m_mutex = new Mutex(true, "PDMutex");

        #endregion
    }
}

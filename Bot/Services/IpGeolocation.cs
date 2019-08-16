using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Models;
using Newtonsoft.Json.Linq;

namespace Bot.Services
{
    public static class IpGeolocation
    {
        #region Methods

        public static async Task<Country> GetLocation(Proxy proxy, CancellationToken cancelToken)
        {
            Country country = null;

            string html = null;

            using (HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy()
                {
                    Address = new Uri($"http://{proxy.IP}:{proxy.Port}"),
                    Credentials = new NetworkCredential()
                    {
                        UserName = proxy.Username,
                        Password = proxy.Password
                    }
                }
            })
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    html = await HttpHelper.GetString(new Uri("http://ip-api.com/json"), HttpMethod.Get, client,
                        cancelToken, 3, true);
                }
            }

            if (html != null)
            {
                string status = null;
                JObject responseJObject = null;

                try
                {
                    responseJObject = JObject.Parse(html);
                    status = responseJObject["status"].Value<string>();
                }
                catch (Exception exception)
                {
                }

                if (status == "success")
                {
                    string codeStr = responseJObject["countryCode"].Value<string>();

                    CountryCode code;
                    if (Enum.TryParse(codeStr, true, out code))
                    {
                        if (CountriesCollection.Countries.ContainsKey(code))
                        {
                            country = CountriesCollection.Countries[code];
                        }
                    }
                }
            }

            return country;
        }

        #endregion
    }
}

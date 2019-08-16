using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bot.Models;
using Newtonsoft.Json.Linq;

namespace Bot.Services
{
    public class LicenseManager
    {
        #region Methods

        public static async Task<LicenseManagerResult?> Check(LicenseInfo info, CancellationToken cancelToken)
        {
            LicenseManagerResult? res = null;

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };

            HttpClient client = new HttpClient(handler);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{m_baseUrl}/?secret_key={HttpUtility.UrlEncode(m_secretKey)}&slm_action=slm_check&license_key={info.Key}&registered_domain={HttpUtility.UrlEncode(info.DeviceId)}"),
                Method = HttpMethod.Get
            };
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/602.5.6 (KHTML, like Gecko) Chrome/59.0.3035.77 Safari/602.5.6");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            request.Headers.TryAddWithoutValidation("Host", "projectdestroyer.com");
            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            
            string html = await HttpHelper.GetString(request, client, cancelToken);

            if (html != null)
            {
                JObject resJObject = JObject.Parse(html);

                if (resJObject["result"].Value<string>() == "error" || !resJObject["registered_domains"].Any() || /*resJObject["registered_domains"][0]["registered_domain"].Value<string>().Replace("\"", "") !=*/ !html.Contains(info.DeviceId))
                {
                    res = LicenseManagerResult.InvalidKey;
                }
                else
                {
                    res = LicenseManagerResult.Ok;
                }
            }
            else
            {
                res = LicenseManagerResult.ServerIsUnreachable;
            }

            request?.Dispose();
            client?.Dispose();

            return res;
        }

        public static async Task<LicenseManagerResult?> Activate(LicenseInfo info, CancellationToken cancelToken)
        {
            LicenseManagerResult? res = null;

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };

            HttpClient client = new HttpClient(handler);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{m_baseUrl}/?secret_key={HttpUtility.UrlEncode(m_secretKey)}&slm_action=slm_activate&license_key={info.Key}&registered_domain={HttpUtility.UrlEncode(info.DeviceId)}"),
                Method = HttpMethod.Get
            };
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/602.5.6 (KHTML, like Gecko) Chrome/59.0.3035.77 Safari/602.5.6");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            request.Headers.TryAddWithoutValidation("Host", "projectdestroyer.com");
            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");

            string html = await HttpHelper.GetString(request, client, cancelToken);

            if (html != null)
            {
                JObject resJObject = JObject.Parse(html);

                if (resJObject["result"].Value<string>() == "success")
                {
                    SendNotification("prodenx@t-sk.ru", $"Successful activation: {info.DeviceId} Key: {info.Key}");
                    res = LicenseManagerResult.Ok;
                }
                if (resJObject["result"].Value<string>() == "error")
                {
                    res = LicenseManagerResult.InvalidKey;

                    if (resJObject["error_code"].Value<int>() == 110)
                    {
                        res = LicenseManagerResult.Ok;
                    }
                    else if (resJObject["error_code"].Value<int>() == 50)
                    {
                        res = LicenseManagerResult.AlreadyActivated;
                    }
                }
            }
            else
            {
                res = LicenseManagerResult.ServerIsUnreachable;
            }

            request?.Dispose();
            client?.Dispose();

            return res;
        }

        public static async Task<LicenseManagerResult?> Deactivate(LicenseInfo info, CancellationToken cancelToken)
        {
            LicenseManagerResult? res = null;

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };

            HttpClient client = new HttpClient(handler);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{m_baseUrl}/?secret_key={HttpUtility.UrlEncode(m_secretKey)}&slm_action=slm_deactivate&license_key={info.Key}&registered_domain={HttpUtility.UrlEncode(info.DeviceId)}"),
                Method = HttpMethod.Get
            };
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/602.5.6 (KHTML, like Gecko) Chrome/59.0.3035.77 Safari/602.5.6");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            request.Headers.TryAddWithoutValidation("Host", "projectdestroyer.com");
            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");

            string html = await HttpHelper.GetString(request, client, cancelToken);

            if (html != null)
            {
                JObject resJObject = JObject.Parse(html);

                if (resJObject["result"].Value<string>() == "error")
                {
                    res = LicenseManagerResult.InvalidKey;
                }
                else
                {
                    res = LicenseManagerResult.Ok;
                }
            }
            else
            {
                res = LicenseManagerResult.ServerIsUnreachable;
            }

            request?.Dispose();
            client?.Dispose();

            return res;
        }

        private static async Task<bool> SendNotification(string to, string message)
        {
            bool res = true;

            SmtpClient notificationClient = new SmtpClient()
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.projectdestroyer.com",
                Credentials = new NetworkCredential("notification@projectdestroyer.com", "NotificationPassword123!")
            };

            MailMessage mailMessage = new MailMessage("notification@projectdestroyer.com", to);
            mailMessage.Subject = "Activation";
            mailMessage.Body = message;

            try
            {
                await notificationClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                res = false;
            }

            notificationClient.Dispose();

            return res;
        }

        #endregion

        #region Fields

        private static string m_baseUrl = "https://projectdestroyer.com";
        private static string m_secretKey = "59a65a977f6189.37164454";

        #endregion
    }



    public enum LicenseManagerResult
    {
        Ok,
        InvalidKey,
        AlreadyActivated,
        ServerIsUnreachable
    }
}

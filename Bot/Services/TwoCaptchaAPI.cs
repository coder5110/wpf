using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using Bot.Models;
using Newtonsoft.Json.Linq;

namespace Bot.Services
{
    public static class TwoCaptchaAPI
    {
        #region Methods

        public static async Task<string> CreateReCaptchaTask(string apiKey, string siteKey, string sitePath, CancellationToken cancelToken)
        {
            string res = null;

            await m_semaphore.WaitAsync(cancelToken);
            cancelToken.ThrowIfCancellationRequested();

            string html = await HttpHelper.GetString(new Uri($"{m_baseUrl}/in.php?json=1&key={apiKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={sitePath}"), HttpMethod.Get, cancelToken);

            if (html != null)
            {
                try
                {
                    var responseJObject = JObject.Parse(html);

                    if (responseJObject?["status"].Value<int>() == 1)
                    {
                        res = responseJObject["request"].Value<string>();
                    }
                }
                catch (Exception e)
                {
                }
            }

            m_timer.Change(m_delayBetweenRequests, Timeout.Infinite);

            return res;
        }

        public static async Task<List<string>> CheckReCaptchaTask(string apiKey, List<string> taskIdList, CancellationToken cancelToken)
        {
            List<string> res = null;

            await m_semaphore.WaitAsync(cancelToken);
            cancelToken.ThrowIfCancellationRequested();

            string html = await HttpHelper.GetString(new Uri($"{m_baseUrl}/res.php?json=1&key={apiKey}&action=get&ids={string.Join(",", taskIdList)}"), HttpMethod.Get, cancelToken); ;

            if (html != null)
            {
                try
                {
                    JObject responseJObject = JObject.Parse(html);

                    if (responseJObject != null && responseJObject["status"].Value<int>() == 1)
                    {
                        res = responseJObject["request"].Value<string>().Split('|').ToList();

                        if (res.Count != taskIdList.Count)
                        {
                            res = null;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            m_timer.Change(m_delayBetweenRequests, Timeout.Infinite);

            return res;
        }

        #endregion

        #region Fields

        private static string m_baseUrl = "http://2captcha.com";
        private static SemaphoreSlim m_semaphore = new SemaphoreSlim(0, 1);
        private static int m_delayBetweenRequests = 100;
        private static readonly Timer m_timer = new Timer((o) =>
        {
            m_semaphore.Release();
        }, null, m_delayBetweenRequests, Timeout.Infinite);

        #endregion
    }

    public enum TwoCaptchaResponseCode
    {
        CAPCHA_NOT_READY,
        ERROR_CAPTCHA_UNSOLVABLE,
        ERROR_WRONG_CAPTCHA_ID
    }
}

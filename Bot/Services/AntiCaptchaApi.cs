using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Bot.Services
{
    public static class AntiCaptchaApi
    {
        #region Methods

        public static async Task<string> CreateReCaptchaTask(string apiKey, string siteKey, string sitePath, CancellationToken cancelToken)
        {
            string res = null;

            string html = null;

            //await m_semaphore.WaitAsync(cancelToken);
            //cancelToken.ThrowIfCancellationRequested();

            JObject taskJObject = new JObject();
            taskJObject["clientKey"] = apiKey;
            taskJObject["task"] = new JObject();
            taskJObject["task"]["type"] = "NoCaptchaTaskProxyless";
            taskJObject["task"]["websiteURL"] = sitePath;
            taskJObject["task"]["websiteKey"] = siteKey;

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"{m_baseUrl}/createTask"),
                    Method = HttpMethod.Post
                })
                {
                    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    request.Content = new StringContent(taskJObject.ToString());

                    html = await HttpHelper.GetString(request, client, cancelToken);
                }
            }

            //string html = await HttpHelper.GetString(new Uri($"{m_baseUrl}/in.php?json=1&key={apiKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={pageUrl}"), HttpMethod.Get, cancelToken);

            if (html != null)
            {
                try
                {
                    var responseJObject = JObject.Parse(html);

                    if (responseJObject?["errorId"].Value<int>() == 0)
                    {
                        res = responseJObject["taskId"].Value<string>();
                    }
                }
                catch (Exception e)
                {
                }
            }

            //m_timer.Change(m_delayBetweenRequests, Timeout.Infinite);

            return res;
        }

        public static async Task<Dictionary<string, string>> CheckReCaptchaTask(string apiKey, List<string> taskIdList, CancellationToken cancelToken)
        {
            Dictionary<string, string> res = res = new Dictionary<string, string>();

            //await m_semaphore.WaitAsync(cancelToken);
            //cancelToken.ThrowIfCancellationRequested();

            JObject taskJObject = new JObject();
            taskJObject["clientKey"] = apiKey;

            Dictionary<string, Task<string>> requestTasks = new Dictionary<string, Task<string>>();
            
            foreach (string taskId in taskIdList)
            {
                taskJObject["taskId"] = taskId;

                requestTasks[taskId] = HttpHelper.GetString(new Uri($"{m_baseUrl}/getTaskResult"), HttpMethod.Post, new StringContent(taskJObject.ToString()), cancelToken);
            }

            foreach (KeyValuePair<string, Task<string>> pair in requestTasks)
            {
                string html = null;

                html = await pair.Value;

                if (html != null)
                {
                    try
                    {
                        JObject responseJObject = JObject.Parse(html);

                        if (responseJObject != null && responseJObject["errorId"].Value<int>() == 0)
                        {
                            if (responseJObject["status"].Value<string>() == "ready")
                            {
                                res[pair.Key] = responseJObject["solution"]["gRecaptchaResponse"].Value<string>();
                            }
                            else if (responseJObject["status"].Value<string>() == "processing")
                            {
                                res[pair.Key] = AntiCaptchaResponseCode.CAPTCHA_IS_NOT_READY.ToString();
                            }
                            else
                            {
                                res[pair.Key] = AntiCaptchaResponseCode.UNKNOWN_RESPONSE.ToString();
                            }
                        }
                        else
                        {
                            res[pair.Key] = responseJObject["errorCode"].Value<string>();
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            //string html = await HttpHelper.GetString(new Uri($"{m_baseUrl}/res.php?json=1&key={apiKey}&action=get&ids={string.Join(",", taskIdList)}"), HttpMethod.Get, cancelToken); ;

            //m_timer.Change(m_delayBetweenRequests, Timeout.Infinite);

            return res;
        }

        #endregion

        #region Fields

        private static string m_baseUrl = "https://api.anti-captcha.com";
        private static SemaphoreSlim m_semaphore = new SemaphoreSlim(0, 1);
        private static int m_delayBetweenRequests = 100;
        //private static readonly Timer m_timer = new Timer((o) =>
        //{
        //    m_semaphore.Release();
        //}, null, m_delayBetweenRequests, Timeout.Infinite);

        #endregion
    }

    enum AntiCaptchaResponseCode
    {
        ERROR_CAPTCHA_UNSOLVABLE,
        ERROR_NO_SUCH_CAPCHA_ID,
        CAPTCHA_IS_NOT_READY,
        UNKNOWN_RESPONSE,
        ERROR_NO_SLOT_AVAILABLE
    }
}

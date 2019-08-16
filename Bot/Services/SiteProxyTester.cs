using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.Services
{
    public class SiteProxyTester
    {
        #region Constructors



        #endregion

        #region Methods

        public static async Task<SiteProxyTestResult> RunTest(Proxy proxy, Url url, CancellationToken cancelToken = new CancellationToken())
        {
            SiteProxyTestResult result = null;
            List<long> pings = new List<long>();
            Stopwatch timeWatch = new Stopwatch();

            using (HttpClientHandler httpClienthandler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = new WebProxy(new Uri($"http://{proxy.IP}:{proxy.Port}"), true, null,
                    new NetworkCredential(proxy.Username, proxy.Password)),
            })
            {
                using (HttpClient client = new HttpClient(httpClienthandler))
                {

                    int i = 0;
                    for (; i < m_maxRetries; i++)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        HttpResponseMessage response;

                        timeWatch.Restart();

                        try
                        {
                            response =
                                await HttpHelper.GetResponse(new Uri(url.Value), HttpMethod.Get, client, cancelToken,
                                    0);
                        }
                        catch (Exception e)
                        {
                            if (e.GetType() == typeof(TaskCanceledException))
                            {
                                throw;
                            }

                            result = new SiteProxyTestResult(HttpStatusCode.Gone, e.Message, -1,
                                SiteProxyTestStatus.Failed);

                            timeWatch.Stop();

                            break;
                        }

                        timeWatch.Stop();

                        pings.Add(timeWatch.ElapsedMilliseconds);

                        if (response == null)
                        {
                            result = new SiteProxyTestResult(HttpStatusCode.Gone, "no connection", -1,
                                SiteProxyTestStatus.Failed);

                            break;
                        }

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            result = new SiteProxyTestResult(response.StatusCode, response.ReasonPhrase, -1,
                                SiteProxyTestStatus.Failed);

                            response.Dispose();

                            break;
                        }

                        response.Dispose();

                        await Task.Delay(m_delayTime, cancelToken);

                    }

                    if (i == m_maxRetries)
                    {
                        result = new SiteProxyTestResult(HttpStatusCode.OK, "PASSED", (int) pings.Average(),
                            SiteProxyTestStatus.Passed);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Fields

        private static int m_maxRetries = 5;
        private static int m_delayTime = 1000;

        #endregion
    }

    public enum SiteProxyTestStatus
    {
        Passed,
        Failed,
        Testing
    }


    public class SiteProxyTestResult: BindableObject
    {
        #region Constructors

        public SiteProxyTestResult(HttpStatusCode code, string message, int ping, SiteProxyTestStatus status)
        {
            Code = code;
            Message = message;
            Ping = ping;
            Status = status;
        }

        #endregion

        #region Properties

        public readonly HttpStatusCode Code;
        public readonly string Message;
        public readonly int Ping;
        public readonly SiteProxyTestStatus Status;

        #endregion

        #region Fields

        

        #endregion
    }
}

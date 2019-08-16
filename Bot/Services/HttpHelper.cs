using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Services
{
    class HttpHelper
    {
        public static async Task<string> GetString(Uri url, HttpMethod method, CancellationToken cancelToken, int retries = 3)
        {
            string str;
            
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (HttpClient client = new HttpClient(handler))
            {
                str = await GetString(url, method, client, cancelToken, retries);
            }

            return str;
        }

        public static async Task<string> GetString(Uri url, HttpMethod method, HttpClient client, CancellationToken cancelToken, int retries = 3, bool pushAllExceptions = false)
        {
            string str = null;
            
            using (HttpResponseMessage response = await GetResponse(url, method, client, cancelToken, retries, pushAllExceptions))
            {
                if (response != null)
                {
                    str = await response.Content.ReadAsStringAsync();
                }
            }

            return str;
        }

        public static async Task<string> GetString(HttpRequestMessage request, HttpClient client, CancellationToken cancelToken)
        {
            string str = null;

            using (HttpResponseMessage response = await GetResponse(request, client, cancelToken))
            {
                if (response != null)
                {
                    str = await response.Content.ReadAsStringAsync();
                }
            }

            return str;
        }

        public static async Task<string> GetString(HttpRequestMessage request, CancellationToken cancelToken)
        {
            string str = null;

            using (HttpResponseMessage response = await GetResponse(request, cancelToken))
            {
                if (response != null)
                {
                    str = await response.Content.ReadAsStringAsync();
                }
            }

            return str;
        }

        public static async Task<string> GetString(Uri url, HttpMethod method, HttpContent content, CancellationToken cancelToken, int retries = 3)
        {
            string str;

            using (HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = url,
                Method = method,
                Content = content
            })
            {
                str = await GetString(request, cancelToken);
            }

            return str;
        }

        public static string GetStringSync(HttpRequestMessage request, HttpClient client, out HttpStatusCode? statusCode, CancellationToken cancelToken)
        {
            string res = null;

            try
            {
                Task<HttpResponseMessage> httpTask = GetResponse(request, client, cancelToken);
                httpTask.Wait(cancelToken);
                HttpResponseMessage response = httpTask.Result;
                
                Task<string> htmlTask = response.Content.ReadAsStringAsync();
                htmlTask.Wait(cancelToken);
                
                res = htmlTask.Result;
                statusCode = response.StatusCode;

                response.Dispose();
            }
            catch (Exception ex)
            {
                statusCode = null;
            }

            return res;
        }

        public static async Task<Stream> GetStream(Uri url, HttpMethod method, CancellationToken cancelToken, int retries = 3)
        {
            Stream stream = null;

            using (HttpClient client = new HttpClient())
            {
                stream = await GetStream(url, method, client, cancelToken, retries);
            }

            return stream;
        }

        public static async Task<Stream> GetStream(Uri url, HttpMethod method, HttpClient client,  CancellationToken cancelToken, int retries = 3)
        {
            Stream ret = null;

            using (HttpResponseMessage response = await GetResponse(url, method, client, cancelToken, retries))
            {
                if (response != null)
                {
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        if (stream != null)
                        {
                            ret = new MemoryStream();
                            await stream.CopyToAsync(ret);
                            ret.Position = 0;
                        }
                    }
                }
            }

            return ret;
        }

        public static async Task<HttpResponseMessage> GetResponse(Uri url, HttpMethod method, CancellationToken cancelToken, int retries = 3)
        {
            HttpResponseMessage response = null;

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.Deflate
            };

            using (HttpClient client = new HttpClient(httpClientHandler))
            {
                response = await GetResponse(url, method, client, cancelToken, retries);
            }

            httpClientHandler.Dispose();

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponse(Uri url, HttpMethod method, HttpClient client, CancellationToken cancelToken, int retries = 3, bool pushAllExceptions = false)
        {
            HttpResponseMessage response = null;
            uint retriesCount = 0;
            Exception exception = null;

            while (retriesCount <= retries)
            {
                try
                {
                    exception = null;

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = url,
                        Method = method
                    };
                    request.Headers.TryAddWithoutValidation("User-Agent", m_agent);
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                    response = await client.SendAsync(request, cancelToken);
                }
                catch (Exception ex)
                {
                    exception = ex;

                    if (ex.GetType() == typeof(TaskCanceledException))
                    {
                        throw;
                    }

                    retriesCount++;
                    continue;
                }

                break;
            }

            if (exception != null && pushAllExceptions)
            {
                throw exception;
            }

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponse(HttpRequestMessage request, HttpClient client, CancellationToken cancelToken)
        {
            HttpResponseMessage response = null;
            
            try
            {
                response = await client.SendAsync(request, cancelToken);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TaskCanceledException))
                {
                    throw;
                }
            }

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponse(HttpRequestMessage request, CancellationToken cancelToken)
        {
            HttpResponseMessage response = null;

            using (HttpClient client = new HttpClient())
            {
                response = await GetResponse(request, client, cancelToken);
            }

            return response;
        }

        #region Fields

        private static string m_agent = "User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Animation;
using System.Xaml.Schema;
using Bot.Helpers;
using Bot.Services;
using Bot.Views;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Bot.Models
{
    public class SupremeUSABot: ReleaseTaskBase
    {
        #region Constructors

        public SupremeUSABot(Release release) : base(release)
        {
        }

        #endregion

        #region Methods

        protected override ICheckoutTaskContext CreateContext(CheckoutTask task)
        {
            HttpClientHandler clientHandler;

            if (task.Proxy != null)
            {
                clientHandler = new HttpClientHandler()
                {
                    Proxy = new WebProxy()
                    {
                        Address = new Uri($"http://{task.Proxy.IP}:{task.Proxy.Port}"),
                        Credentials = new NetworkCredential(task.Proxy.Username, task.Proxy.Password)
                    },
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = true
                };
            }
            else
            {
                clientHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };
            }

            clientHandler.CookieContainer.Add(new Cookie()
            {
                Name = "tohru",
                Value = m_tohruId,
                Domain = ".supremenewyork.com",
                Expired = false,
                Expires = DateTime.Now.AddDays(365),
                HttpOnly = false,
                Secure = false,
                Path = "/"
            });

            return new SupremeUSABotTaskContext()
            {
                Client = new HttpClient(clientHandler)
            };
        }

        protected override bool CheckProduct(ReleaseProductInfo productInfo, Proxy proxy, CancellationToken cancelToken, out List<string> availableSizes)
        {
            bool ret = false;

            availableSizes = null;

            if (!string.IsNullOrEmpty(productInfo.ProductLink))
            {
                Logger.LogEvent(Log, "PRODUCT MONITOR", $"Get product by early link - {productInfo.ProductLink}");
                if (CheckByEarlyLink(productInfo.ProductLink, proxy, cancelToken, out availableSizes))
                {
                    ret = true;
                }
                else
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Product is not found by early link");
                }
            }

            if (productInfo.KeyWords.Any() && !ret)
            {
                Logger.LogEvent(Log, "PRODUCT MONITOR", $"Get product by keywords");
                if (CheckByKeywords(productInfo.KeyWords, proxy, cancelToken, out availableSizes))
                {
                    ret = true;
                }
                else
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Product is not found by keywords");
                }
            }

            if (ret)
            {
                Logger.LogEvent(Log, "PRODUCT MONITOR", $"{ProductName} has been selected.");
            }

            return ret;
        }

        private bool CheckByEarlyLink(string earlyLink, Proxy proxy, CancellationToken cancelToken, out List<string> availableSizes)
        {
            bool ret = false;
            availableSizes = null;

            HttpMessageHandler handler;
            if (proxy != null)
            {
                handler = new HttpClientHandler()
                {
                    Proxy = new WebProxy()
                    {
                        Address = new Uri($"http://{proxy.IP}:{proxy.Port}"),
                        Credentials = new NetworkCredential(proxy.Username, proxy.Password)
                    },
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = true
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = false
                };
            }

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(earlyLink),
                Method = HttpMethod.Get,
            })
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                    html = HttpHelper.GetStringSync(request, client, out statusCode, cancelToken);
                }
            }

            if (html != null)
            {
                html = HttpUtility.HtmlDecode(html);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                Regex productCodeRegex = new Regex("action=\"/shop/([A-Za-l0-9]*)/");
                Regex productStyleUrlCodeFull = new Regex("/shop/([^/]*)/([^/]*)/([^/]*)");

                try
                {
                    m_productCode = productCodeRegex.Match(html)
                        .Groups[1]
                        .Value;

                    Match productStyleUrlCodeFullMatch = productStyleUrlCodeFull.Match(earlyLink);

                    if (productStyleUrlCodeFullMatch.Groups.Count == 4)
                    {
                        m_productStyle = doc.DocumentNode
                            .SelectSingleNode(
                                $"//a[@href='{earlyLink.Split(new[] {Release.Footsite.Domain}, StringSplitOptions.None).Last()}']")
                            .GetAttributeValue("data-styleid", null);
                    }

                    //m_productStyle = productAddFormNode.SelectSingleNode($"//input[@name='{m_styleFieldName}']")
                    //    .GetAttributeValue("value", "");
                }
                catch (Exception e)
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Could not get required information about product. Contact with support.");
                }

                if (m_productCode != null /*&& m_productStyle != null*/)
                {
                    List<SupremeProductStyleInfo> styles = GetProductStyles(m_productCode, proxy, cancelToken);

                    if (styles != null && styles.Any())
                    {
                        SupremeProductStyleInfo style = null;

                        if (m_productStyle != null)
                        {
                            style = styles.FirstOrDefault(s => s.Id == m_productStyle);
                        }
                        else
                        {
                            style = styles.First();
                            m_productStyle = style.Id;
                        }

                        if (style != null)
                        {
                            availableSizes = new List<string>();

                            foreach (SupremeProductSizeInfo sizeInfo in style.Sizes)
                            {
                                string size = ConvertSupremeSizeToCommonSize(sizeInfo.Name);

                                if (size != null && size != "undefined")
                                {
                                    availableSizes.Add(size);
                                    lock (m_lock)
                                    {
                                        m_sizeCodes[size] = sizeInfo.Code;
                                    }
                                }
                            }

                            try
                            {
                                ProductName = doc.GetElementbyId("details").SelectSingleNode("//h1[@itemprop='name']").InnerText;
                            }
                            catch (Exception e)
                            {
                                    
                            }

                            if (ProductName != null)
                            {
                                ProductName += " / " + style.Name;
                            }
                            else
                            {
                                ProductName = earlyLink;
                            }

                            ret = true;
                        }
                    }
                }
            }

            return ret;
        }

        private bool CheckByKeywords(List<string> keyWords, Proxy proxy, CancellationToken cancelToken, out List<string> availableSizes)
        {
            bool ret = false;
            availableSizes = null;

            HttpMessageHandler handler;
            if (proxy != null)
            {
                handler = new HttpClientHandler()
                {
                    Proxy = new WebProxy()
                    {
                        Address = new Uri($"http://{proxy.IP}:{proxy.Port}"),
                        Credentials = new NetworkCredential(proxy.Username, proxy.Password)
                    },
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = true
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = false
                };
            }

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{Release.Footsite.BaseUrl}{m_productListPath}"),
                Method = HttpMethod.Get,
            })
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    html = HttpHelper.GetStringSync(request, client, out statusCode, cancelToken);
                }
            }

            if (html != null)
            {
                JObject productListJObject = null;
                List<JObject> match = new List<JObject>();

                try
                {
                    productListJObject = JObject.Parse(html);

                    if (Release.Category == "All")
                    {
                        foreach (JProperty jToken in productListJObject["products_and_categories"].Values<JProperty>())
                        {
                            if (jToken.Name.ToLower() != "new")
                            {
                                match.AddRange(jToken.First()
                                    .Value<JArray>()
                                    .Values<JObject>()
                                    .Where(o => keyWords.All(w => o["name"].Value<string>().ToLower().Contains(w.ToLower())))
                                    .ToList());
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            match.AddRange(productListJObject["products_and_categories"][Release.Category]
                                .Values<JObject>()
                                .Where(o => keyWords.All(w => o["name"].Value<string>().ToLower().Contains(w.ToLower())))
                                .ToList());
                        }
                        catch (Exception e)
                        {
                            
                        }
                    }

                }
                catch (Exception e)
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Could not get list of products. Contact with support.");
                }

                if (match.Any())
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", $"{match.Count} products are found.");

                    SupremeProductStyleInfo style = null;
                    string productCode = null;
                    string productName = null;

                    if (!string.IsNullOrEmpty(Release.Style))
                    {
                        foreach (JObject o in match)
                        {
                            cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                            if (cancelToken.IsCancellationRequested) break;

                            style = MatchStyle(Release.Style, o, proxy, cancelToken);

                            if (style != null)
                            {
                                productCode = o["id"].Value<string>();
                                productName = o["name"].Value<string>();

                                break;
                            }
                        }
                    }
                    else
                    {
                        JObject o = match.First();

                        List<SupremeProductStyleInfo> styles = GetProductStyles(o["id"].Value<string>(), proxy, cancelToken);

                        if (styles != null && styles.Any())
                        {
                            style = styles.First();
                            productCode = o["id"].Value<string>();
                            productName = o["name"].Value<string>();
                        }
                    }

                    if (style != null)
                    {
                        availableSizes = new List<string>();

                        foreach (SupremeProductSizeInfo sizeInfo in style.Sizes)
                        {
                            string size = ConvertSupremeSizeToCommonSize(sizeInfo.Name);

                            if (size != null && size != "undefined")
                            {
                                availableSizes.Add(size);
                                lock (m_lock)
                                {
                                    m_sizeCodes[size] = sizeInfo.Code;
                                }
                            }
                        }

                        m_productCode = productCode;
                        m_productStyle = style.Id;

                        ProductName = $"{productName} / {style.Name}";

                        ret = true;
                    }
                }
            }

            return ret;
        }

        private List<SupremeProductStyleInfo> GetProductStyles(string id, Proxy proxy, CancellationToken cancelToken)
        {
            List<SupremeProductStyleInfo> styles = null;

            HttpMessageHandler handler;
            if (proxy != null)
            {
                handler = new HttpClientHandler()
                {
                    Proxy = new WebProxy()
                    {
                        Address = new Uri($"http://{proxy.IP}:{proxy.Port}"),
                        Credentials = new NetworkCredential(proxy.Username, proxy.Password)
                    },
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = true
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = false
                };
            }

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{Release.Footsite.BaseUrl}/shop/{id}.josn"),
                Method = HttpMethod.Get
            })
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", MobileBrowserAgent);
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                    html = HttpHelper.GetStringSync(request, client, out statusCode, cancelToken);
                }
            }

            if (html != null)
            {
                JObject stylesJObject = null;

                try
                {
                    stylesJObject = JObject.Parse(html);

                    styles = new List<SupremeProductStyleInfo>();

                    foreach (JObject o in stylesJObject["styles"].Values<JObject>())
                    {
                        SupremeProductStyleInfo style = new SupremeProductStyleInfo()
                        {
                            Id = o["id"].Value<string>(),
                            Name = o["name"].Value<string>()
                        };

                        foreach (JObject sizeJObject in o["sizes"].Values<JObject>())
                        {
                            style.Sizes.Add(new SupremeProductSizeInfo()
                            {
                                Name = sizeJObject["name"].Value<string>(),
                                Code = sizeJObject["id"].Value<string>(),
                                InSotck = sizeJObject["stock_level"].Value<int>(),
                            });
                        }

                        styles.Add(style);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Could not get styles for product. Contact with support.");
                    styles = null;
                }
            }

            return styles;
        }

        private SupremeProductStyleInfo MatchStyle(string styleName, JObject productJObject, Proxy proxy, CancellationToken cancelToken)
        {
            SupremeProductStyleInfo res = null;

            Logger.LogEvent(Log, "PRODUCT MONITOR", $"Check styles for {productJObject["name"]}...");

            string productCode = productJObject["id"].Value<string>();

            List<SupremeProductStyleInfo> styles = GetProductStyles(productCode, proxy, cancelToken);

            if (styles != null)
            {
                foreach (SupremeProductStyleInfo style in styles)
                {
                    if (style.Name.ToLower() == styleName.ToLower())
                    {
                        res = style;
                    }
                }
            }

            return res;
        }

        protected override StepResult AddToCart(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;
            
            string html = null;
            HttpStatusCode? statusCode = null;

            if (m_sizeCodes.ContainsKey(task.Size))
            {
                using (HttpRequestMessage addToCartRequest = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"{task.Footsite.BaseUrl}/shop/{m_productCode}/add.json"),
                    Method = HttpMethod.Post
                })
                {
                    addToCartRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                    addToCartRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                    addToCartRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    addToCartRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    addToCartRequest.Headers.TryAddWithoutValidation("Refer", "http://www.supremenewyork.com/mobile/");
                    addToCartRequest.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    addToCartRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>(m_styleFieldName, m_productStyle),
                        new KeyValuePair<string, string>("size", m_sizeCodes[task.Size]),
                        new KeyValuePair<string, string>("qty", "1") 
                    });

                    html = HttpHelper.GetStringSync(addToCartRequest, cxt.Client, out statusCode, cancelToken);
                }

                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

                bool? isCarted = null;

                try
                {
                    JArray responseJArray = JArray.Parse(html);

                    foreach (JToken token in responseJArray)
                    {
                        if (token["size_id"].Value<string>() == m_sizeCodes[task.Size])
                        {
                            isCarted = true;
                            break;
                        }
                    }

                    isCarted = isCarted ?? false;
                }
                catch (Exception e)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not parse add to cart request response. Contact with support.");
                    
                }

                if (isCarted != null)
                {
                    res = isCarted.Value ? StepResult.Ok : StepResult.OutOfStock;
                }
                else
                {
                    res = StepResult.Failed;
                }
            }
            else
            {
                res = StepResult.OutOfStock;
            }

            return res;
        }

        protected override StepResult CheckCart(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage cartRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_cartPath}"),
                Method = HttpMethod.Get
            })
            {
                cartRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                cartRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                cartRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                html = HttpHelper.GetStringSync(cartRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (doc.GetElementbyId($"item_{m_sizeCodes[task.Size]}") != null)
            {
                res = StepResult.Ok;
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        protected override StepResult Billing(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            cxt.ResetCheckoutInfo();

            using (HttpRequestMessage checkoutPageRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://{task.Footsite.Domain}{m_checkoutPagePath}"),
                Method = HttpMethod.Get
            })
            {
                checkoutPageRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                checkoutPageRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                checkoutPageRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                html = HttpHelper.GetStringSync(checkoutPageRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            cxt.CheckoutTimestamp = TimeHelper.GetUnixTimeStamp();

            using (HttpRequestMessage tohruIdRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(m_tohruIdPath),
                Method = HttpMethod.Get
            })
            {
                tohruIdRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                tohruIdRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                tohruIdRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                HttpHelper.GetStringSync(tohruIdRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode checkoutFormNode1 = doc.GetElementbyId("checkout_form");
            HtmlNode checkoutFormNode2 = doc.DocumentNode.SelectSingleNode("//*[@action='/checkout']");

            HtmlNode checkoutFormNode = checkoutFormNode1 ?? checkoutFormNode2;

            if (checkoutFormNode != null)
            {
                HtmlNode creditCardDetailsNode = doc.GetElementbyId("card_details");

                if (creditCardDetailsNode != null)
                {
                    try
                    {
                        HtmlNodeCollection divs = creditCardDetailsNode.SelectNodes("div");

                        cxt.CreditCardNumberFieldName = divs[0]
                            .SelectSingleNode("input")
                            .GetAttributeValue("name", cxt.CreditCardNumberFieldName);
                        cxt.CreditCardMonthFieldName = divs[1].SelectNodes("select")[0]
                            .GetAttributeValue("name", cxt.CreditCardMonthFieldName);
                        cxt.CreditCardYearFieldName = divs[1].SelectNodes("select")[1]
                            .GetAttributeValue("name", cxt.CreditCardYearFieldName);
                        cxt.CreditCardCVVFieldName = divs[2]
                            .SelectSingleNode("input")
                            .GetAttributeValue("name", cxt.CreditCardCVVFieldName);

                        cxt.CsrfToken = doc.DocumentNode.SelectSingleNode("//meta[@name='csrf-token']")
                            .GetAttributeValue("content", "");
                        cxt.StoreId = checkoutFormNode.SelectSingleNode("//input[@name='store_credit_id']")
                            .GetAttributeValue("value", "");
                        cxt.Utf8Symbol = checkoutFormNode.SelectSingleNode("//input[@name='utf8']")
                            .GetAttributeValue("value", "");
                        cxt.AuthenticityToken = checkoutFormNode.SelectSingleNode("//input[@name='authenticity_token']")
                            .GetAttributeValue("value", "");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Could not parse credit card form. Default field names will be used.");
                    }
                }
                else
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Could not find credit card form. Default field names will be used.");
                }

                try
                {
                    HtmlNode extraFieldNode = doc.GetElementbyId("cart-cc").SelectSingleNode("fieldset").SelectNodes("input").Last();

                    cxt.ExtraField = new KeyValuePair<string, string>(extraFieldNode.GetAttributeValue("name", ""), extraFieldNode.GetAttributeValue("value", ""));
                }
                catch (Exception e)
                {
                    
                }
            }
            else
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get checkout form. Contact with support. Default values will be used");
                
                //res = StepResult.Failed;
            }

            return res;
        }

        protected override StepResult Paying(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            int timeElapsed = TimeHelper.GetUnixTimeStamp() - cxt.CheckoutTimestamp;

            if (timeElapsed * 1000 < task.Footsite.Settings.DelayInCheckout)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}",
                    $"Delay in checkout {task.Footsite.Settings.DelayInCheckout - timeElapsed * 1000} ms");

                cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(task.Footsite.Settings.DelayInCheckout - timeElapsed * 1000));
                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            }

            task.State = CheckoutTaskState.WaitingCaptcha;
            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Waiting captcha...");

            string token = null;

            while (true)
            {
                ICaptchaHarvesterTask harvesterTask = new CaptchaHarvesterTaskBase(null);

                Release.CaptchaHarvester.GetSolution(harvesterTask);

                WaitHandle.WaitAny(new[] { harvesterTask.SolutionReadyEvent, cancelToken.WaitHandle });

                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;

                RecaptchaSolution solution = harvesterTask.Solution as RecaptchaSolution;

                if (solution.TimeStamp > cxt.CheckoutTimestamp)
                {
                    token = solution.Value;

                    break;
                }

                harvesterTask.SolutionReadyEvent.Dispose();
            }

            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Waiting captcha... done");

            task.State = CheckoutTaskState.Paying;

            using (HttpRequestMessage tohruCommitMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(m_tohruCommitPath),
                Method = HttpMethod.Post
            })
            {
                tohruCommitMessage.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                tohruCommitMessage.Headers.TryAddWithoutValidation("Accept", "*/*");
                tohruCommitMessage.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                tohruCommitMessage.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                tohruCommitMessage.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                tohruCommitMessage.Content = new StringContent(m_tohruTrackInfo);

                HttpHelper.GetStringSync(tohruCommitMessage, cxt.Client, out statusCode, cancelToken);
            }

            string billPhoneNumber = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PhoneNumber.Insert(3, "-").Insert(7, "-");

            using (HttpRequestMessage checkoutRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://{task.Footsite.Domain}{m_checkoutPath}"),
                Method = HttpMethod.Post
            })
            {
                checkoutRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                checkoutRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                checkoutRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                checkoutRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                checkoutRequest.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                checkoutRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                checkoutRequest.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                checkoutRequest.Headers.Add("X-CSRF-Token", cxt.CsrfToken);
                checkoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("store_credit_id", cxt.StoreId),
                    new KeyValuePair<string, string>("same_as_billing_address", "1"),
                    new KeyValuePair<string, string>("order[billing_name]", $"{task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.FirstName} {task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.SecondName}"),
                    new KeyValuePair<string, string>("order[email]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.Email),
                    new KeyValuePair<string, string>("order[tel]", billPhoneNumber),
                    new KeyValuePair<string, string>("order[billing_address]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddress1),
                    new KeyValuePair<string, string>("order[billing_address_2]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddressLine2),
                    new KeyValuePair<string, string>("order[billing_city]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.City),
                    new KeyValuePair<string, string>("order[billing_state]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.State?.Abbreviation),
                    new KeyValuePair<string, string>("order[billing_zip]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PostalCode),
                    new KeyValuePair<string, string>("order[billing_country]", ConvertCommonCountryNameToSupremeName(task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.Country.Code)),
                    new KeyValuePair<string, string>("credit_card[type]", ConvertCommonCreditCardTypeToSupremeType(task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Type.Code) ?? ""),
                    new KeyValuePair<string, string>(cxt.CreditCardNumberFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Number),
                    new KeyValuePair<string, string>(cxt.CreditCardMonthFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("MM")),
                    new KeyValuePair<string, string>(cxt.CreditCardYearFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("yyyy")),
                    new KeyValuePair<string, string>(cxt.CreditCardCVVFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.CVS),
                    new KeyValuePair<string, string>("order[terms]", "0"),
                    new KeyValuePair<string, string>("order[terms]", "1"),
                    new KeyValuePair<string, string>("g-recaptcha-response", token),
                    new KeyValuePair<string, string>("utf8", cxt.Utf8Symbol),
                    new KeyValuePair<string, string>("authenticity_token", cxt.AuthenticityToken),

                    //cxt.ExtraField

                });

                html = HttpHelper.GetStringSync(checkoutRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            JObject checkoutReponseJObject = null;
            string checkoutStatus = null;
            string slug = null;

            try
            {
                checkoutReponseJObject = JObject.Parse(html);
                checkoutStatus = checkoutReponseJObject["status"].Value<string>();
            }
            catch (Exception e)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get checkout response. Contact with support.");
            }

            if (checkoutStatus != null)
            {
                if (checkoutStatus == "queued")
                {
                    try
                    {
                        slug = checkoutReponseJObject["slug"].Value<string>();
                    }
                    catch (Exception e)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get queue id. Contact with support.");
                    }

                    if (slug != null)
                    {
                        cxt.Slug = slug;

                        CheckoutStep queuedCheckout = new CheckoutStep(QueuedCheckout, "Waiting in Queue",
                            CheckoutTaskState.Undefined);
                        CheckoutStep retryCheckout = new CheckoutStep(RetryCheckout, "Checking paying status",
                            CheckoutTaskState.Undefined);
                        TimeSpan executionTime;

                        res = queuedCheckout.Run(cxt, task,
                            TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime,
                            cancelToken);

                        if (res == StepResult.Ok)
                        {
                            checkoutStatus = cxt.QueuedCheckoutResponse["status"].Value<string>();
                            checkoutReponseJObject = cxt.QueuedCheckoutResponse;

                            int attempts = 1;

                            while (attempts < 5 && res != StepResult.Canceled && checkoutStatus == "failed")
                            {
                                Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Attempt {attempts} is failed... retry");
                                attempts++;

                                cancelToken.WaitHandle.WaitOne(
                                    TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod));
                                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;

                                res = retryCheckout.Run(cxt, task,
                                    TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod),
                                    out executionTime, cancelToken);

                                if (res == StepResult.Ok)
                                {
                                    try
                                    {
                                        checkoutStatus = cxt.QueuedCheckoutResponse["status"].Value<string>();
                                        checkoutReponseJObject = cxt.QueuedCheckoutResponse;
                                    }
                                    catch (Exception e)
                                    {
                                        res = StepResult.Failed;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        res = StepResult.Failed;
                    }
                }

                if (res == StepResult.Ok)
                {
                    if (checkoutStatus == "failed")
                    {
                        string errorMessage = "";

                        try
                        {
                            foreach (JToken errorJToken in checkoutReponseJObject["errors"])
                            {
                                errorMessage += $"{errorJToken.First().Value<string>()};";
                            }
                        }
                        catch (Exception e)
                        {
                            ;
                        }

                        if (errorMessage != "")
                        {
                            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got errors: {errorMessage}");
                        }

                        res = StepResult.Failed;
                    }
                    else if (checkoutStatus.ToLower() == "outofstock")
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Product is out of stock");

                        res = StepResult.Failed;
                    }
                    else if (checkoutStatus == "dup")
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got duplication");

                        res = StepResult.Failed;
                    }
                    else if (checkoutReponseJObject.ToString().Contains("Your order has been submitted"))
                    {
                        res = StepResult.Ok;
                    }
                    else
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}",
                            $"Could not resolve checkout status '{checkoutStatus}'. Contact with support.");

                        res = StepResult.Failed;
                    }
                }
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        protected StepResult QueuedCheckout(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            JObject checkoutReponseJObject = null;
            string checkoutStatus = "queued";

            cxt.QueuedCheckoutResponse = null;

            while (checkoutStatus == "queued")
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Queued...");

                cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(1000));
                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;

                using (HttpRequestMessage checkoutRequest = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://{task.Footsite.Domain}/checkout/{cxt.Slug}/status.json"),
                    Method = HttpMethod.Get
                })
                {
                    checkoutRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                    checkoutRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                    checkoutRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    checkoutRequest.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                    checkoutRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    checkoutRequest.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                    checkoutRequest.Headers.Add("X-CSRF-Token", cxt.CsrfToken);

                    html = HttpHelper.GetStringSync(checkoutRequest, cxt.Client, out statusCode, cancelToken);
                }

                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

                checkoutStatus = null;

                try
                {
                    checkoutReponseJObject = JObject.Parse(html);
                    checkoutStatus = checkoutReponseJObject["status"].Value<string>();
                }
                catch (Exception e)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get checkout response. Contact with support.");
                }

                try
                {
                    cxt.Slug = checkoutReponseJObject["slug"].Value<string>();
                }
                catch (Exception e)
                {
                }
            }

            if (checkoutStatus != null)
            {
                cxt.QueuedCheckoutResponse = checkoutReponseJObject;

                res = StepResult.Ok;
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        protected StepResult RetryCheckout(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            cxt.QueuedCheckoutResponse = null;

            using (HttpRequestMessage checkoutRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://{task.Footsite.Domain}/checkout/{cxt.Slug}/status.json"),
                Method = HttpMethod.Get
            })
            {
                checkoutRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                checkoutRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                checkoutRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                checkoutRequest.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                checkoutRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                checkoutRequest.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                checkoutRequest.Headers.Add("X-CSRF-Token", cxt.CsrfToken);

                html = HttpHelper.GetStringSync(checkoutRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            JObject checkoutReponseJObject = null;
            string checkoutStatus = null;

            try
            {
                checkoutReponseJObject = JObject.Parse(html);
                checkoutStatus = checkoutReponseJObject["status"].Value<string>();
            }
            catch (Exception e)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get retry checkout response. Contact with support.");
            }
            
            if (checkoutStatus != null)
            {
                cxt.QueuedCheckoutResponse = checkoutReponseJObject;

                res = StepResult.Ok;
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        protected string ConvertSupremeSizeToCommonSize(string size)
        {
            string res = "undefined";
            
            if (size == "XSmall")
            {
                res = "XS";
            }
            else if (size == "Small")
            {
                res = "S";
            }
            else if (size == "Medium")
            {
                res = "M";
            }
            else if (size == "Large")
            {
                res = "L";
            }
            else if (size == "XLarge")
            {
                res = "XL";
            }
            else
            {
                res = size;
            }

            return res;
        }

        protected string ConvertCommonCreditCardTypeToSupremeType(PayCardTypeCode code)
        {
            string res = null;

            if (code == PayCardTypeCode.Visa)
            {
                res = "visa";
            }
            else if (code == PayCardTypeCode.Amex)
            {
                res = "american_express";
            }
            else if (code == PayCardTypeCode.MasterCard)
            {
                res = "master";
            }
            else if (code == PayCardTypeCode.JCB)
            {
                res = "jcb";
            }

            return res;
        }

        protected string ConvertCommonCountryNameToSupremeName(CountryCode code)
        {
            string res = null;

            if (code == CountryCode.US)
            {
                res = "USA";
            }
            else if (code == CountryCode.CA)
            {
                res = "CANADA";
            }
            else
            {
                res = code.ToString();
            }

            return res;
        }

        protected StepResult CheckResult(string html, HttpStatusCode? statusCode, bool htmlIsRequired = true)
        {
            StepResult res = StepResult.Ok;

            if (statusCode == null)
            {
                res = StepResult.Error;
            }
            else if ((int)statusCode > 400 || (html == null && htmlIsRequired))
            {
                res = StepResult.Failed;
            }

            return res;
        }

        #endregion

        #region Fields

        private readonly Footsite m_footsite = FootsiteCollection.SitesDictionary[FootsiteType.SupremeUSA];
        private string m_productCode = null;
        private string m_productStyle = null;
        private readonly Dictionary<string, string> m_sizeCodes = new Dictionary<string, string>();
        protected string m_selectSizeId = "s";
        protected string m_sizeFieldName = "s";
        protected string m_styleFieldName = "st";
        private readonly string m_cartPath = @"/shop/cart";
        protected readonly string m_checkoutPagePath = @"/checkout";
        protected readonly string m_checkoutPath = @"/checkout.json";
        private readonly string m_productListPath = @"/mobile_stock.json";
        protected string m_tohruId = "0268080b-1799-4624-af32-0b3009331d51";
        protected string m_tohruIdPath = "https://teleus.supremenewyork.com/id";
        protected string m_tohruCommitPath = "https://teleus.supremenewyork.com/commit";
        private readonly object m_lock = new object();

        #endregion

        #region TrackInfo

        protected string m_tohruTrackInfo = "";

        #endregion
    }

    public class SupremeUSABotTaskContext : BindableObject, ICheckoutTaskContext
    {
        #region Constructors

        public SupremeUSABotTaskContext()
        {
            ResetCheckoutInfo();
        }

        public void Dispose()
        {
        }

        #endregion

        #region Properties

        public HttpClient Client
        {
            get
            {
                lock (m_lock)
                {
                    return m_client;
                }
            }

            set { SetProperty(ref m_client, value, m_lock); }
        }

        public string Slug
        {
            get
            {
                lock (m_lock)
                {
                    return m_slug;
                }
            }

            set { SetProperty(ref m_slug, value, m_lock); }
        }

        public string CsrfToken
        {
            get
            {
                lock (m_lock)
                {
                    return m_csrfToken;
                }
            }

            set { SetProperty(ref m_csrfToken, value, m_lock); }
        }

        public JObject QueuedCheckoutResponse
        {
            get
            {
                lock (m_lock)
                {
                    return m_queuedCheckoutResponse;
                }
            }

            set { SetProperty(ref m_queuedCheckoutResponse, value, m_lock); }
        }

        public string CreditCardNumberFieldName
        {
            get
            {
                lock (m_lock)
                {
                    return m_creditCardNumberFieldName;
                }
            }

            set { SetProperty(ref m_creditCardNumberFieldName, value, m_lock); }
        }

        public string CreditCardMonthFieldName
        {
            get
            {
                lock (m_lock)
                {
                    return m_creditCardMonthFieldName;
                }
            }

            set { SetProperty(ref m_creditCardMonthFieldName, value, m_lock); }
        }

        public string CreditCardYearFieldName
        {
            get
            {
                lock (m_lock)
                {
                    return m_creditCardYearFieldName;
                }
            }

            set { SetProperty(ref m_creditCardYearFieldName, value, m_lock); }
        }

        public string CreditCardCVVFieldName
        {
            get
            {
                lock (m_lock)
                {
                    return m_creditCardCVVFieldName;
                }
            }

            set { SetProperty(ref m_creditCardCVVFieldName, value, m_lock); }
        }

        public int CheckoutTimestamp
        {
            get
            {
                lock (m_lock)
                {
                    return m_checkoutTimestamp;
                }
            }

            set { SetProperty(ref m_checkoutTimestamp, value, m_lock); }
        }

        public KeyValuePair<string, string> ExtraField
        {
            get
            {
                lock (m_lock)
                {
                    return m_extraField;
                }
            }

            set { SetProperty(ref m_extraField, value, m_lock); }
        }

        public string StoreId
        {
            get
            {
                lock (m_lock)
                {
                    return m_storeId;
                }
            }

            set { SetProperty(ref m_storeId, value, m_lock); }
        }

        public string Utf8Symbol
        {
            get
            {
                lock (m_lock)
                {
                    return m_utf8Symbol;
                }
            }

            set { SetProperty(ref m_utf8Symbol, value, m_lock); }
        }

        public string AuthenticityToken
        {
            get
            {
                lock (m_lock)
                {
                    return m_authenticityToken;
                }
            }

            set { SetProperty(ref m_authenticityToken, value, m_lock); }
        }

        #endregion

        #region Methods

        public void ResetCheckoutInfo()
        {
            CreditCardNumberFieldName = "credit_card[cnb]";
            CreditCardMonthFieldName = "credit_card[month]";
            CreditCardYearFieldName = "credit_card[year]";
            CreditCardCVVFieldName = "credit_card[vval]";
            CheckoutTimestamp = 0;
            ExtraField = new KeyValuePair<string, string>();
            StoreId = "";
            Utf8Symbol = "";
            AuthenticityToken = "";
            CsrfToken = "";
        }

        #endregion

        #region Fields

        private HttpClient m_client = null;
        private string m_slug = null;
        private string m_csrfToken = null;
        private JObject m_queuedCheckoutResponse = null;
        private string m_creditCardNumberFieldName = null;
        private string m_creditCardMonthFieldName = null;
        private string m_creditCardYearFieldName = null;
        private string m_creditCardCVVFieldName = null;
        private int m_checkoutTimestamp = 0;
        private KeyValuePair<string, string> m_extraField = new KeyValuePair<string, string>();
        private string m_storeId = null;
        private string m_utf8Symbol = null;
        private string m_authenticityToken = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public class SupremeProductStyleInfo
    {
        #region Properties

        public string Id { get; set; }
        public string Name { get; set; }
        public List<SupremeProductSizeInfo> Sizes { get; } = new List<SupremeProductSizeInfo>();

        #endregion
    }

    public class SupremeProductSizeInfo
    {
        #region MyRegion

        public string Name { get; set; }
        public string Code { get; set; }
        public int InSotck { get; set; }

        #endregion
    }
}

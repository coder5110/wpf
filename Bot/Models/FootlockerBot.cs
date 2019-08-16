using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bot.Helpers;
using Bot.Services;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Bot.Models
{
    public class FootlockerBot: ReleaseTaskBase
    {
        #region Constructors

        public FootlockerBot(Release release) : base(release)
        {
            Regex modelRegex = new Regex("model:([0-9]*)/");
            Regex skuRegex = new Regex("sku:([0-9A-Za-z]*)");

            Match modelMatch = modelRegex.Match(Release.ProductLink);
            Match skuMatch = skuRegex.Match(Release.ProductLink);

            m_model = modelMatch.Groups[1].Value;
            m_sku = skuMatch.Groups[1].Value;
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
                    UseProxy = false
                };
            }

            return new FoorlockerBotTaskContext()
            {
                Client = new HttpClient(clientHandler),
                BbDeviceId = BbDeviceIdGenerator.Generate()
            };
        }

        protected override bool CheckProduct(ReleaseProductInfo productInfo, Proxy proxy, CancellationToken cancelToken, out List<string> availableSizes)
        {
            bool ret = true;

            if (productInfo.ProductLink != null)
            {
                Logger.LogEvent(Log, "PRODUCT MONITOR", $"Get product by early link - {productInfo.ProductLink}");
                if (!CheckByEarlyLink(productInfo.ProductLink, proxy, cancelToken, out availableSizes))
                {
                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Product is not available at that moment");

                    ret = false;
                }
            }
            else
            {
                Logger.LogEvent(Log, "PRODUCT MONITOR", "Early link is not provided!");

                availableSizes = null;
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

            HttpClient client = new HttpClient(handler);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(earlyLink),
                Method = HttpMethod.Get,
            };
            request.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            
            string html = null;
            HttpStatusCode? statusCode = null;
            html = HttpHelper.GetStringSync(request, client, out statusCode, cancelToken);

            if (html != null)
            {
                html = HttpUtility.HtmlDecode(html);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode productSkuNode = doc.GetElementbyId("productSKU");

                if (productSkuNode != null)
                {
                    bool isSameSku = productSkuNode.InnerText == m_sku;

                    if (isSameSku)
                    {
                        Logger.LogEvent(Log, "PRODUCT MONITOR", "Product is detected");

                        Regex hotSkusRegex = new Regex($"hotSkus_{m_model}([ ]*[=][ ]*)\"([0-9A-Za-z,]*)\";");
                        Match hotSkusMatch = hotSkusRegex.Match(html);
                        
                        List<string> hotSkus = null;

                        try
                        {
                            hotSkus = hotSkusMatch.Groups[2]
                                .Value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                .ToList();
                        }
                        catch (Exception e)
                        {
                            
                        }

                        if (hotSkus != null)
                        {
                            if (hotSkus.Contains(m_sku))
                            {
                                Regex launchTimeRegex = new Regex($"launchTime_{m_model}([ ]*[=][ ]*)([0-9]*);");
                                Match launchTimeMatch = launchTimeRegex.Match(html);

                                Int64 launchTime = 0;
                                bool isLaunchTimeExist = true;

                                try
                                {
                                    launchTime = Int64.Parse(launchTimeMatch.Groups[2].Value);
                                }
                                catch (Exception e)
                                {
                                    isLaunchTimeExist = false;
                                }

                                if (isLaunchTimeExist)
                                {
                                    launchTime /= 1000;

                                    HttpRequestMessage serverTimeRequest = new HttpRequestMessage()
                                    {
                                        RequestUri = new Uri($"{m_footsite.BaseUrl}{m_serverTimePath}"),
                                        Method = HttpMethod.Get,
                                    };
                                    serverTimeRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                                    serverTimeRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                                    serverTimeRequest.Headers.TryAddWithoutValidation("Accept-Encoding",
                                        "gzip, deflate");

                                    string jsTime = HttpHelper.GetStringSync(serverTimeRequest, client, out statusCode,
                                        cancelToken);

                                    if (jsTime != null && statusCode == HttpStatusCode.OK)
                                    {
                                        Regex serverTimeRegex = new Regex("epoch([ ]*[=][ ]*)([0-9]*);");
                                        Match serverTimeMatch = serverTimeRegex.Match(jsTime);

                                        Int64 serverTime = 0;
                                        bool isServerTimeExist = true;

                                        try
                                        {
                                            serverTime = Int64.Parse(serverTimeMatch.Groups[2].Value);
                                        }
                                        catch (Exception e)
                                        {
                                            isServerTimeExist = false;
                                        }

                                        if (isServerTimeExist)
                                        {
                                            Int64 timeLeft = launchTime - serverTime;

                                            Logger.LogEvent(Log, "PRODUCT MONITOR",
                                                $"Time left before release: {timeLeft} seconds");

                                            while (!cancelToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)) && timeLeft > 0)
                                            {
                                                timeLeft--;

                                                Logger.RemoveLastEvent(Log);
                                                Logger.LogEvent(Log, "PRODUCT MONITOR",
                                                    $"Time left before release: {timeLeft} seconds");
                                            }

                                            ret = !cancelToken.IsCancellationRequested;
                                        }
                                        else
                                        {
                                            Logger.LogEvent(Log, "PRODUCT MONITOR", "Failed to get server time");
                                            ret = true;
                                        }

                                        ProductName = earlyLink;
                                    }
                                    else
                                    {
                                        Logger.LogEvent(Log, "PRODUCT MONITOR", "Failed to get server time");
                                        ret = false;
                                    }
                                }
                                else
                                {
                                    Logger.LogEvent(Log, "PRODUCT MONITOR", "Failed to get launch time");
                                    ret = true;
                                }
                            }
                            else
                            {
                                ret = true;
                            }
                        }
                        else
                        {
                            Logger.LogEvent(Log, "PRODUCT MONITOR", "Failed to get hot skus list");
                            ret = true;
                        }
                    }
                    else
                    {
                        Logger.LogEvent(Log, "PRODUCT MONITOR", "Product sku on page differs from sku at early link!");
                        ret = false;
                    }
                }
            }

            if (ret)
            {
                availableSizes = new List<string>();

                Regex stylesRegex = new Regex("styles([ ]*[=][ ]*)(.*)};");
                Match stylesMatch = stylesRegex.Match(html);
                
                try
                {
                    string stylesJson = stylesMatch.Groups[2].Value;
                    stylesJson += "}";

                    JObject styles = JObject.Parse(stylesJson);
                    JToken sizes = styles[m_sku][7];

                    foreach (JToken size in sizes)
                    {
                        if (size[3].Value<string>() != "Only In Store")
                        {
                            availableSizes.Add(ConvertFootlockerSizeToCommonSize(size[0].Value<string>()));
                        }
                    }

                }
                catch (Exception e)
                {
                    availableSizes = null;
                }
            }

            request.Dispose();
            client.Dispose();

            return ret;
        }

        protected override StepResult AddToCart(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            FoorlockerBotTaskContext cxt = context as FoorlockerBotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage productPageRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(task.ProductInfo.ProductLink),
                Method = HttpMethod.Get
            })
            { 
                productPageRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                productPageRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                productPageRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                
                html = HttpHelper.GetStringSync(productPageRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode formNode = doc.DocumentNode.SelectSingleNode(@"//form[@id='product_form']");

            if (formNode != null)
            {
                using (HttpRequestMessage addToCartRequest = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_addTocCartPath}"),
                    Method = HttpMethod.Post,
                })
                { 
                    addToCartRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                    addToCartRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                    addToCartRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    addToCartRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    addToCartRequest.Headers.TryAddWithoutValidation(":authority:", task.Footsite.Domain);
                    addToCartRequest.Headers.TryAddWithoutValidation(":method:", "POST");
                    addToCartRequest.Headers.TryAddWithoutValidation(":path:", m_addTocCartPath);
                    addToCartRequest.Headers.TryAddWithoutValidation(":scheme:", "https");
                    addToCartRequest.Headers.TryAddWithoutValidation("Referer", task.ProductInfo.ProductLink);
                    addToCartRequest.Headers.TryAddWithoutValidation("Origin", task.Footsite.BaseUrl);
                    addToCartRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    addToCartRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("storeCostOfGoods", "0.00"),
                        new KeyValuePair<string, string>("model", m_model),
                        new KeyValuePair<string, string>("requestKey", formNode.SelectSingleNode(@"//input[@name='requestKey']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("hasXYPromo", formNode.SelectSingleNode(@"//input[@name='hasXYPromo']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("sameDayDeliveryConfig", formNode.SelectSingleNode(@"//input[@name='sameDayDeliveryConfig']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("sku", formNode.SelectSingleNode(@"//input[@name='sku']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("the_model_nbr", formNode.SelectSingleNode(@"//input[@name='the_model_nbr']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("model_name", formNode.SelectSingleNode(@"//input[@name='model_name']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("skipISA", formNode.SelectSingleNode(@"//input[@name='skipISA']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("selectedPrice", formNode.SelectSingleNode(@"//input[@name='selectedPrice']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("qty", "1"),
                        new KeyValuePair<string, string>("size", ConvertCommonSizeToFootlockerSize(task.Size)),
                        new KeyValuePair<string, string>("fulfillmentType", formNode.SelectSingleNode(@"//input[@name='fulfillmentType']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("storeNumber", "00000"),
                        new KeyValuePair<string, string>("coreMetricsDo", formNode.SelectSingleNode(@"//input[@name='coreMetricsDo']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("coreMetricsCategory", formNode.SelectSingleNode(@"//input[@name='coreMetricsCategory']").GetAttributeValue("value", "")),
                        new KeyValuePair<string, string>("quantity", "1"),
                        new KeyValuePair<string, string>("inlineAddToCart", "1")
                    });

                    html = HttpHelper.GetStringSync(addToCartRequest, cxt.Client, out statusCode, cancelToken);
                }

                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
                
                if (html.Contains("Item added to cart"))
                {
                    res = StepResult.Ok;
                }
                else if (html.Contains("because it is out of stock"))
                {
                    res = StepResult.OutOfStock;
                }
                else
                {
                    res = StepResult.Failed;
                }
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        protected override StepResult CheckCart(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            FoorlockerBotTaskContext cxt = context as FoorlockerBotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{task.Footsite.BaseUrl}{(cxt.BillUrlForm == null ? m_shoppingCartPath : m_shoppingCartRetryPath)}"),
                Method = HttpMethod.Get,
            })
            {
                request.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Referer", task.ProductInfo.ProductLink);

                html = HttpHelper.GetStringSync(request, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode productNode = doc.DocumentNode.SelectSingleNode(@"//div[@class='attributes']");

            if (productNode != null && productNode.InnerText.Contains(m_sku))
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
            FoorlockerBotTaskContext cxt = context as FoorlockerBotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            using (HttpRequestMessage inventoryCheckRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_invertoryCheckPath}"),
                Method = HttpMethod.Get,
            })
            {
                inventoryCheckRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                inventoryCheckRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                inventoryCheckRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                html = HttpHelper.GetStringSync(inventoryCheckRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            using (HttpRequestMessage checkoutPageRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_checkoutPagePath}"),
                Method = HttpMethod.Get,
            })
            {
                checkoutPageRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                checkoutPageRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                checkoutPageRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                html = HttpHelper.GetStringSync(checkoutPageRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode formNode = doc.DocumentNode.SelectSingleNode(@"//form[@id='spcoForm']");

            if (formNode != null)
            {
                string billPhoneNumber = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PhoneNumber.Insert(3, " ").Insert(7, " ");
                string shipPhoneNumber = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.PhoneNumber.Insert(3, " ").Insert(7, " ");

                JObject vcdJObject = new JObject();
                vcdJObject["maxVisitedPane"] = "billAddressPane";
                vcdJObject["billMyAddressBookIndex"] = "-1";
                vcdJObject["addressNeedsVerification"] = true;
                vcdJObject["billFirstName"] = "";
                vcdJObject["billLastName"] = "";
                vcdJObject["billAddress1"] = "";
                vcdJObject["billAddress2"] = "";
                vcdJObject["billCity"] = "";
                vcdJObject["billState"] = "";
                vcdJObject["billProvince"] = "";
                vcdJObject["billPostalCode"] = "";
                vcdJObject["billHomePhone"] = "";
                vcdJObject["billMobilePhone"] = "";
                vcdJObject["billCountry"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.Country.ShortName;
                vcdJObject["billEmailAddress"] = "";
                vcdJObject["billConfirmEmail"] = "";
                vcdJObject["billAddrIsPhysical"] = true;
                vcdJObject["billSubscribePhone"] = false;
                vcdJObject["billAbbreviatedAddress"] = false;
                vcdJObject["shipUpdateDefaultAddress"] = false;
                vcdJObject["VIPNumber"] = "";
                vcdJObject["accountBillAddress"] = new JObject();
                vcdJObject["accountBillAddress"]["billMyAddressBookIndex"] = -1;
                vcdJObject["selectedBillAddress"] = new JObject();
                vcdJObject["billMyAddressBook"] = new JArray();
                vcdJObject["updateBillingForBML"] = false;
                vcdJObject["shipMyAddressBookIndex"] = -2;
                vcdJObject["shipContactID"] = "";
                vcdJObject["shipFirstName"] = "";
                vcdJObject["shipLastName"] = "";
                vcdJObject["shipAddress1"] = "";
                vcdJObject["shipAddress2"] = "";
                vcdJObject["shipCity"] = "";
                vcdJObject["shipState"] = "";
                vcdJObject["shipProvince"] = "";
                vcdJObject["shipPostalCode"] = "";
                vcdJObject["shipHomePhone"] = "";
                vcdJObject["shipMobilePhone"] = "";
                vcdJObject["shipCountry"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.Country.ShortName;
                vcdJObject["shipToStore"] = false;

                cxt.BillJsonForm = vcdJObject;

                cxt.BillUrlForm = new Dictionary<string, string>()
                {
                    { "verifiedCheckoutData", vcdJObject.ToString() },
                    { "requestKey", formNode.SelectSingleNode(@"//input[@name='requestKey']").GetAttributeValue("value", "") },
                    { "hbg", formNode.SelectSingleNode(@"//input[@name='hbg']").GetAttributeValue("value", "") },
                    { "addressBookEnabled", formNode.SelectSingleNode(@"//input[@name='addressBookEnabled']").GetAttributeValue("value", "") },
                    { "bb_device_id", cxt.BbDeviceId },
                    { "loginHeaderEmailAddress", formNode.SelectSingleNode(@"//input[@name='loginHeaderEmailAddress']").GetAttributeValue("value", "") },
                    { "loginHeaderPassword", formNode.SelectSingleNode(@"//input[@name='loginHeaderPassword']").GetAttributeValue("value", "") },
                    { "loginPaneNewEmailAddress", formNode.SelectSingleNode(@"//input[@name='loginPaneNewEmailAddress']").GetAttributeValue("value", "") },
                    { "loginPaneConfirmNewEmailAddress", formNode.SelectSingleNode(@"//input[@name='loginPaneConfirmNewEmailAddress']").GetAttributeValue("value", "") },
                    { "loginPaneEmailAddress", formNode.SelectSingleNode(@"//input[@name='loginPaneEmailAddress']").GetAttributeValue("value", "") },
                    { "loginPanePassword", formNode.SelectSingleNode(@"//input[@name='loginPanePassword']").GetAttributeValue("value", "") },
                    { "billAddressType", "new" },
                    { "billAddressInputType", formNode.SelectSingleNode(@"//input[@name='billAddressInputType']").GetAttributeValue("value", "") },
                    { "billAPOFPOCountry", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.Country.ShortName },
                    { "bill-country", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.Country.ShortName },
                    { "billMyAddressBookIndex", "-1" },
                    { "bill-fname", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.FirstName },
                    { "bill-lname", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.SecondName },
                    { "bill-address1", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddress1 },
                    { "bill-address2", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddressLine2 },
                    { "bill-postal", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PostalCode },
                    { "bill-city", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.City.ToUpper() },
                    { "billAPOFPO-region", "" },
                    { "bill-state", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.State.Abbreviation },
                    { "bill-province", "" },
                    { "billAPOFPO-state", "" },
                    { "billAPOFPO-postal", "" },
                    { "bill-tel", billPhoneNumber },
                    { "email", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.Email },
                    { "billConfirmEmail", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.Email },
                    { "shipAddressType", "new" },
                    { "shipAddressInputType", formNode.SelectSingleNode(@"//input[@name='shipAddressInputType']").GetAttributeValue("value", "") },
                    { "shipAPOFPOCountry", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.Country.ShortName },
                    { "ship-country", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.Country.ShortName },
                    { "shipMyAddressBookIndex", "-1" },
                    { "shipToStore", "false" },
                    { "ship-fname", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.FirstName },
                    { "ship-lname", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.SecondName },
                    { "ship-address1", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.StreetAddress1 },
                    { "ship-address2", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.StreetAddressLine2 },
                    { "ship-postal", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.PostalCode },
                    { "ship-city", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.City.ToUpper() },
                    { "shipAPOFPO-region", "" },
                    { "ship-state", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.State.Abbreviation },
                    { "ship-province", "" },
                    { "shipAPOFPO-state", "" },
                    { "shipAPOFPO-postal", "" },
                    { "ship-tel", shipPhoneNumber },
                    { "shipMethodCodeS2S", "" },
                    { "storePickupInputPostalCode", "" },
                    { "promoType", "" },
                    { "CPCOrSourceCode", "" },
                    { "CardNumber", "" },
                    { "CardExpireDateMM", "" },
                    { "CardExpireDateYY", "" },
                    { "CardCCV", "" },
                    { "payMethodPaneStoredType", "" },
                    { "payMethodPaneConfirmCardNumber", "" },
                    { "payMethodPaneStoredCCExpireMonth", "" },
                    { "payMethodPaneStoredCCExpireYear", "" },
                    { "payMethodPaneCardType", "" },
                    { "payMethodPaneCardNumber", "" },
                    { "payMethodPaneExpireMonth", "" },
                    { "payMethodPaneExpireYear", "" },
                    { "payMethodPaneCVV", "" },
                    { "payMethodPaneStoredCCCVV", "" },
                    { "giftCardCode_1", "" },
                    { "giftCardPin_1", "" },
                    { "fieldCount", "1" },
                    { "orderReviewPaneBillSubscribeEmail", "true" },
                    { "billMobilePhone", ""}
                };
                
                using (HttpRequestMessage billPaneRequest = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_billPanePath}"),
                    Method = HttpMethod.Post,
                })
                { 
                    billPaneRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                    billPaneRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                    billPaneRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    billPaneRequest.Headers.TryAddWithoutValidation(":authority:", task.Footsite.Domain);
                    billPaneRequest.Headers.TryAddWithoutValidation(":method:", "POST");
                    billPaneRequest.Headers.TryAddWithoutValidation(":path:", m_billPanePath);
                    billPaneRequest.Headers.TryAddWithoutValidation(":scheme:", "https");
                    billPaneRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    billPaneRequest.Headers.TryAddWithoutValidation("Referer", $"{task.Footsite.BaseUrl}{m_checkoutPagePath}");
                    billPaneRequest.Headers.TryAddWithoutValidation("Origin", task.Footsite.BaseUrl);
                    billPaneRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    billPaneRequest.Content = new FormUrlEncodedContent(cxt.BillUrlForm.ToList());

                    html = HttpHelper.GetStringSync(billPaneRequest, cxt.Client, out statusCode, cancelToken);
                }

                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
                
                html = html.Substring(2);

                JObject responseJObject = JObject.Parse(html);

                bool? isError = null;

                try
                {
                    isError = responseJObject["RESPONSEERROR"].Value<bool>();
                }
                catch (Exception e)
                {
                }

                if (isError.HasValue)
                {
                    if (!responseJObject["RESPONSEERROR"].Value<bool>())
                    {
                        cxt.BillPaneResponse = html;
                        res = StepResult.Ok;
                    }
                    else
                    {
                        res = StepResult.Failed;
                    }
                }
                else
                {
                    res = StepResult.Failed;
                }
            }
            else
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got unknown answer: {html}");
                res = StepResult.Failed;
            }

            return res;
        }
        
        protected override StepResult Paying(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            FoorlockerBotTaskContext cxt = context as FoorlockerBotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            JObject billResponseJObject = null;

            try
            {
                billResponseJObject = JObject.Parse(cxt.BillPaneResponse);
            }
            catch (Exception e)
            {
                billResponseJObject = null;
            }

            if (billResponseJObject != null)
            {
                cxt.ShipMethodJsonForm = new JObject(cxt.BillJsonForm);
                cxt.ShipMethodUrlForm = new Dictionary<string, string>(cxt.BillUrlForm);

                string billPhoneNumber = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PhoneNumber.Insert(3, " ").Insert(7, " ");
                string shipPhoneNumber = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.PhoneNumber.Insert(3, " ").Insert(7, " ");

                cxt.ShipMethodJsonForm["maxVisitedPane"] = "shipMethodPane";
                cxt.ShipMethodJsonForm["addressNeedsVerification"] = false;
                cxt.ShipMethodJsonForm["billFirstName"] =    task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.FirstName;
                cxt.ShipMethodJsonForm["billLastName"] =     task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.SecondName;
                cxt.ShipMethodJsonForm["billAddress1"] =     task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddress1;
                cxt.ShipMethodJsonForm["billAddress2"] =     task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddressLine2;
                cxt.ShipMethodJsonForm["billCity"] =         task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.City.ToUpper();
                cxt.ShipMethodJsonForm["billState"] =        task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.State.Abbreviation;
                cxt.ShipMethodJsonForm["billPostalCode"] =   task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PostalCode;
                cxt.ShipMethodJsonForm["billHomePhone"] =    billPhoneNumber;
                cxt.ShipMethodJsonForm["billCountry"] =      task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.Country.ShortName;
                cxt.ShipMethodJsonForm["billEmailAddress"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.Email;
                cxt.ShipMethodJsonForm["billConfirmEmail"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.Email;
                cxt.ShipMethodJsonForm["shipFirstName"] =    task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.FirstName;
                cxt.ShipMethodJsonForm["shipLastName"] =     task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.SecondName;
                cxt.ShipMethodJsonForm["shipAddress1"] =     task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.StreetAddress1;
                cxt.ShipMethodJsonForm["shipAddress2"] =     task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.StreetAddressLine2;
                cxt.ShipMethodJsonForm["shipCity"] =         task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.City.ToUpper();
                cxt.ShipMethodJsonForm["shipState"] =        task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.State.Abbreviation;
                cxt.ShipMethodJsonForm["shipPostalCode"] =   task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.PostalCode; 
                cxt.ShipMethodJsonForm["shipHomePhone"] =    shipPhoneNumber;
                cxt.ShipMethodJsonForm["shipCountry"] =      task.Profile.ReleaseCheckoutProfile.CheckoutProfile.ShippingAddress.Country.ShortName;

                bool success = true;

                try
                {
                    cxt.ShipMethodJsonForm["shipHash"] = billResponseJObject["SHIPPANE"]["SHIPHASH"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMultiple"] = false;
                    cxt.ShipMethodJsonForm["isShipToStoreEligibleCheckout"] = true;
                    cxt.ShipMethodJsonForm["isMultipleAddressEligible"] = false;
                    cxt.ShipMethodJsonForm["shipAbbreviatedAddress"] = false;
                    cxt.ShipMethodJsonForm["selectedStore"] = new JObject();
                    cxt.ShipMethodJsonForm["accountShipAddress"] = new JObject();
                    cxt.ShipMethodJsonForm["accountShipAddress"]["shipMyAddressBookIndex"] = -1;
                    cxt.ShipMethodJsonForm["selectedShipAddress"] = new JObject();
                    cxt.ShipMethodJsonForm["shipMyAddressBook"] = new JArray();
                    cxt.ShipMethodJsonForm["shipMethodCode"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODCODE"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodName"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODNAME"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodPrice"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDPRICE"].Value<string>();
                    cxt.ShipMethodJsonForm["shipDeliveryEstimate"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDDELIVERYESTIMATE"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodCodeSDD"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODCODESDD"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodNameSDD"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODNAMESDD"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodPriceSDD"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDPRICESDD"].Value<string>();
                    cxt.ShipMethodJsonForm["shipDeliveryEstimateSDD"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDDELIVERYESTIMATESDD"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodCodeS2S"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODCODES2S"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodNameS2S"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODNAMES2S"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodPriceS2S"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDPRICES2S"].Value<string>();
                    cxt.ShipMethodJsonForm["shipDeliveryEstimateS2S"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDDELIVERYESTIMATES2S"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodCodeSFS"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODCODESFS"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodNameSFS"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODNAMESFS"].Value<string>();
                    cxt.ShipMethodJsonForm["shipMethodPriceSFS"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDPRICESFS"].Value<string>();
                    cxt.ShipMethodJsonForm["shipDeliveryEstimateSFS"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDDELIVERYESTIMATESFS"].Value<string>();
                    cxt.ShipMethodJsonForm["homeDeliveryPrice"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDHOMEDELIVERYPRICE"].Value<string>();
                    cxt.ShipMethodJsonForm["overallHomeDeliveryPrice"] = billResponseJObject["SHIPMETHODPANE"]["OVERALLHOMEDELIVERYPRICE"].Value<string>();
                    cxt.ShipMethodJsonForm["aggregatedDeliveryPrice"] = billResponseJObject["SHIPMETHODPANE"]["AGGREGATEDDELIVERYPRICE"].Value<string>();
                    cxt.ShipMethodJsonForm["aggregatedDeliveryLabel"] = billResponseJObject["SHIPMETHODPANE"]["AGGREGATEDDELIVERYLABEL"].Value<string>();
                    cxt.ShipMethodJsonForm["showGiftBoxOption"] = billResponseJObject["SHIPMETHODPANE"]["SHOWGIFTBOXOPTION"].Value<bool>();
                    cxt.ShipMethodJsonForm["addGiftBox"] = billResponseJObject["SHIPMETHODPANE"]["ADDGIFTBOX"].Value<bool>();
                    cxt.ShipMethodJsonForm["giftBoxPrice"] = billResponseJObject["SHIPMETHODPANE"]["GIFTBOXPRICE"].Value<string>();
                    cxt.ShipMethodJsonForm["useGiftReceipt"] = billResponseJObject["SHIPMETHODPANE"]["USEGIFTRECEIPT"].Value<bool>();
                    cxt.ShipMethodJsonForm["showGiftOptions"] = billResponseJObject["SHIPMETHODPANE"]["SHOWGIFTOPTIONS"].Value<bool>();
                    cxt.ShipMethodJsonForm["loyaltyMessageText"] = billResponseJObject["SHIPMETHODPANE"]["LOYALTYMESSAGETEXT"].Value<bool>();
                    cxt.ShipMethodJsonForm["showLoyaltyRenewalMessage"] = billResponseJObject["SHIPMETHODPANE"]["SHOWLOYALTYRENEWALMESSAGE"].Value<bool>();
                    cxt.ShipMethodJsonForm["pickupPersonFirstName"] = billResponseJObject["SHIPMETHODPANE"]["PICKUPPERSONFIRSTNAME"].Value<string>();
                    cxt.ShipMethodJsonForm["pickupPersonLastName"] = billResponseJObject["SHIPMETHODPANE"]["PICKUPPERSONLASTNAME"].Value<string>();
                }
                catch (Exception e)
                {
                    success = false;
                }

                if (success)
                {
                    cxt.ShipMethodUrlForm["verifiedCheckoutData"] = cxt.ShipMethodJsonForm.ToString();
                    cxt.ShipMethodUrlForm["requestKey"] = billResponseJObject["REQUESTKEY"].Value<string>();
                    cxt.ShipMethodUrlForm["hbg"] = billResponseJObject["hbg"].Value<string>();
                    cxt.ShipMethodUrlForm["shipMethodCode"] = billResponseJObject["SHIPMETHODPANE"]["SELECTEDMETHODCODE"].Value<string>();

                    using (HttpRequestMessage shipMethodPaneRequest = new HttpRequestMessage()
                    {
                        RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_shipMethodPath}"),
                        Method = HttpMethod.Post,
                    })
                    { 
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation(":authority:", task.Footsite.Domain);
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation(":method:", "POST");
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation(":path:", m_shipMethodPath);
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation(":scheme:", "https");
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("Referer", $"{task.Footsite.BaseUrl}{m_checkoutPagePath}");
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("Origin", task.Footsite.BaseUrl);
                        shipMethodPaneRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                        shipMethodPaneRequest.Content = new FormUrlEncodedContent(cxt.ShipMethodUrlForm.ToList());

                        html = HttpHelper.GetStringSync(shipMethodPaneRequest, cxt.Client, out statusCode, cancelToken);
                    }

                    if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                    if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
                    
                    html = html.Substring(2);

                    JObject responseJObject = JObject.Parse(html);

                    bool? isError = null;

                    try
                    {
                        isError = responseJObject["RESPONSEERROR"].Value<bool>();
                    }
                    catch (Exception e)
                    {
                    }

                    if (isError.HasValue)
                    {
                        if (!isError.Value)
                        {
                            cxt.ShipMethodPaneResponse = html;
                            CheckoutStep validatePayment = new CheckoutStep(ValidatePayment, "Validating payment",
                                CheckoutTaskState.Undefined);
                            CheckoutStep review = new CheckoutStep(Review, "Reviewing", CheckoutTaskState.Undefined);

                            TimeSpan executionTime;

                            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Delay in checkout {task.Footsite.Settings.DelayInCheckout} ms");

                            cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(task.Footsite.Settings.DelayInCheckout));
                            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;

                            res = validatePayment.Run(context, task,
                                TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime,
                                cancelToken);

                            if (res == StepResult.Ok)
                            {
                                res = review.Run(context, task,
                                    TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime,
                                    cancelToken);
                            }
                        }
                        else
                        {
                            res = StepResult.Failed;
                        }
                    }
                    else
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got unknown answer: {html}");
                        res = StepResult.Failed;
                    }
                }
                else
                {
                    res = StepResult.Failed;
                }

            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        private StepResult ValidatePayment(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            FoorlockerBotTaskContext cxt = context as FoorlockerBotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            JObject shipResponseJObject = null;

            try
            {
                shipResponseJObject = JObject.Parse(cxt.ShipMethodPaneResponse);
            }
            catch (Exception e)
            {
                shipResponseJObject = null;
            }

            if (shipResponseJObject != null)
            {
                cxt.PaymentJsonForm = new JObject(cxt.ShipMethodJsonForm);
                cxt.PaymentUrlForm = new Dictionary<string, string>(cxt.ShipMethodUrlForm);

                cxt.PaymentJsonForm["maxVisitedPane"] = "promoCodePane";

                bool success = true;

                try
                {
                    cxt.PaymentJsonForm["preferredLanguage"] = shipResponseJObject["PAYMENTMETHODPANE"]["PREFERREDLANGUAGE"].Value<string>();
                    cxt.PaymentJsonForm["advanceToConfirm"] = shipResponseJObject["PAYMENTMETHODPANE"]["ADVANCETOCONFIRM"].Value<bool>();
                    cxt.PaymentJsonForm["payType"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYTYPE"].Value<string>();
                    cxt.PaymentJsonForm["payPalToken"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYPALTOKEN"].Value<string>();
                    cxt.PaymentJsonForm["payPalInContext"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYPALINCONTEXT"].Value<bool>();
                    cxt.PaymentJsonForm["payPalMerchantId"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYPALMERCHANTID"].Value<string>();
                    cxt.PaymentJsonForm["payPalStage"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYPALSTAGE"].Value<string>();
                    cxt.PaymentJsonForm["payPalPaymentAllowed"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYPALPAYMENTALLOWED"].Value<bool>();
                    cxt.PaymentJsonForm["payMethodPaneExpireMonth"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEEXPIREMONTH"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPaneExpireYear"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEEXPIREYEAR"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPaneCardNumber"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECARDNUMBER"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPaneCardType"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECARDTYPE"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPaneLastFour"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANELASTFOUR"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPanePurchaseOrder"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEPURCHASEORDER"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPanePurchaseOrderNewCustomer"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEPURCHASEORDERNEWCUSTOMER"].Value<string>();
                    cxt.PaymentJsonForm["payMethodPaneCVV"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECVV"].Value<string>();
                    cxt.PaymentJsonForm["creditcardPaymentAllowed"] = shipResponseJObject["PAYMENTMETHODPANE"]["CREDITCARDPAYMENTALLOWED"].Value<bool>();
                    cxt.PaymentJsonForm["billMeLaterStage"] = shipResponseJObject["PAYMENTMETHODPANE"]["BILLMELATERSTAGE"].Value<string>();
                    cxt.PaymentJsonForm["BMLPaymentAllowed"] = shipResponseJObject["PAYMENTMETHODPANE"]["BMLPAYMENTALLOWED"].Value<bool>();
                    cxt.PaymentJsonForm["displayBMLPromotion"] = shipResponseJObject["PAYMENTMETHODPANE"]["DISPLAYBMLPROMOTION"].Value<bool>();
                    cxt.PaymentJsonForm["POPaymentAllowed"] = shipResponseJObject["PAYMENTMETHODPANE"]["POPAYMENTALLOWED"].Value<bool>();
                    cxt.PaymentJsonForm["promoType"] = "";
                    cxt.PaymentJsonForm["promoCode"] = "";
                    cxt.PaymentJsonForm["sourceCode"] = shipResponseJObject["PAYMENTMETHODPANE"]["SOURCECODE"].Value<string>();
                    cxt.PaymentJsonForm["sourceCodeDescription"] = shipResponseJObject["PAYMENTMETHODPANE"]["SOURCECODEDESCRIPTION"].Value<string>();
                    cxt.PaymentJsonForm["sourceCodeCartDisplayText"] = shipResponseJObject["PAYMENTMETHODPANE"]["SOURCECODECARTDISPLAYTEXT"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDCODE1"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDCODE1"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDPIN1"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDPIN1"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDUSED1"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDUSED1"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDCODE2"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDCODE2"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDPIN2"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDPIN2"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDUSED2"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDUSED2"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDCODE3"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDCODE3"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDPIN3"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDPIN3"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDUSED3"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDUSED3"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDCODE4"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDCODE4"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDPIN4"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDPIN4"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDUSED4"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDUSED4"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDCODE5"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDCODE5"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDPIN5"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDPIN5"].Value<string>();
                    cxt.PaymentJsonForm["GIFTCARDUSED5"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDUSED5"].Value<string>();
                    cxt.PaymentJsonForm["giftCardsEmpty"] = shipResponseJObject["PAYMENTMETHODPANE"]["GIFTCARDSEMPTY"].Value<bool>();
                    cxt.PaymentJsonForm["sourceCodesEmpty"] = shipResponseJObject["PAYMENTMETHODPANE"]["SOURCECODESEMPTY"].Value<bool>();
                    cxt.PaymentJsonForm["ContingencyQueue"] = shipResponseJObject["PAYMENTMETHODPANE"]["CONTINGENCYQUEUE"].Value<string>();
                    cxt.PaymentJsonForm["lgr"] = shipResponseJObject["PAYMENTMETHODPANE"]["LGR"].Value<string>();
                    cxt.PaymentJsonForm["displayEmailOptIn"] = shipResponseJObject["PAYMENTMETHODPANE"]["DISPLAYEMAILOPTIN"].Value<bool>();
                    cxt.PaymentJsonForm["billSubscribeEmail"] = shipResponseJObject["PAYMENTMETHODPANE"]["BILLSUBSCRIBEEMAIL"].Value<bool>();
                    cxt.PaymentJsonForm["billReceiveNewsletter"] = shipResponseJObject["PAYMENTMETHODPANE"]["BILLRECEIVENEWSLETTER"].Value<bool>();
                    cxt.PaymentJsonForm["billFavoriteTeams"] = shipResponseJObject["PAYMENTMETHODPANE"]["BILLFAVORITETEAMS"].Value<bool>();
                    cxt.PaymentJsonForm["paypalEmailAddress"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYPALEMAILADDRESS"].Value<string>();
                    cxt.PaymentJsonForm["displaySheerIdIframe"] = false;
                    cxt.PaymentJsonForm["displayCmsEntry"] = "";
                    cxt.PaymentJsonForm["payMethodPaneUserGotStoredCC"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEUSERGOTSTOREDCC"].Value<bool>();
                    cxt.PaymentJsonForm["payMethodPaneHasStoredCC"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEHASSTOREDCC"].Value<bool>();
                    cxt.PaymentJsonForm["payMethodPaneUsedStoredCC"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEUSEDSTOREDCC"].Value<bool>();
                    cxt.PaymentJsonForm["payMethodPaneSavedNewCC"] = shipResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANESAVEDNEWCC"].Value<bool>();
                    cxt.PaymentJsonForm["selectedCreditCard"] = new JObject();
                    cxt.PaymentJsonForm["selectedCreditCard"]["payMethodPaneHasCVV"] = true;
                    cxt.PaymentJsonForm["payMethodPaneHasCVV"] = true;
                }
                catch (Exception e)
                {
                    success = false;
                }

                if (success)
                {
                    cxt.PaymentUrlForm["verifiedCheckoutData"] = cxt.PaymentJsonForm.ToString();
                    cxt.PaymentUrlForm["requestKey"] = shipResponseJObject["REQUESTKEY"].Value<string>();
                    cxt.PaymentUrlForm["hbg"] = shipResponseJObject["hbg"].Value<string>();
                    cxt.PaymentUrlForm["payMethodPanePayType"] = "CC";
                    cxt.PaymentUrlForm["payMethodPanestoredCCCardNumber"] = "CC";
                    cxt.PaymentUrlForm["CardNumber"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Number;
                    cxt.PaymentUrlForm["CardExpireDateMM"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("MM");
                    cxt.PaymentUrlForm["CardExpireDateYY"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("yy");
                    cxt.PaymentUrlForm["CardCCV"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.CVS.ToString();
                    cxt.PaymentUrlForm["payMethodPaneCardType"] = CreditCardTypeToFootlockerTypeName(task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Type.Code);
                    cxt.PaymentUrlForm["payMethodPaneCardNumber"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Number;
                    cxt.PaymentUrlForm["payMethodPaneExpireMonth"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("MM");
                    cxt.PaymentUrlForm["payMethodPaneExpireYear"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("yy");
                    cxt.PaymentUrlForm["payMethodPaneCVV"] = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.CVS.ToString();

                    using (HttpRequestMessage paymentPaneRequest = new HttpRequestMessage()
                    {
                        RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_paymentPath}"),
                        Method = HttpMethod.Post,
                    })
                    { 
                        paymentPaneRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                        paymentPaneRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                        paymentPaneRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                        paymentPaneRequest.Headers.TryAddWithoutValidation(":authority:", task.Footsite.Domain);
                        paymentPaneRequest.Headers.TryAddWithoutValidation(":method:", "POST");
                        paymentPaneRequest.Headers.TryAddWithoutValidation(":path:", m_paymentPath);
                        paymentPaneRequest.Headers.TryAddWithoutValidation(":scheme:", "https");
                        paymentPaneRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                        paymentPaneRequest.Headers.TryAddWithoutValidation("Referer", $"{task.Footsite.BaseUrl}{m_checkoutPagePath}");
                        paymentPaneRequest.Headers.TryAddWithoutValidation("Origin", task.Footsite.BaseUrl);
                        paymentPaneRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                        paymentPaneRequest.Content = new FormUrlEncodedContent(cxt.PaymentUrlForm.ToList());

                        html = HttpHelper.GetStringSync(paymentPaneRequest, cxt.Client, out statusCode, cancelToken);
                    }

                    if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                    if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
                    
                    html = html.Substring(2);

                    JObject responseJObject = JObject.Parse(html);

                    bool? isError = null;

                    try
                    {
                        isError = responseJObject["RESPONSEERROR"].Value<bool>();
                    }
                    catch (Exception e)
                    {
                        ;
                    }

                    if (isError.HasValue)
                    {
                        if (!isError.Value)
                        {
                            cxt.PaymentPaneResponse = html;
                            res = StepResult.Ok;
                        }
                        else
                        {
                            res = StepResult.Failed;
                        }
                    }
                    else
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got unknown answer: {html}");
                        res = StepResult.Failed;
                    }
                }
                else
                {
                    res = StepResult.Failed;
                }
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        private StepResult Review(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            FoorlockerBotTaskContext cxt = context as FoorlockerBotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            JObject paymentResponseJObject = null;

            try
            {
                paymentResponseJObject = JObject.Parse(cxt.PaymentPaneResponse);
            }
            catch (Exception e)
            {
                paymentResponseJObject = null;
            }

            if (paymentResponseJObject != null)
            {
                cxt.ReviewJsonForm = new JObject(cxt.PaymentJsonForm);
                cxt.ReviewUrlForm = new Dictionary<string, string>(cxt.PaymentUrlForm);

                cxt.ReviewJsonForm["maxVisitedPane"] = "orderReviewPane";

                bool success = true;

                try
                {
                    cxt.ReviewJsonForm["payType"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYTYPE"].Value<string>();
                    cxt.ReviewJsonForm["payMethodPaneExpireMonth"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEEXPIREMONTH"].Value<string>().Replace(" ", "");
                    cxt.ReviewJsonForm["payMethodPaneExpireYear"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEEXPIREYEAR"].Value<string>().Replace(" ", "");
                    cxt.ReviewJsonForm["payMethodPaneCardNumber"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECARDNUMBER"].Value<string>().Replace(" ", "");
                    cxt.ReviewJsonForm["payMethodPaneCardType"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECARDTYPE"].Value<string>();
                    cxt.ReviewJsonForm["payMethodPaneLastFour"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANELASTFOUR"].Value<string>().Replace(" ", "");
                    cxt.ReviewJsonForm["payMethodPanePayMethodName"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEPAYMETHODNAME"].Value<string>();
                    cxt.ReviewJsonForm["payMethodPaneCVV"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECVV"].Value<string>().Replace(" ", "");
                    cxt.ReviewJsonForm["emptyCart"] = paymentResponseJObject["PAYMENTMETHODPANE"]["EMPTYCART"].Value<bool>();
                    cxt.ReviewJsonForm["lgr"] = paymentResponseJObject["PAYMENTMETHODPANE"]["LGR"].Value<string>();
                    cxt.ReviewJsonForm["displayEmailOptIn"] = paymentResponseJObject["PAYMENTMETHODPANE"]["DISPLAYEMAILOPTIN"].Value<bool>();
                    cxt.ReviewJsonForm["billSubscribeEmail"] = paymentResponseJObject["PAYMENTMETHODPANE"]["BILLSUBSCRIBEEMAIL"].Value<bool>();
                    cxt.ReviewJsonForm["billReceiveNewsletter"] = paymentResponseJObject["PAYMENTMETHODPANE"]["BILLRECEIVENEWSLETTER"].Value<bool>();
                    cxt.ReviewJsonForm["billFavoriteTeams"] = paymentResponseJObject["PAYMENTMETHODPANE"]["BILLFAVORITETEAMS"].Value<bool>();
                    cxt.ReviewJsonForm["paypalEmailAddress"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYPALEMAILADDRESS"].Value<string>();
                    cxt.ReviewJsonForm["displaySheerIdIframe"] = false;
                    cxt.ReviewJsonForm["displayCmsEntry"] = "";
                    cxt.ReviewJsonForm["payMethodPaneUserGotStoredCC"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEUSERGOTSTOREDCC"].Value<bool>();
                    cxt.ReviewJsonForm["payMethodPaneHasStoredCC"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEHASSTOREDCC"].Value<bool>();
                    cxt.ReviewJsonForm["payMethodPaneUsedStoredCC"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANEUSEDSTOREDCC"].Value<bool>();
                    cxt.ReviewJsonForm["payMethodPaneSavedNewCC"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANESAVEDNEWCC"].Value<bool>();
                    cxt.ReviewJsonForm["selectedCreditCard"] = new JObject();
                    cxt.ReviewJsonForm["payMethodPaneCVVAF"] = paymentResponseJObject["PAYMENTMETHODPANE"]["PAYMETHODPANECVVAF"].Value<string>().Replace(" ", "");
                }
                catch (Exception e)
                {
                    success = false;
                }

                if (success)
                {
                    cxt.ReviewUrlForm["verifiedCheckoutData"] = cxt.ReviewJsonForm.ToString();
                    cxt.ReviewUrlForm["requestKey"] = paymentResponseJObject["REQUESTKEY"].Value<string>();
                    cxt.ReviewUrlForm["hbg"] = paymentResponseJObject["hbg"].Value<string>();

                    using (HttpRequestMessage reviewPaneRequest = new HttpRequestMessage()
                    {
                        RequestUri = new Uri($"{task.Footsite.BaseUrl}{m_reviewPath}"),
                        Method = HttpMethod.Post,
                    })
                    {
                        reviewPaneRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                        reviewPaneRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                        reviewPaneRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                        reviewPaneRequest.Headers.TryAddWithoutValidation(":authority:", task.Footsite.Domain);
                        reviewPaneRequest.Headers.TryAddWithoutValidation(":method:", "POST");
                        reviewPaneRequest.Headers.TryAddWithoutValidation(":path:", m_reviewPath);
                        reviewPaneRequest.Headers.TryAddWithoutValidation(":scheme:", "https");
                        reviewPaneRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                        reviewPaneRequest.Headers.TryAddWithoutValidation("Referer", $"{task.Footsite.BaseUrl}{m_checkoutPagePath}");
                        reviewPaneRequest.Headers.TryAddWithoutValidation("Origin", task.Footsite.BaseUrl);
                        reviewPaneRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                        reviewPaneRequest.Content = new FormUrlEncodedContent(cxt.ReviewUrlForm.ToList());

                        html = HttpHelper.GetStringSync(reviewPaneRequest, cxt.Client, out statusCode, cancelToken);
                    }

                    if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
                    if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;
                    
                    html = html.Substring(2);

                    JObject responseJObject = JObject.Parse(html);

                    bool? isError = null;

                    try
                    {
                        isError = responseJObject["RESPONSEERROR"].Value<bool>();
                    }
                    catch (Exception e)
                    {
                        ;
                    }

                    if (isError.HasValue)
                    {
                        if (!isError.Value)
                        {
                            cxt.ReviewPaneResponse = html;
                            res = StepResult.Ok;
                        }
                        else
                        {
                            res = StepResult.Failed;
                        }
                    }
                    else
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got unknown answer: {html}");
                        res = StepResult.Failed;
                    }
                }
                else
                {
                    res = StepResult.Failed;
                }
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        private StepResult CheckResult(string html, HttpStatusCode? statusCode, bool htmlIsRequired = true)
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

        private string ConvertCommonSizeToFootlockerSize(string size)
        {
            string res = null;

            res = decimal.Parse(size, CultureInfo.InvariantCulture).ToString("00.0", CultureInfo.InvariantCulture);

            return res;
        }

        private string ConvertFootlockerSizeToCommonSize(string size)
        {
            string res = null;

            res = decimal.Parse(size, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

            return res;
        }

        private string CreditCardTypeToFootlockerTypeName(PayCardTypeCode code)
        {
            string res = null;

            if (code == PayCardTypeCode.Visa)
            {
                return "visa";
            }
            else if (code == PayCardTypeCode.MasterCard)
            {
                return "mc";
            }
            else if (code == PayCardTypeCode.Amex)
            {
                return "amex";
            }
            else if (code == PayCardTypeCode.Discover)
            {
                return "disc";
            }
            else if (code == PayCardTypeCode.JCB)
            {
                return "jcb";
            }

            return res;
        }

        #endregion

        #region Fields

        private Footsite m_footsite = FootsiteCollection.SitesDictionary[FootsiteType.Footlocker];
        private string m_model = null;
        private string m_sku = null;
        private string m_addTocCartPath = @"/catalog/miniAddToCart.cfm?secure=1&";
        private string m_shoppingCartPath = @"/shoppingcart/default.cfm?sku=";
        private string m_shoppingCartRetryPath = @"/shoppingcart/?sessionExpired=true";
        private string m_invertoryCheckPath = @"/checkout/inventoryCheck.cfm";
        private string m_checkoutPagePath = @"/checkout";
        private string m_billPanePath = @"/checkout/eventGateway?&method=validateBillPane";
        private string m_serverTimePath = @"/images/common/js/timevariable.cfm?variable=curTime";
        private string m_shipMethodPath = @"/checkout/eventGateway?&method=validateShipMethodPane";
        private string m_paymentPath = @"/checkout/eventGateway?&method=validatePaymentMethodPane";
        private string m_reviewPath = @"/checkout/eventGateway?&method=validateReviewPane";

        #endregion
    }

    public class FoorlockerBotTaskContext : BindableObject, ICheckoutTaskContext
    {
        #region Constructors



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

        public Dictionary<string, string> BillUrlForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_billUrlForm;
                }
            }

            set { SetProperty(ref m_billUrlForm, value, m_lock); }
        }

        public JObject BillJsonForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_billJsonForm;
                }
            }

            set { SetProperty(ref m_billJsonForm, value, m_lock); }
        }

        public string BillPaneResponse
        {
            get
            {
                lock (m_lock)
                {
                    return m_billPaneResponse;
                }
            }

            set { SetProperty(ref m_billPaneResponse, value, m_lock); }
        }

        public Dictionary<string, string> ShipMethodUrlForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_shipMethodUrlForm;
                }
            }

            set { SetProperty(ref m_shipMethodUrlForm, value, m_lock); }
        }

        public JObject ShipMethodJsonForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_shipMethodJsonForm;
                }
            }

            set { SetProperty(ref m_shipMethodJsonForm, value, m_lock); }
        }

        public string ShipMethodPaneResponse
        {
            get
            {
                lock (m_lock)
                {
                    return m_shipMethodPaneResponse;
                }
            }

            set { SetProperty(ref m_shipMethodPaneResponse, value, m_lock); }
        }

        public Dictionary<string, string> PaymentUrlForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_paymentUrlForm;
                }
            }

            set { SetProperty(ref m_paymentUrlForm, value, m_lock); }
        }

        public JObject PaymentJsonForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_paymentJsonForm;
                }
            }

            set { SetProperty(ref m_paymentJsonForm, value, m_lock); }
        }

        public string PaymentPaneResponse
        {
            get
            {
                lock (m_lock)
                {
                    return m_paymentPaneResponse;
                }
            }

            set { SetProperty(ref m_paymentPaneResponse, value, m_lock); }
        }

        public string BbDeviceId
        {
            get
            {
                lock (m_lock)
                {
                    return m_bbDeviceId;
                }
            }

            set { SetProperty(ref m_bbDeviceId, value, m_lock); }
        }

        public Dictionary<string, string> ReviewUrlForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_reviewUrlForm;
                }
            }

            set { SetProperty(ref m_reviewUrlForm, value, m_lock); }
        }

        public JObject ReviewJsonForm
        {
            get
            {
                lock (m_lock)
                {
                    return m_reviewJsonForm;
                }
            }

            set { SetProperty(ref m_reviewJsonForm, value, m_lock); }
        }

        public string ReviewPaneResponse
        {
            get
            {
                lock (m_lock)
                {
                    return m_reviewPaneResponse;
                }
            }

            set { SetProperty(ref m_reviewPaneResponse, value, m_lock); }
        }

        public bool IsRetryATC
        {
            get
            {
                lock (m_lock)
                {
                    return m_isRetryATC;
                }
            }

            set { SetProperty(ref m_isRetryATC, value, m_lock); }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            m_client?.Dispose();
        }

        #endregion

        #region Fields

        private HttpClient m_client = null;
        private string m_bbDeviceId = "";
        private string m_checkoutPageHtml = null;
        private Dictionary<string, string> m_billUrlForm = null;
        private JObject m_billJsonForm = null;
        private string m_billPaneResponse = null;
        private Dictionary<string, string> m_shipMethodUrlForm = null;
        private JObject m_shipMethodJsonForm = null;
        private string m_shipMethodPaneResponse = null;
        private Dictionary<string, string> m_paymentUrlForm = null;
        private JObject m_paymentJsonForm = null;
        private string m_paymentPaneResponse = null;
        private Dictionary<string, string> m_reviewUrlForm = null;
        private JObject m_reviewJsonForm = null;
        private string m_reviewPaneResponse = null;
        private bool m_isRetryATC = false;
        private readonly object m_lock = new object();

        #endregion
    }
}

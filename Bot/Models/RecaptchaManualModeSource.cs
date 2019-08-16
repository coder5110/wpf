using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Bot.Helpers;
using CefSharp;
using CefSharp.Wpf;

namespace Bot.Models
{
    public class RecaptchaManualModeSource: BindableObject, ICaptchaSolutionSource
    {
        #region Constructors

        public RecaptchaManualModeSource()
        {
            m_requestHandler = new RequestHandler();
            m_requestHandler.TokenFound += (s, e) =>
            {
                RecaptchaManualSourceTask task = m_currentTask;

                lock (m_lock)
                {
                    m_currentTask = null;
                }

                task.Solution = e.Token;
            };
        }

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get
            {
                lock (m_lock)
                {
                    return m_isEnabled;
                }
            }

            protected set
            {
                lock (m_lock)
                {
                    m_isEnabled = value;
                }
            }
        }

        public CancellationToken CancelToken
        {
            get { return new CancellationToken(); }

            set { }
        }

        public ICaptchaSolutionSourceTask CurrentTask
        {
            get
            {
                lock (m_lock)
                {
                    return m_currentTask;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_currentTask = value as RecaptchaManualSourceTask;
                }
            }
        }

        public IWpfWebBrowser Browser
        {
            get
            {
                lock (m_lock)
                {
                    return m_browser;
                }
            }

            set
            {
                if (value != null)
                {
                    lock (m_lock)
                    {
                        ChromiumWebBrowser browser = value as ChromiumWebBrowser;

                        SiteKey = null;
                        SitePath = null;

                        browser.RequestHandler = m_requestHandler;

                        browser.IsBrowserInitializedChanged += (s, e) =>
                        {
                            if (browser.IsBrowserInitialized)
                            {
                                IsEnabled = true;

                                Reload();
                            }
                        };

                        browser.FrameLoadEnd += (s, e) =>
                        {
                            if (e.Frame.IsMain)
                            {
                                Reset();
                            }
                        };

                        m_browser = browser;

                        if (m_browser.IsBrowserInitialized)
                        {
                            IsEnabled = true;
                            Reload();
                        }
                    }
                }
            }
        }
        
        public string SiteKey
        {
            get
            {
                lock (m_lock)
                {
                    return m_siteKey;
                }
            }

            protected set
            {
                lock (m_lock)
                {
                    m_siteKey = value;
                }
            }
        }

        public string SitePath
        {
            get
            {
                lock (m_lock)
                {
                    return m_sitePath;
                }
            }

            protected set
            {
                lock (m_lock)
                {
                    m_sitePath = value;
                }
            }
        }

        #endregion

        #region Methods

        public ICaptchaSolutionSourceTask GetSolution(object parameter)
        {
            RecaptchaManualSourceTask task = new RecaptchaManualSourceTask()
            {
                Parameter = parameter as RecaptchaParameter
            };

            if (task.Parameter == null)
            {
                throw new ArgumentException("Parameter has wrong type or Null");
            }

            CurrentTask = task;

            if (IsEnabled)
            {
                Reload();
            }

            return task;
        }

        public void Reload()
        {
            bool reset = false;

            lock (m_lock)
            {
                if (m_currentTask != null)
                {
                    if (SitePath != m_currentTask.Parameter.SitePath || SiteKey != m_currentTask.Parameter.SiteKey)
                    {
                        SitePath = m_currentTask.Parameter.SitePath;
                        SiteKey = m_currentTask.Parameter.SiteKey;

                        m_browser.LoadHtml($"<!DOCTYPE html><html><head><style>{LoaderHtmlElement.Style}</style><script src=\"https://www.google.com/recaptcha/api.js\">async defer</script></head><body>" +
                                           $"<center><div>{LoaderHtmlElement.Element}</div></center><div class=\"g-recaptcha\" data-callback=\"void\" data-sitekey=\"{m_currentTask.Parameter.SiteKey}\" data-size=\"invisible\">" +
                                           "</div></body></html>", m_currentTask.Parameter.SitePath);
                    }
                    else
                    {
                        reset = true;
                    }
                }
            }

            if (reset)
            {
                Reset();
            }
        }

        private void Reset()
        {
            lock (m_lock)
            {
                try
                {
                    m_browser.ExecuteScriptAsync(@"(function() {{ grecaptcha.execute(); }})()");
                }
                catch (Exception e)
                {
                    
                }
                
            }
        }

        #endregion

        #region Fields

        private IWpfWebBrowser m_browser = null;
        private RecaptchaManualSourceTask m_currentTask = null;
        private readonly RequestHandler m_requestHandler = null;
        private bool m_isEnabled = false;
        private string m_siteKey = null;
        private string m_sitePath = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public class RecaptchaManualSourceTask: ICaptchaSolutionSourceTask
    {
        #region Properties

        public object Solution {
            get
            {
                lock (m_lock)
                {
                    return m_solution;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_solution = value;
                }

                OnSolutionChange();
            }
        }

        public RecaptchaParameter Parameter
        {
            get
            {
                lock (m_lock)
                {
                    return m_parameter;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_parameter = value;
                }
            }
        }

        #endregion

        #region Methods

        private void OnSolutionChange()
        {
            CaptchaSolutionReleasedEvent handler = SolutionReleased;
            handler?.Invoke(this, null);
        }

        #endregion

        #region Fields

        private object m_solution = null;
        private RecaptchaParameter m_parameter = null;
        private object m_lock = new object();

        #endregion

        #region Events

        public event CaptchaSolutionReleasedEvent SolutionReleased;

        #endregion
    }

    public class RequestHandler : IRequestHandler
    {

        #region Properties



        #endregion

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
        {
            return false;
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
            WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl,
            ISslInfo sslInfo, IRequestCallback callback)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {

        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port,
            string realm, string scheme, IAuthCallback callback)
        {
            return false;
        }

        public bool OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port,
            X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {

        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize,
            IRequestCallback callback)
        {
            return false;
        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response, ref string newUrl)
        {

        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return false;
        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response)
        {
            return false;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response)
        {
            IResponseFilter filter = null;
            
            int statusCode = response.StatusCode;

            if (statusCode == 200 && request.Url.Contains("userverify"))
            {
                RecaptchaResponseFilter rFilter = new RecaptchaResponseFilter();
                rFilter.TokenFound += OnTokenFound;

                filter = rFilter;
            }

            return filter;
        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response, UrlRequestStatus status, long receivedContentLength)
        {

        }

        private void OnTokenFound(object sender, RecaptchaResponseFilterTokenFoundEventArgs args)
        {
            RecaptchaResponseFilterTokenFoundEventHandler handler = TokenFound;
            handler?.Invoke(this, args);
        }

        #region Fields
        


        #endregion

        #region Events

        public event RecaptchaResponseFilterTokenFoundEventHandler TokenFound;

        #endregion
    }

    public class RecaptchaResponseFilter : IResponseFilter
    {
        #region Constructors



        #endregion

        #region Properties



        #endregion

        public void Dispose()
        {
        }

        public bool InitFilter()
        {
            return true;
        }

        public FilterStatus Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            dataInRead = dataIn.Length;
            dataOutWritten = Math.Min(dataInRead, dataOut.Length);
            dataIn.CopyTo(dataOut);

            dataIn.Position = 0;

            string str = null;

            using (StreamReader reader = new StreamReader(dataIn))
            {
                str = reader.ReadToEnd();
            }

            if (str.Length < 512)
            {
                Regex tokenRegex = new Regex("\"uvresp\",\"([^\"]*)\",");
                Match tokenRegexMatch = tokenRegex.Match(str);

                OnTokenFound(tokenRegexMatch.Groups[1].Value);
            }

            return FilterStatus.Done;
        }

        #region Methods

        private void OnTokenFound(string token)
        {
            RecaptchaResponseFilterTokenFoundEventHandler handler = TokenFound;
            handler?.Invoke(this, new RecaptchaResponseFilterTokenFoundEventArgs(token));
        }

        #endregion

        #region Fields



        #endregion

        #region Events

        public event RecaptchaResponseFilterTokenFoundEventHandler TokenFound;

        #endregion
    }

    public delegate void RecaptchaResponseFilterTokenFoundEventHandler(object sender,
        RecaptchaResponseFilterTokenFoundEventArgs args);

    public class RecaptchaResponseFilterTokenFoundEventArgs : EventArgs
    {
        #region Construtors

        public RecaptchaResponseFilterTokenFoundEventArgs(string token)
        {
            Token = token;
        }

        #endregion

        #region Properties

        public string Token { get; }

        #endregion
    }

    public static class LoaderHtmlElement
    {
        #region Properties

        public static string Style = @"#floatingBarsG{
	                                    position:relative;
	                                    width:59px;
	                                    height:73px;
	                                    margin:auto;
                                    }

                                    .blockG{
	                                    position:absolute;
	                                    background-color:rgb(255,255,255);
	                                    width:9px;
	                                    height:23px;
	                                    border-radius:8px 8px 0 0;
		                                    -o-border-radius:8px 8px 0 0;
		                                    -ms-border-radius:8px 8px 0 0;
		                                    -webkit-border-radius:8px 8px 0 0;
		                                    -moz-border-radius:8px 8px 0 0;
	                                    transform:scale(0.4);
		                                    -o-transform:scale(0.4);
		                                    -ms-transform:scale(0.4);
		                                    -webkit-transform:scale(0.4);
		                                    -moz-transform:scale(0.4);
	                                    animation-name:fadeG;
		                                    -o-animation-name:fadeG;
		                                    -ms-animation-name:fadeG;
		                                    -webkit-animation-name:fadeG;
		                                    -moz-animation-name:fadeG;
	                                    animation-duration:0.732s;
		                                    -o-animation-duration:0.732s;
		                                    -ms-animation-duration:0.732s;
		                                    -webkit-animation-duration:0.732s;
		                                    -moz-animation-duration:0.732s;
	                                    animation-iteration-count:infinite;
		                                    -o-animation-iteration-count:infinite;
		                                    -ms-animation-iteration-count:infinite;
		                                    -webkit-animation-iteration-count:infinite;
		                                    -moz-animation-iteration-count:infinite;
	                                    animation-direction:normal;
		                                    -o-animation-direction:normal;
		                                    -ms-animation-direction:normal;
		                                    -webkit-animation-direction:normal;
		                                    -moz-animation-direction:normal;
                                    }

                                    #rotateG_01{
	                                    left:0;
	                                    top:27px;
	                                    animation-delay:0.2695s;
		                                    -o-animation-delay:0.2695s;
		                                    -ms-animation-delay:0.2695s;
		                                    -webkit-animation-delay:0.2695s;
		                                    -moz-animation-delay:0.2695s;
	                                    transform:rotate(-90deg);
		                                    -o-transform:rotate(-90deg);
		                                    -ms-transform:rotate(-90deg);
		                                    -webkit-transform:rotate(-90deg);
		                                    -moz-transform:rotate(-90deg);
                                    }

                                    #rotateG_02{
	                                    left:8px;
	                                    top:9px;
	                                    animation-delay:0.366s;
		                                    -o-animation-delay:0.366s;
		                                    -ms-animation-delay:0.366s;
		                                    -webkit-animation-delay:0.366s;
		                                    -moz-animation-delay:0.366s;
	                                    transform:rotate(-45deg);
		                                    -o-transform:rotate(-45deg);
		                                    -ms-transform:rotate(-45deg);
		                                    -webkit-transform:rotate(-45deg);
		                                    -moz-transform:rotate(-45deg);
                                    }

                                    #rotateG_03{
	                                    left:25px;
	                                    top:3px;
	                                    animation-delay:0.4525s;
		                                    -o-animation-delay:0.4525s;
		                                    -ms-animation-delay:0.4525s;
		                                    -webkit-animation-delay:0.4525s;
		                                    -moz-animation-delay:0.4525s;
	                                    transform:rotate(0deg);
		                                    -o-transform:rotate(0deg);
		                                    -ms-transform:rotate(0deg);
		                                    -webkit-transform:rotate(0deg);
		                                    -moz-transform:rotate(0deg);
                                    }

                                    #rotateG_04{
	                                    right:8px;
	                                    top:9px;
	                                    animation-delay:0.549s;
		                                    -o-animation-delay:0.549s;
		                                    -ms-animation-delay:0.549s;
		                                    -webkit-animation-delay:0.549s;
		                                    -moz-animation-delay:0.549s;
	                                    transform:rotate(45deg);
		                                    -o-transform:rotate(45deg);
		                                    -ms-transform:rotate(45deg);
		                                    -webkit-transform:rotate(45deg);
		                                    -moz-transform:rotate(45deg);
                                    }

                                    #rotateG_05{
	                                    right:0;
	                                    top:27px;
	                                    animation-delay:0.6355s;
		                                    -o-animation-delay:0.6355s;
		                                    -ms-animation-delay:0.6355s;
		                                    -webkit-animation-delay:0.6355s;
		                                    -moz-animation-delay:0.6355s;
	                                    transform:rotate(90deg);
		                                    -o-transform:rotate(90deg);
		                                    -ms-transform:rotate(90deg);
		                                    -webkit-transform:rotate(90deg);
		                                    -moz-transform:rotate(90deg);
                                    }

                                    #rotateG_06{
	                                    right:8px;
	                                    bottom:7px;
	                                    animation-delay:0.732s;
		                                    -o-animation-delay:0.732s;
		                                    -ms-animation-delay:0.732s;
		                                    -webkit-animation-delay:0.732s;
		                                    -moz-animation-delay:0.732s;
	                                    transform:rotate(135deg);
		                                    -o-transform:rotate(135deg);
		                                    -ms-transform:rotate(135deg);
		                                    -webkit-transform:rotate(135deg);
		                                    -moz-transform:rotate(135deg);
                                    }

                                    #rotateG_07{
	                                    bottom:0;
	                                    left:25px;
	                                    animation-delay:0.8185s;
		                                    -o-animation-delay:0.8185s;
		                                    -ms-animation-delay:0.8185s;
		                                    -webkit-animation-delay:0.8185s;
		                                    -moz-animation-delay:0.8185s;
	                                    transform:rotate(180deg);
		                                    -o-transform:rotate(180deg);
		                                    -ms-transform:rotate(180deg);
		                                    -webkit-transform:rotate(180deg);
		                                    -moz-transform:rotate(180deg);
                                    }

                                    #rotateG_08{
	                                    left:8px;
	                                    bottom:7px;
	                                    animation-delay:0.905s;
		                                    -o-animation-delay:0.905s;
		                                    -ms-animation-delay:0.905s;
		                                    -webkit-animation-delay:0.905s;
		                                    -moz-animation-delay:0.905s;
	                                    transform:rotate(-135deg);
		                                    -o-transform:rotate(-135deg);
		                                    -ms-transform:rotate(-135deg);
		                                    -webkit-transform:rotate(-135deg);
		                                    -moz-transform:rotate(-135deg);
                                    }



                                    @keyframes fadeG{
	                                    0%{
		                                    background-color:rgb(0,0,0);
	                                    }

	                                    100%{
		                                    background-color:rgb(255,255,255);
	                                    }
                                    }

                                    @-o-keyframes fadeG{
	                                    0%{
		                                    background-color:rgb(0,0,0);
	                                    }

	                                    100%{
		                                    background-color:rgb(255,255,255);
	                                    }
                                    }

                                    @-ms-keyframes fadeG{
	                                    0%{
		                                    background-color:rgb(0,0,0);
	                                    }

	                                    100%{
		                                    background-color:rgb(255,255,255);
	                                    }
                                    }

                                    @-webkit-keyframes fadeG{
	                                    0%{
		                                    background-color:rgb(0,0,0);
	                                    }

	                                    100%{
		                                    background-color:rgb(255,255,255);
	                                    }
                                    }

                                    @-moz-keyframes fadeG{
	                                    0%{
		                                    background-color:rgb(0,0,0);
	                                    }

	                                    100%{
		                                    background-color:rgb(255,255,255);
	                                    }
                                    }";

        public static string Element = @"<div style='position: absolute; top: 50%; left: 50%; margin-top: -50px; margin-left: -50px; width: 100px; height: 100px;'>
                                            <div id='floatingBarsG'>
	                                            <div class='blockG' id='rotateG_01'></div>
	                                            <div class='blockG' id='rotateG_02'></div>
	                                            <div class='blockG' id='rotateG_03'></div>
	                                            <div class='blockG' id='rotateG_04'></div>
	                                            <div class='blockG' id='rotateG_05'></div>
	                                            <div class='blockG' id='rotateG_06'></div>
	                                            <div class='blockG' id='rotateG_07'></div>
	                                            <div class='blockG' id='rotateG_08'></div>
                                            </div>
                                        </div>";

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bot.Helpers;
using Bot.Services;

namespace Bot.Models
{
    public class ReleaseTaskBase: BindableObject, IReleaseTask
    {
        #region Constructors

        public ReleaseTaskBase(Release release)
        {
            Release = release;
        }

        #endregion

        #region Properties

        public ReleaseTaskState State {
            get
            {
                lock (m_lock)
                {
                    return m_state;
                }
            }

            protected set { SetProperty(ref m_state, value, m_lock); }
        }

        public Release Release
        {
            get
            {
                lock (m_lock)
                {
                    return m_release;
                }
            }

            protected set { SetProperty(ref m_release, value, m_lock); }
        }
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();
        public ObservableCollection<CheckoutTaskCcProfile> Profiles { get; } = new ObservableCollection<CheckoutTaskCcProfile>();

        public bool IsProductAvailable
        {
            get
            {
                lock (m_lock)
                {
                    return m_isProductAvailable;
                }
            }

            set { SetProperty(ref m_isProductAvailable, value, m_lock); }
        }

        public static string BrowserAgent { get; } = @"User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36";
        public static string MobileBrowserAgent { get; } = @"User-Agent: User-Agent: Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1";

        public List<string> AvailableSizes
        {
            get
            {
                lock (m_lock)
                {
                    return m_availableSizes;
                }
            }

            set { SetProperty(ref m_availableSizes, value, m_lock); }
        }

        public string ProductName
        {
            get
            {
                lock (m_lock)
                {
                    return m_productName;
                }
            }

            set { SetProperty(ref m_productName, value, m_lock); }
        }

        #endregion

        #region Methods

        public async Task<bool> Start(CancellationToken cancelToken)
        {
            bool ret = true;

            lock (m_lock)
            {
                if (m_state == ReleaseTaskState.Working)
                {
                    ret = false;
                }
                m_state = ReleaseTaskState.Working;
            }

            if (ret)
            {
                OnPropertyChanged("State");

                await Task.Run(() =>
                {
                    Log.Clear();

                    ReleaseProductInfo productInfo = new ReleaseProductInfo()
                    {
                        ProductLink = Release.ProductLink,
                        KeyWords = Release.KeyWords.ToList()
                    };

                    ProductMonitorTask productMonitorTask = new ProductMonitorTask()
                    {
                        Period = Release.Footsite.Settings.ProductMonitorPeriod,
                        ProductInfo = productInfo,
                        Proxy = Release.GlobalProxy
                    };

                    Thread monitorThread = new Thread(() => MonitorProduct(cancelToken, Log, productMonitorTask));
                    monitorThread.Start();

                    List<CheckoutTask> checkoutTasks = new List<CheckoutTask>();

                    int taskId = 0;
                    ProxyPool proxyPool = new ProxyPool(Release.Proxies.ToList(), 1);

                    foreach (ReleaseCheckoutProfile profile in Release.Profiles)
                    {
                        CheckoutTaskCcProfile checkoutTaskCcProfile = new CheckoutTaskCcProfile(new ReleaseCheckoutProfile(profile) { CheckoutProfile = new CheckoutProfile(profile.CheckoutProfile)}, Release.Footsite.Settings.PurchaseLimitPerProfile);
                        Profiles.Add(checkoutTaskCcProfile);

                        for (int i = 0; i < profile.TasksCount; i++)
                        {
                            CheckoutTask checkoutTask = new CheckoutTask()
                            {
                                Id = taskId++,
                                Size = profile.Size,
                                VariantId = profile.VariantId,
                                Profile = checkoutTaskCcProfile,
                                ProxyPool = proxyPool,
                                ProductAvailableEvent = productMonitorTask.ProductAvailableEvent,
                                Footsite = Release.Footsite,
                                ProductInfo = productInfo
                            };
                            Logger.LogEvent(checkoutTask.Log, $"TASK {checkoutTask.Id}", "Initializing...");

                            checkoutTaskCcProfile.CheckoutTasks.Add(checkoutTask);
                            checkoutTasks.Add(checkoutTask);
                        }
                    }

                    RestockMonitorTask restockMonitorTask = new RestockMonitorTask()
                    {
                        Period = TimeSpan.FromMilliseconds(Release.Footsite.Settings.ProductMonitorPeriod),
                        CheckoutTasks = checkoutTasks,
                        ProductInfo = productInfo,
                    };

                    Thread restockMonitorThread = new Thread(() => RestockMonitor(Log, restockMonitorTask, cancelToken));
                    restockMonitorThread.Start();

                    CheckoutTasksWatcher checkoutTasksWatcher = new CheckoutTasksWatcher(checkoutTasks);

                    Task.Run(() =>
                    {
                        foreach (CheckoutTask checkoutTask in checkoutTasks)
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            Thread checkoutThread = new Thread(() => Checkout(cancelToken, checkoutTask));
                            checkoutThread.Start();
                        }
                    });

                    checkoutTasksWatcher.FinishedEvent.WaitOne(cancelToken);

                    productMonitorTask.CompleteEvent.Set();
                    restockMonitorTask.CompleteEvent.Set();
                });

                TierControl.ReleaseQuota(Release.TasksCount, Release.Footsite.Type);

                State = ReleaseTaskState.Idle;
            }

            return ret;
        }

        private void MonitorProduct(CancellationToken cancelToken, ObservableCollection<string> log, ProductMonitorTask task)
        {
            bool isFirstTime = true;

            Logger.LogEvent(log, "PRODUCT MONITOR", "Started!");

            while (true)
            {
                List<string> availableSizes = null;

                Logger.LogEvent(log, "PRODUCT MONITOR", "Checking product...");

                try
                {
                    if (!CheckProduct(task.ProductInfo, task.Proxy, cancelToken, out availableSizes))
                    {
                        Logger.LogEvent(log, "PRODUCT MONITOR", "Product is available!");

                        if (availableSizes != null && availableSizes.Count > 0)
                        {
                            Logger.LogEvent(log, "PRODUCT MONITOR", $"Sizes: {string.Join(", ", availableSizes)}");
                            AvailableSizes = availableSizes.GetRange(0, availableSizes.Count / 2);
                        }
                        else
                        {
                            Logger.LogEvent(log, "PRODUCT MONITOR",
                                "Could not fetch available sizes. All range of sizes will be used for RANDOM option.");
                            AvailableSizes = Release.Profiles[0].ClothesSizeSystem.Sizes.ToList();
                        }

                        task.ProductAvailableEvent.Set();
                        IsProductAvailable = true;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogEvent(log, "PRODUCT MONITOR",
                        "Could not fetch available sizes. All range of sizes will be used for RANDOM option.");
                }

                if (WaitHandle.WaitAny(new WaitHandle[] { cancelToken.WaitHandle, task.CompleteEvent },
                        TimeSpan.FromMilliseconds(task.Period)) != WaitHandle.WaitTimeout)
                {
                    break;
                }
            }
            Logger.LogEvent(log, "PRODUCT MONITOR", "Exit");
        }

        private void RestockMonitor(ObservableCollection<string> log, RestockMonitorTask task, CancellationToken cancelToken)
        {
            Logger.LogEvent(log, "RESTOCK MONITOR", "Started!");
            Logger.LogEvent(log, "RESTOCK MONITOR", "Paused. Waiting for signal.");

            WaitHandle.WaitAny(new WaitHandle[] {task.StartEvent, task.CompleteEvent, cancelToken.WaitHandle});

            while (WaitHandle.WaitAny(new WaitHandle[] { cancelToken.WaitHandle, task.CompleteEvent}, task.Period) == WaitHandle.WaitTimeout)
            {
                CheckStock(task.CheckoutTasks, task.Proxy, cancelToken);
            }

            Logger.LogEvent(log, "RESTOCK MONITOR", "Exit");
        }

        private void Checkout(CancellationToken cancelToken, CheckoutTask task)
        {
            ICheckoutTaskContext context = null;

            CheckoutStep waitProxy = new CheckoutStep(WaitProxy, "Waiting proxy", CheckoutTaskState.WaitingProxy);
            CheckoutStep waitProduct = new CheckoutStep(WaitProduct, "Waiting product", CheckoutTaskState.WaitingProduct);
            CheckoutStep addToCart = new CheckoutStep(AddToCart, "Carting", CheckoutTaskState.Carting);
            CheckoutStep checkCart = new CheckoutStep(CheckCart, "Checking cart", CheckoutTaskState.Checking);
            CheckoutStep billing = new CheckoutStep(Billing, "Billing", CheckoutTaskState.Billing);
            CheckoutStep waitQuota = new CheckoutStep(WaitQuota, "Waiting quota", CheckoutTaskState.WaitingQuota);
            CheckoutStep paying = new CheckoutStep(Paying, "Paying", CheckoutTaskState.Paying);

            StepResult res = StepResult.Ok;
            TimeSpan executionTime;

            if (!TierControl.Wait(cancelToken))
            {
                if (IsCanceled(cancelToken, task, null, false)) return;
                throw new Exception();
            }

            Logger.LogEvent(task.Log, $"TASK {task.Id}", "Initializing... done");

            //res = waitProxy.Run(null, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);
            task.Proxy = task.ProxyPool.GetOne();

            if (IsCanceled(cancelToken, task)) return;

            if (res != StepResult.Ok)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "ATC can not be continued without proxy");

                return;
            }
            
            while (true)
            {
                if (res != StepResult.Ok)
                {
                    cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod));

                    if (IsCanceled(cancelToken, task, context)) return;
                }

                context?.Dispose();
                context = CreateContext(task);

                res = waitProduct.Run(context, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                if (IsCanceled(cancelToken, task, context)) return;

                if (res != StepResult.Ok)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{waitProduct.Name} is failed... retry");

                    continue;
                }
                
                if (task.Size == ClothesSizeSystemCollection.RandomSize)
                {
                    task.Size = GetRandomSize();

                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Size {task.Size} is selected");
                }

                res = addToCart.Run(context, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                if (IsCanceled(cancelToken, task, context)) return;

                if (res == StepResult.OutOfStock)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{addToCart.Name} is failed... Out of Stock. Retry ATC");

                    continue;
                }

                if (res != StepResult.Ok)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{addToCart.Name} is failed... retry ATC");

                    continue;
                }

                while (true)
                {
                    if (res != StepResult.Ok)
                    {
                        cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod));

                        if (IsCanceled(cancelToken, task, context)) return;
                    }

                    res = checkCart.Run(context, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                    if (IsCanceled(cancelToken, task, context)) return;

                    if (res != StepResult.Ok)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{checkCart.Name} is failed... retry ATC");

                        break;
                    }

                    res = billing.Run(context, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                    if (IsCanceled(cancelToken, task, context)) return;

                    if (res != StepResult.Ok)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{billing.Name} is failed... retrying checkout");
                    }

                    res = waitQuota.Run(context, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                    if (IsCanceled(cancelToken, task, context)) return;

                    if (res == StepResult.Failed)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", "Checkout quota has ended");
                    }

                    res = paying.Run(context, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                    if (res == StepResult.Ok)
                    {
                        task.Profile.Release(true);

                        OnSuccessfulCheckout(new CheckoutInfo()
                        {
                            ReleaseName = Release.Name,
                            ProductName = ProductName,
                            ReleaseCheckoutProfile = task.Profile.ReleaseCheckoutProfile,
                            Size = task.Size,
                            TimeStamp = DateTime.Now
                        });
                    }
                    else
                    {
                        task.Profile.Release(false);
                    }

                    if (IsCanceled(cancelToken, task, context)) return;

                    if (res != StepResult.Ok)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{paying.Name} is failed... retrying checkout");

                        continue;
                    }

                    break;
                }

                if (res != StepResult.Ok)
                {
                    continue;
                }

                break;
            }

            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Finished!");

            task.State = CheckoutTaskState.Finished;

            FreeResources(context, task);
        }

        private bool IsCanceled(CancellationToken cancelToken, CheckoutTask task, ICheckoutTaskContext context = null, bool isTierAcquired = true)
        {
            bool ret = false;

            if (cancelToken.IsCancellationRequested)
            {
                task.State = CheckoutTaskState.Canceled;
                context?.Dispose();
                if (isTierAcquired)
                {
                    TierControl.Release();
                }
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Task is canceled!");
                ret = true;
            }
            
            return ret;
        }

        protected virtual ICheckoutTaskContext CreateContext(CheckoutTask task)
        {
            return null;
        }

        protected virtual StepResult WaitProduct(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            task.ProductAvailableEvent.WaitOne(cancelToken);
            return StepResult.Ok;
        }

        protected virtual StepResult AddToCart(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            return StepResult.Ok;
        }

        protected virtual StepResult CheckCart(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            return StepResult.Ok;
        }

        protected virtual StepResult Billing(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            return StepResult.Ok;
        }

        protected virtual StepResult Paying(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            return StepResult.Ok;
        }

        protected virtual void CheckStock(List<CheckoutTask> tasks, Proxy proxy, CancellationToken cancelToken)
        {

        }

        protected virtual bool CheckProduct(ReleaseProductInfo productInfo, Proxy proxy, CancellationToken cancelToken, out List<string> availableSizes)
        {
            Random r = new Random();

            availableSizes = new List<string>();

            return r.Next(0, 100) > 50;
        }

        protected virtual void FreeResources(ICheckoutTaskContext context, CheckoutTask task)
        {
            task.ProxyPool.Release(task.Proxy);
            task.Proxy = null;
            context?.Dispose();
            TierControl.Release();
        }

        private StepResult WaitProxy(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;

            task.Proxy = task.ProxyPool.WaitOne(cancelToken);

            if (task.Proxy == null)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    res = StepResult.Canceled;
                }
                else
                {
                    res = StepResult.Failed;
                }
            }

            return res;
        }

        private StepResult WaitQuota(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;

            bool isQuotaAvailable = task.Profile.Wait(cancelToken);

            if (!isQuotaAvailable)
            {
                res = StepResult.Failed;
            }

            return res;
        }

        private string GetRandomSize()
        {
            lock (m_lock)
            {
                Random random = new Random(DateTime.Now.Millisecond);
                int index = random.Next(0, m_availableSizes.Count);

                return m_availableSizes[index];
            }
        }

        private void OnSuccessfulCheckout(CheckoutInfo checkoutInfo)
        {
            ReleaseTaskSuccessfulCheckoutEventHandler handler = SuccessfulCheckout;
            handler?.Invoke(this, new ReleaseTaskSuccessfulCheckoutEventArgs(checkoutInfo));
        }

        #endregion

        #region Fields

        private Release m_release = null;
        private ReleaseTaskState m_state = ReleaseTaskState.Idle;
        private bool m_isProductAvailable = false;
        private List<string> m_availableSizes = null;
        private string m_productName = null;
        private readonly object m_lock = new object();

        #endregion

        #region Events

        public event ReleaseTaskSuccessfulCheckoutEventHandler SuccessfulCheckout;

        #endregion
    }

    public enum CheckoutTaskState
    {
        Initializing,
        WaitingProxy,
        WaitingProduct,
        WaitingQuota,
        WaitingCaptcha,
        Carting,
        Checking,
        Billing,
        Paying,
        Verifying,
        Finished,
        Canceled,
        Undefined
    }

    public class CheckoutTask : BindableObject
    {
        #region Properties

        public int? Id
        {
            get
            {
                lock (m_lock)
                {
                    return m_id;
                }
            }

            set { SetProperty(ref m_id, value, m_lock); }
        }

        public CheckoutTaskState State
        {
            get
            {
                lock (m_lock)
                {
                    return m_state;
                }
            }

            set
            {
                if (SetProperty(ref m_state, value, m_lock))
                {
                    if (State == CheckoutTaskState.Finished)
                    {
                        OnFinished();
                    }
                }
            }
        }

        public string Size
        {
            get
            {
                lock (m_lock)
                {
                    return m_size;
                }
            }

            set { SetProperty(ref m_size, value, m_lock); }
        }

        public string VariantId
        {
            get
            {
                lock (m_lock)
                {
                    return m_variantId;
                }
            }

            set { SetProperty(ref m_variantId, value, m_lock); }
        }

        public Proxy Proxy
        {
            get
            {
                lock (m_lock)
                {
                    return m_proxy;
                }
            }

            set { SetProperty(ref m_proxy, value, m_lock); }
        }

        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        public CheckoutTaskCcProfile Profile
        {
            get
            {
                lock (m_lock)
                {
                    return m_profile;
                }
            }

            set { SetProperty(ref m_profile, value, m_lock); }
        }

        public ProxyPool ProxyPool
        {
            get
            {
                lock (m_lock)
                {
                    return m_proxyPool;
                }
            }

            set { SetProperty(ref m_proxyPool, value, m_lock); }
        }

        public WaitHandle ProductAvailableEvent
        {
            get
            {
                lock (m_lock)
                {
                    return m_productAvailableEvent;
                }
            }

            set { SetProperty(ref m_productAvailableEvent, value, m_lock); }
        }

        public Footsite Footsite
        {
            get
            {
                lock (m_lock)
                {
                    return m_footsite;
                }
            }

            set { SetProperty(ref m_footsite, value, m_lock); }
        }

        public ReleaseProductInfo ProductInfo
        {
            get
            {
                lock (m_lock)
                {
                    return m_productInfo;
                }
            }

            set { SetProperty(ref m_productInfo, value, m_lock); }
        }

        #endregion

        #region Events

        public event CheckoutTaskFinishedEventHandler Finished;

        #endregion

        #region Methods

        private void OnFinished()
        {
            CheckoutTaskFinishedEventHandler handler = Finished;
            handler?.Invoke(this, null);
        }

        #endregion

        #region Fields

        private int? m_id = null;
        
        private CheckoutTaskState m_state = CheckoutTaskState.Initializing;
        private string m_size = null;
        private string m_variantId = null;
        private Proxy m_proxy = null;
        private CheckoutTaskCcProfile m_profile = null;
        private ProxyPool m_proxyPool = null;
        private WaitHandle m_productAvailableEvent = null;
        private Footsite m_footsite = null;
        private ReleaseProductInfo m_productInfo = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public class CheckoutTaskCcProfile : BindableObject
    {
        #region Constructors

        public CheckoutTaskCcProfile(ReleaseCheckoutProfile releaseCheckoutProfile, int maxCheckoutsCount)
        {
            m_releaseCheckoutProfile = releaseCheckoutProfile;
            m_maxCheckoutsCount = maxCheckoutsCount;
            m_semaphore = new SemaphoreSlim(maxCheckoutsCount, maxCheckoutsCount);
        }

        #endregion

        #region Properties

        public ReleaseCheckoutProfile ReleaseCheckoutProfile
        {
            get
            {
                lock (m_lock)
                {
                    return m_releaseCheckoutProfile;
                }
            }

            protected set { SetProperty(ref m_releaseCheckoutProfile, value, m_lock); }
        }

        public int CheckoutsCount
        {
            get
            {
                lock (m_lock)
                {
                    return m_checkoutsCount;
                }
            }

            protected set { SetProperty(ref m_checkoutsCount, value, m_lock); }
        }

        public ObservableCollection<CheckoutTask> CheckoutTasks { get; } = new ObservableCollection<CheckoutTask>();

        #endregion

        #region Methods

        public bool Wait(CancellationToken cancelToken)
        {
            bool ret = true;

            if (CheckoutsCount < m_maxCheckoutsCount)
            {
                bool isCanceled = false;

                try
                {
                    m_semaphore.Wait(cancelToken);
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(OperationCanceledException))
                    {
                        isCanceled = true;
                    }
                }

                if (isCanceled || CheckoutsCount >= m_maxCheckoutsCount)
                {
                    if (!isCanceled)
                    {
                        m_semaphore.Release();
                    }

                    ret = false;
                }
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public void Release(bool isCheckoutSuccessful)
        {
            if (isCheckoutSuccessful)
            {
                lock (m_lock)
                {
                    m_checkoutsCount++;
                }
            }

            m_semaphore.Release();
        }

        #endregion

        #region Fields

        private ReleaseCheckoutProfile m_releaseCheckoutProfile = null;
        private int m_checkoutsCount = 0;
        private readonly int m_maxCheckoutsCount = 0;
        private readonly SemaphoreSlim m_semaphore = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public class ProductMonitorTask: BindableObject
    {
        #region Properties

        public int Period
        {
            get
            {
                lock (m_lock)
                {
                    return m_period;
                }
            }

            set { SetProperty(ref m_period, value, m_lock); }
        }

        public ManualResetEvent CompleteEvent
        {
            get
            {
                lock (m_lock)
                {
                    return m_completeEvent;
                }
            }
        }

        public ManualResetEvent ProductAvailableEvent
        {
            get
            {
                lock (m_lock)
                {
                    return m_productAvailableEvent;
                }
            }
        }

        public ReleaseProductInfo ProductInfo
        {
            get
            {
                lock (m_lock)
                {
                    return m_productInfo;
                }
            }

            set { SetProperty(ref m_productInfo, value, m_lock); }
        }

        public Proxy Proxy
        {
            get
            {
                lock (m_lock)
                {
                    return m_proxy;
                }
            }

            set { SetProperty(ref m_proxy, value, m_lock); }
        }

        #endregion

        #region Fields

        private int m_period = 0;
        private ReleaseProductInfo m_productInfo = null;
        private readonly ManualResetEvent m_completeEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent m_productAvailableEvent = new ManualResetEvent(false);
        private Proxy m_proxy = null;
        private readonly object m_lock = new object(); 

        #endregion
    }

    public class RestockMonitorTask : BindableObject
    {
        #region Constructors



        #endregion

        #region Properties

        public TimeSpan Period
        {
            get
            {
                lock (m_lock)
                {
                    return m_period;
                }
            }

            set { SetProperty(ref m_period, value, m_lock); }
        }

        public ManualResetEvent StartEvent
        {
            get
            {
                lock (m_lock)
                {
                    return m_startEvent;
                }
            }
        }

        public List<CheckoutTask> CheckoutTasks
        {
            get
            {
                lock (m_lock)
                {
                    return m_checkoutTasks;
                }
            }

            set { SetProperty(ref m_checkoutTasks, value, m_lock); }
        }

        public ManualResetEvent CompleteEvent
        {
            get
            {
                lock (m_lock)
                {
                    return m_completeEvent;
                }
            }
        }

        public ReleaseProductInfo ProductInfo
        {
            get
            {
                lock (m_lock)
                {
                    return m_productInfo;
                }
            }

            set { SetProperty(ref m_productInfo, value, m_lock); }
        }

        public Proxy Proxy
        {
            get
            {
                lock (m_lock)
                {
                    return m_proxy;
                }
            }

            set { SetProperty(ref m_proxy, value, m_lock); }
        }

        #endregion

        #region Fields

        private TimeSpan m_period;
        private readonly ManualResetEvent m_startEvent = new ManualResetEvent(false);
        private List<CheckoutTask> m_checkoutTasks = null;
        private readonly ManualResetEvent m_completeEvent = new ManualResetEvent(false);
        private ReleaseProductInfo m_productInfo = null;
        private Proxy m_proxy = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public class ProxyPool : BindableObject
    {
        #region Constructors

        public ProxyPool(List<Proxy> proxies, int maxLocksCount)
        {
            foreach (Proxy proxy in proxies)
            {
                m_proxies[proxy] = 0;
            }
            m_maxLocksCount = maxLocksCount;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public Proxy WaitOne(CancellationToken cancelToken)
        {
            Proxy ret = null;

            WaitHandle[] events = {m_releasedEvent, cancelToken.WaitHandle};

            while (true)
            {
                int eventIndex = WaitHandle.WaitAny(events);

                if (events[eventIndex] == cancelToken.WaitHandle)
                {
                    break;
                }

                ret = GetOne();

                if (ret == null)
                {
                    m_releasedEvent.Reset();
                }
                else
                {
                    break;
                }
            }

            return ret;
        }

        public void Release(Proxy proxy)
        {
            lock (m_lock)
            {
                if (proxy != null && m_proxies.ContainsKey(proxy))
                {
                    m_proxies[proxy]--;
                    m_releasedEvent.Set();
                }
            }
        }

        public Proxy GetOne()
        {
            Proxy ret = null;

            lock (m_lock)
            {
                List<KeyValuePair<Proxy, int>> freeProxies = m_proxies.Where(p => p.Value < m_maxLocksCount).ToList();

                if (freeProxies.Count > 0)
                {
                    ret = freeProxies.First().Key;
                    m_proxies[ret]++;
                }
            }

            return ret;
        }

        #endregion

        #region Fields
        
        private readonly Dictionary<Proxy, int> m_proxies = new Dictionary<Proxy, int>();
        private readonly int m_maxLocksCount;
        private readonly ManualResetEvent m_releasedEvent = new ManualResetEvent(true);
        private readonly object m_lock = new object();

        #endregion
    }

    public class CheckoutTasksWatcher
    {
        #region Constructors

        public CheckoutTasksWatcher(List<CheckoutTask> tasks)
        {
            m_tasks = tasks.ToList();

            foreach (CheckoutTask task in m_tasks)
            {
                task.Finished += CheckoutTaskFinishedHandler;
            }
        }

        #endregion

        #region Properties

        public WaitHandle FinishedEvent
        {
            get
            {
                lock (m_lock)
                {
                    return m_finishedEvent;
                }
            }
        }

        #endregion

        #region Methods

        private void CheckoutTaskFinishedHandler(object sender, EventArgs args)
        {
            CheckoutTask task = sender as CheckoutTask;
            task.Finished -= CheckoutTaskFinishedHandler;

            lock (m_lock)
            {
                m_tasks.Remove(task);

                if (m_tasks.Count == 0)
                {
                    m_finishedEvent.Set();
                }
            }
        }

        #endregion

        #region Fields

        private readonly List<CheckoutTask> m_tasks = null;
        private readonly ManualResetEvent m_finishedEvent = new ManualResetEvent(false);
        private readonly object m_lock = new object();

        #endregion
    }

    public delegate void CheckoutTaskFinishedEventHandler(object sender, EventArgs args);

    public interface ICheckoutTaskContext : IDisposable
    {

    }

    public enum StepResult
    {
        Ok,
        Error,
        Failed,
        OutOfStock,
        Canceled
    }

    public class CheckoutStep
    {
        #region Constructors

        public CheckoutStep(Func<ICheckoutTaskContext, CheckoutTask, CancellationToken, StepResult> action, string name, CheckoutTaskState state)
        {
            m_action = action;
            Name = name;
            State = state;
        }

        #endregion

        #region Methods

        public StepResult Run(ICheckoutTaskContext context, CheckoutTask task, TimeSpan delayBetweenRetires, out TimeSpan executionTime, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Error;
            Stopwatch stopWatch = new Stopwatch();

            task.State = State != CheckoutTaskState.Undefined ? State : task.State;
            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{Name}...");

            while (res == StepResult.Error)
            {
                stopWatch.Reset();
                stopWatch.Start();

                try
                {
                    res = m_action(context, task, cancelToken);
                }
                catch (Exception e)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Exception has occurred.");

                    NotificationService.SendStatistic($"Excetpion: {e.ToString()}\nStack Trace:{e.StackTrace}",
                        "ERROR");

                    res = StepResult.Failed;
                }

                stopWatch.Stop();

                if (res == StepResult.Error)
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{Name} got error... retry");

                    if (cancelToken.WaitHandle.WaitOne(delayBetweenRetires))
                    {
                        res = StepResult.Canceled;
                        break;
                    }
                }
            }

            executionTime = stopWatch.Elapsed;

            if (res == StepResult.Ok)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", $"{Name}... done ({(int)executionTime.TotalMilliseconds} ms)");
            }

            return res;
        }

        #endregion

        #region Properties
        
        public string Name { get; }
        public CheckoutTaskState State { get; }

        #endregion

        #region Fields

        private Func<ICheckoutTaskContext, CheckoutTask, CancellationToken, StepResult> m_action { get; }

        #endregion
    }

    public class ReleaseProductInfo : BindableObject
    {
        #region Constructors



        #endregion

        #region Properties

        public string ProductLink
        {
            get
            {
                lock (m_lock)
                {
                    return m_productLink;
                }
            }

            set { SetProperty(ref m_productLink, value, m_lock); }
        }

        public List<string> KeyWords
        {
            get
            {
                lock (m_lock)
                {
                    return m_keyWords;
                }
            }

            set { SetProperty(ref m_keyWords, value, m_lock); }
        }

        #endregion

        #region Fields

        private string m_productLink = null;
        private List<string> m_keyWords = null;
        private readonly object m_lock = new object();

        #endregion
    }

    public delegate void ReleaseTaskSuccessfulCheckoutEventHandler(object sender, ReleaseTaskSuccessfulCheckoutEventArgs args);

    public class ReleaseTaskSuccessfulCheckoutEventArgs : EventArgs
    {
        #region Constructors

        public ReleaseTaskSuccessfulCheckoutEventArgs(CheckoutInfo checkoutInfo)
        {
            CheckoutInfo = checkoutInfo;
        }

        #endregion

        #region Properties

        public CheckoutInfo CheckoutInfo { get; protected set; }

        #endregion
    }
}

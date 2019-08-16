using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Data;

namespace Bot.Models
{
    static class TierControl
    {
        #region Properties

        public static Project Project
        {
            get
            {
                lock (m_lock)
                {
                    return m_project;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_project = value;
                    //CheckProject();
                }
            }
        }

        public static Tier CurrenTier
        {
            get
            {
                lock (m_lock)
                {
                    return m_currentTier;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_currentTier = value;
                }
            }
        }

        #endregion

        #region Methods

        public static void Init(Tier tier)
        {
            if (!m_isInitialized)
            {
                m_currentTier = tier;
                m_semaphore = new SemaphoreSlim((int)tier, (int)tier);

                SiteTierControl supremeTierControl = new SiteTierControl(new Dictionary<Tier, int>()
                {
                    { Tier.Novice, 10 },
                    { Tier.Standart, 20 },
                    { Tier.Ultimate, 999999 }
                });

                m_sitesTiers = new Dictionary<FootsiteType, SiteTierControl>()
                {
                    {
                        FootsiteType.Footlocker, new SiteTierControl(new Dictionary<Tier, int>()
                        {
                            { Tier.Novice, 50 },
                            { Tier.Standart, 150 },
                            { Tier.Ultimate, 999999 }
                        })
                    },
                    { FootsiteType.SupremeUSA, supremeTierControl },
                    { FootsiteType.SupremeEurope, supremeTierControl },
                    { FootsiteType.SupremeJapan, supremeTierControl }
                };

                m_isInitialized = true;
            }
        }

        public static bool Wait(CancellationToken cancelToken)
        {
            bool res = true;
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

            if (isCanceled)
            {
                res = false;
            }

            return res;
        }

        public static void Release()
        {
            m_semaphore.Release();
        }

        public static int CheckLimit(int count)
        {
            int res = count;
            int tasksTotal = 0;

            foreach (Release release in Project.Releases)
            {
                foreach (ReleaseCheckoutProfile profile in release.Profiles)
                {
                    tasksTotal += profile.TasksCount;
                }
            }

            tasksTotal += count;

            if (tasksTotal > (int) m_currentTier)
            {
                res -= tasksTotal - (int) m_currentTier;
            }

            return res;
        }

        public static int GetRemainder()
        {
            int tasksTotal = 0;

            foreach (Release release in Project.Releases)
            {
                foreach (ReleaseCheckoutProfile profile in release.Profiles)
                {
                    tasksTotal += profile.TasksCount;
                }
            }
            
            return (int)m_currentTier - tasksTotal;
        }

        private static void CheckProject()
        {
            int tasksTotal = 0;

            foreach (Release release in m_project.Releases)
            {
                foreach (ReleaseCheckoutProfile profile in release.Profiles)
                {
                    if (tasksTotal + profile.TasksCount > (int) m_currentTier)
                    {
                        profile.TasksCount = (int) m_currentTier - tasksTotal;
                    }

                    tasksTotal += profile.TasksCount;
                }
            }
        }

        public static bool TryGetQuota(int count, FootsiteType footsite)
        {
            bool res = false;

            lock (m_lock)
            {
                if (m_isInitialized)
                {
                    if (m_sitesTiers[footsite].TryGetQuota(count, m_currentTier))
                    {
                        if ((int) m_currentTier - m_currentLocksCount >= count)
                        {
                            m_currentLocksCount += count;

                            res = true;
                        }
                        else
                        {
                            m_sitesTiers[footsite].ReleaseQuota(count, m_currentTier);
                        }
                    }
                }
            }

            return res;
        }

        public static void ReleaseQuota(int count, FootsiteType footsite)
        {
            lock (m_lock)
            {
                if (m_isInitialized)
                {
                    m_sitesTiers[footsite].ReleaseQuota(count, m_currentTier);

                    m_currentLocksCount -= count;
                }
            }
        }

        #endregion

        #region Fields

        private static bool m_isInitialized = false;
        private static Tier m_currentTier = Tier.Novice;
        private static Project m_project = null;
        private static int m_currentLocksCount = 0;
        private static SemaphoreSlim m_semaphore = null;
        private static Dictionary<FootsiteType, SiteTierControl> m_sitesTiers = null;
        private static readonly object m_lock = new object();

        #endregion
    }

    public enum Tier
    {
        Novice = 50,
        Standart = 150,
        Ultimate = 999999
    }

    public class SiteTierControl
    {
        #region Constructors

        public SiteTierControl(Dictionary<Tier, int> limits)
        {
            Limits = new Dictionary<Tier, int>(limits);
            
            m_locks = new Dictionary<Tier, int>();

            foreach (KeyValuePair<Tier, int> limit in Limits)
            {
                m_locks[limit.Key] = 0;
            }

        }

        #endregion

        #region Properties

        public Dictionary<Tier, int> Limits { get; }

        #endregion

        #region Methods

        public bool TryGetQuota(int count, Tier tier)
        {
            bool res = true;

            lock (m_lock)
            {
                if (Limits[tier] - m_locks[tier] >= count)
                {
                    m_locks[tier] += count;
                }
                else
                {
                    res = false;
                }
            }

            return res;
        }

        public void ReleaseQuota(int count, Tier tier)
        {
            lock (m_lock)
            {
                m_locks[tier] -= count;
            }
        }

        #endregion

        #region Fields

        private readonly Dictionary<Tier, int> m_locks = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

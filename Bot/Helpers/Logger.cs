using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Bot.Helpers
{
    public static class Logger
    {
        #region Methods

        public static void LogEvent(ObservableCollection<string> log, string sender, string message)
        {
            m_logMutex.WaitOne();

            if (Application.Current != null)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => log.Add($"{DateTime.Now:G}:[{sender}] {message}"));
                }
                catch (Exception e)
                {
                }
            }

            m_logMutex.ReleaseMutex();
        }

        public static void RemoveLastEvent(ObservableCollection<string> log)
        {
            m_logMutex.WaitOne();

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (log.Count > 0)
                    {
                        log.RemoveAt(log.Count - 1);
                    }
                });
            }

            m_logMutex.ReleaseMutex();
        }

        #endregion

        #region Fields

        private static readonly Mutex m_logMutex = new Mutex();

        #endregion
    }
}

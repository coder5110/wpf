using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    static class DateTimeValues
    {
        #region Properties

        public static List<int> Hours
        {
            get
            {
                if (m_hours == null)
                {
                    m_hours = new List<int>();
                    m_hours.Add(12);
                    for (int i = 1; i < 12; i++)
                    {
                        m_hours.Add(i);
                    }
                }

                return m_hours;
            }
        }

        public static List<int> Minutes
        {
            get
            {
                if (m_minutes == null)
                {
                    m_minutes = new List<int>();
                    for (int i = 0; i < 60; i++)
                    {
                        m_minutes.Add(i);
                    }
                }

                return m_minutes;
            }
        }

        public static List<string> TimePeriods { get; } = new List<string>() {AM, PM};
        public static string AM => "AM";
        public static string PM => "PM";
        public static int PeriodDuration = 12;

        #endregion

        #region Fields

        private static List<int> m_hours = null;
        private static List<int> m_minutes = null;

        #endregion
    }
}

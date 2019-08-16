using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public static class MonthsAndYearsCollection
    {
        #region Properties

        public static ObservableCollection<int> Months { get; } = new ObservableCollection<int>()
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
        };

        public static ObservableCollection<int> Years
        {
            get
            {
                if (m_years == null)
                {
                    m_years = new ObservableCollection<int>();
                    DateTime currentDate = DateTime.Now;
                    for (int i = 0; i < 20; i++)
                    {
                        m_years.Add(currentDate.Year + i);
                    }
                }

                return m_years;
            }
        }

        #endregion

        private static ObservableCollection<int> m_years;
    }
}

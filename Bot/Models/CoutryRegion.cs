using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Bot.Models
{
    public class CoutryRegion: BindableObject
    {
        #region Constructors

        public CoutryRegion(string abbreviation, string name)
        {
            m_name = name;
            m_abbreviation = abbreviation;
        }

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                lock (m_lock)
                {
                    return m_name;
                }
            }
        }

        public string Abbreviation
        {
            get
            {
                lock (m_lock)
                {
                    return m_abbreviation;
                }
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Fields

        private readonly string m_name = null;
        private readonly string m_abbreviation = null;
        private readonly object m_lock = new object();

        #endregion
    }
}

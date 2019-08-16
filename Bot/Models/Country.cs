using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class Country: BindableObject
    {
        #region Constructors

        public Country(CountryCode code, string name, string shortName, ObservableCollection<CoutryRegion> states)
        {
            m_name = name;
            m_shortName = shortName;
            m_states = states;
            m_code = code;
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

        public string ShortName
        {
            get
            {
                lock (m_lock)
                {
                    return m_shortName;
                }
            }
        }

        public ObservableCollection<CoutryRegion> States
        {
            get
            {
                lock (m_lock)
                {
                    return m_states;
                }
            }
        }

        public CountryCode Code
        {
            get
            {
                lock (m_lock)
                {
                    return m_code;
                }
            }
        }

        public bool IsStateRequired => States.Any();

        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Fields

        private readonly string m_name = null;
        private readonly string m_shortName = null;
        private readonly ObservableCollection<CoutryRegion> m_states = null;
        private readonly CountryCode m_code;
        private readonly object m_lock = new object();

        #endregion
    }

    public enum CountryCode
    {
        AT,
        BY,
        BE,
        BG,
        HR,
        CA,
        CZ,
        DK,
        EE,
        FI,
        FR,
        GB,
        DE,
        GR,
        HU,
        IS,
        IE,
        IT,
        JP,
        LV,
        LT,
        LU,
        MC,
        NB,
        NL,
        NO,
        PL,
        PT,
        RO,
        RU,
        SK,
        SI,
        ES,
        SE,
        CH,
        TR,
        UK,
        US,
        Undefined
    }
}

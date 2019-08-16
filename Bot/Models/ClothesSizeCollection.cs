using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public enum ClothesSizeSystemCode
    {
        ClothesUS,
        FootwearUS,
        PantsUS,
        ClothesEU,
        FootwearEU,
        JP
    }

    public class ClothesSizeSystem
    {
        #region Properties

        public ClothesSizeSystemCode Code { get; set; }
        public string Name { get; set; }
        public ObservableCollection<string> Sizes { get; set; }

        #endregion
    }

    public static class ClothesSizeSystemCollection
    {
        #region Properties

        public static Dictionary<ClothesSizeSystemCode, ClothesSizeSystem> SystemsDictionary
        {
            get
            {
                if (m_systems == null)
                {
                    Initialize();
                }

                return m_systems;
            }
        }

        public static Dictionary<ClothesSizeSystemCode, ClothesSizeSystem>.ValueCollection Systems => SystemsDictionary.Values;

        public static string RandomSize = "RANDOM";

        #endregion

        #region Methods

        private static void Initialize()
        {
            ObservableCollection<string> shoeSizesUs = new ObservableCollection<string>();
            
            shoeSizesUs.Add(RandomSize);
            for (int i = 0; i < 35; i++)
            {
                decimal size = 1.0m + 0.5m * i;
                shoeSizesUs.Add(size.ToString(CultureInfo.InvariantCulture));
            }

            m_systems = new Dictionary<ClothesSizeSystemCode, ClothesSizeSystem>()
            {
                {
                    ClothesSizeSystemCode.FootwearUS, new ClothesSizeSystem()
                    {
                        Code = ClothesSizeSystemCode.FootwearUS,
                        Name = "Footwear US",
                        Sizes = shoeSizesUs
                    }
                },
                {
                    ClothesSizeSystemCode.ClothesUS, new ClothesSizeSystem()
                    {
                        Code = ClothesSizeSystemCode.ClothesUS,
                        Name = "Clothes US",
                        Sizes = new ObservableCollection<string>() { RandomSize, "XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL"}
                    }
                },
                {
                    ClothesSizeSystemCode.PantsUS, new ClothesSizeSystem()
                    {
                        Code = ClothesSizeSystemCode.PantsUS,
                        Name = "Pants US",
                        Sizes = new ObservableCollection<string>() { RandomSize, "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40" }
                    }
                }
            };
        }

        #endregion

        #region Fields

        private static Dictionary<ClothesSizeSystemCode, ClothesSizeSystem> m_systems = null;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public static class ProductCategoriesCollection
    {
        #region Properties

        public static List<string> List { get; } = new List<string>()
        {
            "All",
            "New",
            "Jackets",
            "Shirts",
            "Tops/Sweaters",
            "Sweatshirts",
            "Pants",
            "Shorts",
            "T-Shirts",
            "Hats",
            "Bags",
            "Accessories",
            "Skate",
            "Shoes"
        };

        #endregion
    }
}

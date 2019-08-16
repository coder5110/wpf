using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Helpers
{
    public abstract class ObjectFactoryBase<Base> where Base: class 
    {
        #region Methods

        public abstract Base New();

        #endregion
    }
}

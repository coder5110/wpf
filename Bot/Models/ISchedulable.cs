using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public interface ISchedulable
    {
        #region Properteis

        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }

        #endregion
    }
}

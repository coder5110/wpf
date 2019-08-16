using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Models
{
    public enum ReleaseTaskState
    {
        Idle,
        Working
    }

    interface IReleaseTask
    {
        #region Properties

        ReleaseTaskState State { get; }

        #endregion

        #region methods

        Task<bool> Start(CancellationToken cancelToken);

        #endregion
    }
}

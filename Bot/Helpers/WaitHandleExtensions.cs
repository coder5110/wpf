using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Helpers
{
    public static class WaitHandleExtensions
    {
        #region Methods

        public static bool WaitAll(this WaitHandle waitHandle, WaitHandle[] waitHandles, CancellationToken cancelToken)
        {
            bool ret = true;

            WaitHandle[] events = new WaitHandle[waitHandles.Length + 1];
            Array.Copy(waitHandles, events, waitHandles.Length);
            events[waitHandles.Length] = cancelToken.WaitHandle;
            
            while (events.Length > 1)
            {
                WaitHandle firedEvent = events[WaitHandle.WaitAny(events)];
                if (firedEvent != cancelToken.WaitHandle)
                {
                    events = events.Where(e => e != firedEvent).ToArray();
                }
                else
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        public static bool WaitOne(this WaitHandle waitHandle, CancellationToken cancelToken)
        {
            bool ret = true;
            WaitHandle[] events = {waitHandle, cancelToken.WaitHandle};

            int id = WaitHandle.WaitAny(events);

            if (events[id] == cancelToken.WaitHandle)
            {
                ret = false;
            }

            return ret;
        }

        #endregion
    }
}

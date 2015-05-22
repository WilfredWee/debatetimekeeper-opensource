using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D.Timekeeper_Universal.Common
{
    public interface TimekeeperReceiver
    {
        Task updateTimeText(TimeSpan elapsed);
        Task autoRing(int amount);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QAudioSwitch
{
    static class Utils
    {
        public static void ScheduleUIAction(Dispatcher dispatcher, Action operation, DispatcherPriority priority = DispatcherPriority.Background)
        {
            dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate
                {
                    operation();
                    return null;
                }),
                null);
        }
    }
}

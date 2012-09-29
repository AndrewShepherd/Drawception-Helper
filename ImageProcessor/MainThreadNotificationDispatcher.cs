using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ImageProcessor
{
    public sealed class MainThreadNotificationDispatcher : INotificationDispatcher
    {
        public Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public void Begin(Action act)
        {
            _dispatcher.BeginInvoke(act);
        }
    }
}

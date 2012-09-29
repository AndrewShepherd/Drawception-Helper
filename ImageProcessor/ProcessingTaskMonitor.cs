using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    public class ProcessingTask
    {
        public string Description
        {
            get;
            set;
        }
    }

    public class ProcessingTaskMonitor : NotifyPropertyChangedImpl<ProcessingTaskMonitor, ImmediateNotificationDispatcher>
    {
        public IEnumerable<ProcessingTask> Tasks
        {
            get
            {
                return this._tasks.Values;
            }
        }

        private System.Collections.Concurrent.ConcurrentDictionary<long, ProcessingTask> _tasks = new System.Collections.Concurrent.ConcurrentDictionary<long, ProcessingTask>();

        private long _key = 0;

        public long OpenPanel()
        {
            var keyValue = System.Threading.Interlocked.Increment(ref _key);
            _tasks.TryAdd(_key, new ProcessingTask());
            return keyValue;
        }

        public void SetPanelText(long id, string text)
        {
            _tasks.AddOrUpdate(id, l => new ProcessingTask { Description = text }, (l, t) => new ProcessingTask { Description = text });
            Notify(t => t.Tasks);
        }


    }
}

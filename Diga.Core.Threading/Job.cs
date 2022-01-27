using System;
using System.Threading.Tasks;

namespace Diga.Core.Threading
{
    internal sealed class Job<T> : IJob
    {
        private readonly Action<T> _action;
        private readonly T _parameter;
        private readonly TaskCompletionSource<bool> _taskCompletionSource;

       

        public Job(Action<T> action, T parameter, DispatcherPriority priority, bool throwOnUiThread)
        {
            _action = action;
            _parameter = parameter;
            Priority = priority;
            if (throwOnUiThread)
                this._taskCompletionSource = null;
            else
                this._taskCompletionSource = new TaskCompletionSource<bool>();
        }

        /// <inheritdoc/>
        public DispatcherPriority Priority { get; }

        /// <inheritdoc/>
        void IJob.Run()
        {
            if (_taskCompletionSource == null)
            {
                _action(_parameter);
                return;
            }
            try
            {
                _action(_parameter);
                _taskCompletionSource.SetResult(default);
            }
            catch (Exception e)
            {
                _taskCompletionSource.SetException(e);
            }
        }
    }
    internal sealed class Job : IJob
    {
      
        private readonly Action _action;
        private readonly TaskCompletionSource<object> _taskCompletionSource;

        public Job(Action action, DispatcherPriority priority, bool throwOnUiThread)
        {
            this._action = action;
            this.Priority = priority;
            this._taskCompletionSource = throwOnUiThread ? null : new TaskCompletionSource<object>();
        }

        public DispatcherPriority Priority { get; }

        public Task Task => this._taskCompletionSource?.Task;
            
        void IJob.Run()
        {
            if (this._taskCompletionSource == null)
            {
                this._action();
                return;
            }
            try
            {
                this._action();
                this._taskCompletionSource.SetResult(null);
            }
            catch (Exception e)
            {
                this._taskCompletionSource.SetException(e);
            }
        }
    }
}
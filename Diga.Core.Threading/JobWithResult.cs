using System;
using System.Threading.Tasks;

namespace Diga.Core.Threading
{
    internal sealed class JobWithResult<TResult> : IJob
    {
        private readonly Func<TResult> _function;
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;

       
        public JobWithResult(Func<TResult> function, DispatcherPriority priority)
        {
            this._function = function;
            this.Priority = priority;
            this._taskCompletionSource = new TaskCompletionSource<TResult>();
        }

      
        public DispatcherPriority Priority { get; }

      
        public Task<TResult> Task => this._taskCompletionSource.Task;

        
        void IJob.Run()
        {
            try
            {
                var result = this._function();
                this._taskCompletionSource.SetResult(result);
            }
            catch (Exception e)
            {
                this._taskCompletionSource.SetException(e);
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Diga.Core.Threading
{
    public class UIDispatcher : IDispatcher
    {
        private readonly JobRunner _jobRunner;
        private IPlatformThreadingInterface _platform;

        public static UIDispatcher UIThread { get; } 


        private bool disposedValue;

        static UIDispatcher()
        {
            UIThread = new UIDispatcher(WinPlatform.Instance);
            AppDomain.CurrentDomain.ProcessExit += AppDomainProcessExit;
        }

        private static void AppDomainProcessExit(object sender, EventArgs e)
        {
            UIThread?.Dispose();
        }


        private UIDispatcher(IPlatformThreadingInterface platform)
        {
            
            this._platform = platform;
            this._jobRunner = new JobRunner(platform);
            
            if (this._platform != null)
            {
                this._platform.Signaled += this._jobRunner.RunJobs;
            }

           
        }

        /// <summary>
        /// Checks that the current thread is the UI thread.
        /// </summary>
        public bool CheckAccess() => this._platform?.CurrentThreadIsLoopThread ?? true;

        /// <summary>
        /// Checks that the current thread is the UI thread and throws if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current thread is not the UI thread.
        /// </exception>
        public void VerifyAccess()
        {
            if (!CheckAccess())
                throw new InvalidOperationException("Call from invalid thread");
        }

        ///// <summary>
        ///// Runs the dispatcher's main loop.
        ///// </summary>
        ///// <param name="cancellationToken">
        ///// A cancellation token used to exit the main loop.
        ///// </param>
        //public void MainLoop(CancellationToken cancellationToken)
        //{
        //    var platform = WinPlatform.Instance;
        //    cancellationToken.Register(() => platform.Signal(DispatcherPriority.Send));
        //    platform.RunLoop(cancellationToken);
        //}

        /// <summary>
        /// Runs continuations pushed on the loop.
        /// </summary>
        public void RunJobs()
        {
            this._jobRunner.RunJobs(null);
            
        }

        /// <summary>
        /// Use this method to ensure that more prioritized tasks are executed
        /// </summary>
        /// <param name="minimumPriority"></param>
        public void RunJobs(DispatcherPriority minimumPriority) => this._jobRunner.RunJobs(minimumPriority);

        /// <inheritdoc/>
        public Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            return this._jobRunner.InvokeAsync(action, priority);
        }
        
        /// <inheritdoc/>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return this._jobRunner.InvokeAsync(function, priority);
        }

        /// <inheritdoc/>
        public Task InvokeAsync(Func<Task> function, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return this._jobRunner.InvokeAsync(function, priority).Unwrap();
        }

        /// <inheritdoc/>
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return this._jobRunner.InvokeAsync(function, priority).Unwrap();
        }
        /// <inheritdoc/>
        public TResult Invoke<TResult>(Task<TResult> task)
        {
            TaskAwaiter<TResult> awaiter = task.GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                //Thread.Sleep(10);
                this._platform.DoEvents();
            }

            return awaiter.GetResult();
        }
        /// <inheritdoc/>
        public TResult Invoke<TResult>(Func<TResult> function)
        {
            return Invoke(InvokeAsync(function));
        }
        /// <inheritdoc/>
        public TResult Invoke<TResult>(Func<Task<TResult>> function)
        {
            return Invoke(InvokeAsync(function));
        }
        /// <inheritdoc/>
        public void Invoke(Task task)
        {
            TaskAwaiter awaiter = task.GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                //Thread.Sleep(10);
                this._platform.DoEvents();
            }
        }
        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            Invoke(InvokeAsync(action));
        }

        
        /// <inheritdoc/>
        public void Post(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            this._jobRunner.Post(action, priority);
        }

        /// <inheritdoc/>
        public void Post<T>(Action<T> action, T arg, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            this._jobRunner.Post(action, arg, priority);
        }

        /// <summary>
        /// This is needed for platform backends that don't have internal priority system (e. g. win32)
        /// To ensure that there are no jobs with higher priority
        /// </summary>
        /// <param name="currentPriority"></param>
        internal void EnsurePriority(DispatcherPriority currentPriority)
        {
            if (currentPriority == DispatcherPriority.MaxValue)
                return;
            currentPriority += 1;
            this._jobRunner.RunJobs(currentPriority);
        }

        /// <summary>
        /// Allows unit tests to change the platform threading interface.
        /// </summary>
        internal void UpdateServices()
        {
            if (this._platform != null)
            {
                this._platform.Signaled -= this._jobRunner.RunJobs;
            }

            this._platform = WinPlatform.Instance;
            this._jobRunner.UpdateServices();

            if (this._platform != null)
            {
                this._platform.Signaled += this._jobRunner.RunJobs;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                this._platform?.Dispose();
                disposedValue = true;
            }
        }

   
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Diga.Core.Threading
{
    public class UIDispatcherFinalDisposeException : Exception
    {
        public UIDispatcherFinalDisposeException(string message):base(message)
        {
            
        }
    }
    public class UIDispatcher : IDispatcher
    {
        private readonly JobRunner _jobRunner;
        private IPlatformThreadingInterface _platform;

        public static UIDispatcher UIThread { get; }
        public static volatile bool FilnalDisposed = false;

        private bool disposedValue;

        static UIDispatcher()
        {
            UIThread = new UIDispatcher(WinPlatform.Instance);
            //AppDomain.CurrentDomain.ProcessExit += AppDomainProcessExit;
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

        public bool InvokeRequired => this._platform.InvokeRequired;

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
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function,
            DispatcherPriority priority = DispatcherPriority.Normal)
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
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function,
            DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return this._jobRunner.InvokeAsync(function, priority).Unwrap();
        }
        /// <inheritdoc/>
        public void DoEvents()
        {
            this._platform?.DoEvents();
        }
        /// <inheritdoc/>
        public void Wait(int milliSeconds)
        {
            var start = Environment.TickCount;

            while (true)
            {
                if (FilnalDisposed)
                {
                    break;
                }
                DoEvents();
                var diff = Environment.TickCount - start;
                if (diff > milliSeconds)
                {
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public Task WaitAsyn(int milliSeconds)
        {
            return Task.Run(() =>
            {
                var start = Environment.TickCount;

                while (true)
                {
                    if (FilnalDisposed)
                    {
                        break;
                    }
                    DoEvents();
                    var diff = Environment.TickCount - start;
                    if (diff > milliSeconds)
                    {
                        break;
                    }
                }
            });
        }
        /// <inheritdoc/>
        public TResult Invoke<TResult>(Task<TResult> task)
        {
            TaskAwaiter<TResult> awaiter = task.GetAwaiter();

            while (!awaiter.IsCompleted)
            {
                if (FilnalDisposed)
                {

                    try
                    {
                        this._jobRunner.JobsClear();
                        task.Dispose();
                    }
                    catch (Exception)
                    {
                        throw new UIDispatcherFinalDisposeException("UIDispatcher.Invoke<TResult>: Final Disposed! => You close The application while Task is running!");    
                    }
                    
                    
                }
                    
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
                if (FilnalDisposed)
                {
                    try
                    {
                        this._jobRunner.JobsClear();
                        task.Dispose();
                    }
                    catch (Exception)
                    {
                        throw new UIDispatcherFinalDisposeException("UIDispatcher.Invoke: Final Disposed! => You close The application while Task is running!");    
                    }

                }
                    

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
                FilnalDisposed = true;
                this._platform?.Dispose();
                disposedValue = true;
            }
        }


        public void Dispose()
        {
            if (!FilnalDisposed)
            {

                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
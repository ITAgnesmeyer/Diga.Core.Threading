﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diga.Core.Threading
{
    internal class JobRunner
    {
        private IPlatformThreadingInterface _platform;

        private readonly Queue<IJob>[] _queues = Enumerable.Range(0, (int)DispatcherPriority.MaxValue + 1)
            .Select(_ => new Queue<IJob>()).ToArray();

        public JobRunner(IPlatformThreadingInterface platform)
        {
            _platform = platform;
        }

        /// <summary>
        /// Runs continuations pushed on the loop.
        /// </summary>
        /// <param name="priority">Priority to execute jobs for. Pass null if platform doesn't have internal priority system</param>
        public void RunJobs(DispatcherPriority? priority)
        {
            var minimumPriority = priority ?? DispatcherPriority.MinValue;
            while (true)
            {
                var job = GetNextJob(minimumPriority);
                if (job == null)
                    return;

                job.Run();
            }
        }

        /// <summary>
        /// Invokes a method on the main loop.
        /// </summary>
        /// <param name="action">The method.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        /// <returns>A task that can be used to track the method's execution.</returns>
        public Task InvokeAsync(Action action, DispatcherPriority priority)
        {
            var job = new Job(action, priority, false);
            AddJob(job);
            return job.Task;
        }

        /// <summary>
        /// Invokes a method on the main loop.
        /// </summary>
        /// <param name="function">The method.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        /// <returns>A task that can be used to track the method's execution.</returns>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority)
        {
            var job = new JobWithResult<TResult>(function, priority);
            AddJob(job);
            return job.Task;
        }

        /// <summary>
        /// Post action that will be invoked on main thread
        /// </summary>
        /// <param name="action">The method.</param>
        /// 
        /// <param name="priority">The priority with which to invoke the method.</param>
        public void Post(Action action, DispatcherPriority priority)
        {
            AddJob(new Job(action, priority, true));
        }

        /// <summary>
        /// Post action that will be invoked on main thread
        /// </summary>
        /// <param name="action">The method to call.</param>
        /// <param name="parameter">The parameter of method to call.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        public void Post<T>(Action<T> action, T parameter, DispatcherPriority priority)
        {
            AddJob(new Job<T>(action, parameter, priority, true));
        }

        /// <summary>
        /// Allows unit tests to change the platform threading interface.
        /// </summary>
        internal void UpdateServices()
        {
            _platform = WinPlatform.Instance;
        }

        private void AddJob(IJob job)
        {
            bool needWake;
            var queue = _queues[(int)job.Priority];
            lock (queue)
            {
                needWake = queue.Count == 0;
                queue.Enqueue(job);
            }
            if (needWake)
                _platform?.Signal(job.Priority);
        }

        public void JobsClear()
        {
            int len = this._queues.Length;
            for (int i = 0; i < len; i++)
            {
                this._queues[i].Clear();
                
            }
        }
        private IJob GetNextJob(DispatcherPriority minimumPriority)
        {
            for (int c = (int)DispatcherPriority.MaxValue; c >= (int)minimumPriority; c--)
            {
                var q = _queues[c];
                lock (q)
                {
                    if (q.Count > 0)
                        return q.Dequeue();
                }
            }
            return null;
        }
    }
}

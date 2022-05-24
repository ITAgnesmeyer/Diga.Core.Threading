using System;
using System.Threading.Tasks;

namespace Diga.Core.Threading
{
    public interface IDispatcher:IDisposable
    {
        /// <summary>
        /// Determines whether the calling thread is the thread associated with this <see cref="IDispatcher"/>.
        /// </summary>
        /// <returns>True if he calling thread is the thread associated with the dispatched, otherwise false.</returns>
        bool CheckAccess();

        /// <summary>
        /// Throws an exception if the calling thread is not the thread associated with this <see cref="IDispatcher"/>.
        /// </summary>
        void VerifyAccess();

        /// <summary>
        /// Posts an action that will be invoked on the dispatcher thread.
        /// </summary>
        /// <param name="action">The method.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        void Post(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

        /// <summary>
        /// Posts an action that will be invoked on the dispatcher thread.
        /// </summary>
        /// <typeparam name="T">type of argument</typeparam>
        /// <param name="action">The method to call.</param>
        /// <param name="arg">The argument of method to call.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        void Post<T>(Action<T> action, T arg, DispatcherPriority priority = DispatcherPriority.Normal);

        /// <summary>
        /// Invokes a action on the dispatcher thread.
        /// </summary>
        /// <param name="action">The method.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        /// <returns>A task that can be used to track the method's execution.</returns>
        Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

        /// <summary>
        /// Invokes a method on the dispatcher thread.
        /// </summary>
        /// <param name="function">The method.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        /// <returns>A task that can be used to track the method's execution.</returns>
        Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority = DispatcherPriority.Normal);
        /// <summary>
        /// Invoke Task sync
        /// </summary>
        /// <typeparam name="TResult">Result</typeparam>
        /// <param name="task">Taks</param>
        /// <returns>Result</returns>
        TResult Invoke<TResult>(Task<TResult> task);
        /// <summary>
        /// Invoke Function on UIThread Sync
        /// </summary>
        /// <typeparam name="TResult">Result</typeparam>
        /// <param name="function">Function</param>
        /// <returns>Result</returns>
        TResult Invoke<TResult>(Func<TResult> function);

        /// <summary>
        /// Invoke Function on UIThread Sync
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        TResult Invoke<TResult>(Func<Task<TResult>> function);
        /// <summary>
        /// Execute Task Sync
        /// </summary>
        /// <param name="task">Task to execute</param>
        void Invoke(Task task);
        /// <summary>
        /// Invoke Action on UIThread Sync
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Invoke(Action action);

        /// <summary>
        /// Queues the specified work to run on the dispatcher thread and returns a proxy for the
        /// task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        /// <returns>A task that represents a proxy for the task returned by <paramref name="function"/>.</returns>
        Task InvokeAsync(Func<Task> function, DispatcherPriority priority = DispatcherPriority.Normal);

        /// <summary>
        /// Queues the specified work to run on the dispatcher thread and returns a proxy for the
        /// task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="priority">The priority with which to invoke the method.</param>
        /// <returns>A task that represents a proxy for the task returned by <paramref name="function"/>.</returns>
        Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, DispatcherPriority priority = DispatcherPriority.Normal);

        /// <summary>
        /// Call DoEvents on UI-Thread
        /// </summary>
        void DoEvents();

        /// <summary>
        /// Wait without blocking UI-Thread
        /// </summary>
        /// <param name="milliSeconds">Milliseconds to wait</param>
        void Wait(int milliSeconds);

        /// <summary>
        /// Wait Async. Internally calls DoEvent
        /// </summary>
        /// <param name="milliSeconds">Milliseconds to wait</param>
        /// <returns>Task</returns>
        Task WaitAsyn(int milliSeconds);
    }
}
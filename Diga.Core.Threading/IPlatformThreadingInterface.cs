using System;
using System.Threading;

namespace Diga.Core.Threading
{
    public interface IPlatformThreadingInterface: IDisposable
    {
        void RunLoop(CancellationToken cancellationToken);

        /// <summary>
        /// Starts a timer.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="interval">The interval.</param>
        /// <param name="tick">The action to call on each tick.</param>
        /// <returns>An <see cref="IDisposable"/> used to stop the timer.</returns>
        IDisposable StartTimer(DispatcherPriority priority, TimeSpan interval, Action tick);

        void Signal(DispatcherPriority priority);

        bool InvokeRequired { get; }
        bool CurrentThreadIsLoopThread { get; }

        event Action<DispatcherPriority?> Signaled;

        void DoEvents();
    }
}
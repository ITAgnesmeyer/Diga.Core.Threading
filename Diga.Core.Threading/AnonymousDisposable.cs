using System;
using System.Threading;

namespace Diga.Core.Threading
{
    internal sealed class AnonymousDisposable : ICancelable
    {
        private volatile Action _dispose;

        /// <summary>
        /// Constructs a new disposable with the given action used for disposal.
        /// </summary>
        /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
        public AnonymousDisposable(Action dispose)
        {
            System.Diagnostics.Debug.Assert(dispose != null);

            this._dispose = dispose;
        }

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public bool IsDisposed => this._dispose == null;

        /// <summary>
        /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref this._dispose, null)?.Invoke();
        }
    }
}
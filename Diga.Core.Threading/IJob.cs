namespace Diga.Core.Threading
{
    internal interface IJob
    {
        /// <summary>
        /// Gets the job priority.
        /// </summary>
        DispatcherPriority Priority { get; }

        /// <summary>
        /// Runs the job.
        /// </summary>
        void Run();
    }
}
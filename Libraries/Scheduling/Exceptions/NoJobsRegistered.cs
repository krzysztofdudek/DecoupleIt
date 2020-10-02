using System;

namespace GS.DecoupleIt.Scheduling.Exceptions
{
    /// <summary>
    ///     Exception is thrown when scheduler is tried to run, but there are no registered jobs.
    /// </summary>
    public sealed class NoJobsRegistered : Exception
    {
        /// <inheritdoc />
        public override string Message { get; } = "Job jobs are registered.";

        internal NoJobsRegistered() { }
    }
}

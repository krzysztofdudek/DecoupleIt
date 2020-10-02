using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling.Exceptions
{
    /// <summary>
    ///     Exception is thrown when one of jobs' schedule configurations is invalid.
    /// </summary>
    public sealed class InvalidScheduleConfiguration : Exception
    {
        /// <inheritdoc />
        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public override string Message => $"Invalid schedule configuration. {_reason}";

        internal InvalidScheduleConfiguration([NotNull] string reason)
        {
            _reason = reason;
        }

        [NotNull]
        private readonly string _reason;
    }
}

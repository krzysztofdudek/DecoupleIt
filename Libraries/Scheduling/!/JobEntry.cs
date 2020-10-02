using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Represents a single job with it's properties of scheduling defined as an attribute.
    /// </summary>
    public readonly struct JobEntry : IEquatable<JobEntry>
    {
        /// <summary>
        ///     Type of the job.
        /// </summary>
        [NotNull]
        public readonly Type JobType;

        /// <summary>
        ///     Attribute defining schedule.
        /// </summary>
        [NotNull]
        public readonly IScheduleAttribute Attribute;

        internal JobEntry([NotNull] Type jobType, [NotNull] IScheduleAttribute attribute)
        {
            JobType   = jobType;
            Attribute = attribute;
        }

        /// <inheritdoc />
        public bool Equals(JobEntry other)
        {
            return JobType == other.JobType && Attribute.Equals(other.Attribute);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is JobEntry other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (JobType.GetHashCode() * 397) ^ Attribute.GetHashCode();
            }
        }
    }
}

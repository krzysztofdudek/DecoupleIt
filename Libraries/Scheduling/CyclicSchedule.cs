using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Marks the job class to be registered automatically. This type of a job is running from the start of an
    ///     application with specific period and count of repetitions.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [BaseTypeRequired(typeof(IJob))]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [PublicAPI]
    public sealed class CyclicSchedule : Attribute, IScheduleAttribute
    {
        /// <summary>
        ///     Days.
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        ///     Hours.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        ///     Milliseconds.
        /// </summary>
        public int Milliseconds { get; set; }

        /// <summary>
        ///     Minutes.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        ///     Repeat count. If value is equal to or less than zero, job will be repeated forever.
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        ///     Seconds.
        /// </summary>
        public int Seconds { get; set; }

        /// <inheritdoc />
        public string Validate()
        {
            if (Days == 0 && Hours == 0 && Minutes == 0 && Seconds == 0 && Milliseconds == 0)
                return "All properties related to time are equal to zero.";

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling.Exceptions
{
    /// <summary>
    ///     Exception is thrown when scan of the assembly founds jobs with more than one scheduling attribute.
    /// </summary>
    public sealed class AmbiguousSchedulingAttributes : Exception
    {
        /// <inheritdoc />
        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public override string Message
        {
            get
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append(_jobTypes.First()
                                              .AsNotNull()
                                              .FullName);

                foreach (var jobType in _jobTypes.Skip(1))
                    stringBuilder.Append($"{jobType.AsNotNull().FullName}, ");

                return $"Jobs: {stringBuilder} has multiple scheduling attributes.";
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="AmbiguousSchedulingAttributes" />.
        /// </summary>
        /// <param name="jobTypes">Jobs types.</param>
        internal AmbiguousSchedulingAttributes([NotNull] [ItemNotNull] IReadOnlyCollection<Type> jobTypes)
        {
            _jobTypes = jobTypes;
        }

        [NotNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<Type> _jobTypes;
    }
}

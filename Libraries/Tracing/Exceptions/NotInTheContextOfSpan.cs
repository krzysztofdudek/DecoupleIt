using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing.Exceptions
{
    /// <summary>
    ///     Exception is thrown when request for current span fails because current thread is not in the context of any span.
    ///     Class is not inheritable.
    /// </summary>
    public sealed class NotInTheContextOfSpan : Exception
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public override string Message => "Current thread is not in the context of any span.";

        /// <summary>
        ///     Creates an instance of <see cref="NotInTheContextOfSpan" />.
        /// </summary>
        internal NotInTheContextOfSpan() { }
    }
}

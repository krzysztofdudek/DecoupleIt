using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing.Exceptions
{
    /// <summary>
    ///     Exception is thrown when <see cref="Tracer.GetResource{TResource}" /> won't find resource. Class is not
    ///     inheritable.
    /// </summary>
    public sealed class ResourceNotFound : Exception
    {
        /// <inheritdoc />
        [NotNull]
        public override string Message => "Resource not found.";

        internal ResourceNotFound() { }
    }
}

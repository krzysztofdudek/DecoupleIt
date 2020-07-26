using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing.Exceptions
{
    /// <summary>
    ///     Exception is thrown on requesting access to trace stack without it's initialization prior.
    ///     Class is not inheritable.
    /// </summary>
    public sealed class TraceIsNotInitialized : Exception
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [NotNull]
        public override string Message => "Trace was not initialized. The best option is to initialize it at the beginning of the thread.";

        /// <summary>
        ///     Creates an an instance of <see cref="TraceIsNotInitialized" />.
        /// </summary>
        internal TraceIsNotInitialized() { }
    }
}

using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing.Exceptions
{
    /// <summary>
    ///     Exception is thrown when request for creation of root span but there is a one.
    ///     Class is not inheritable.
    /// </summary>
    public sealed class RootSpanIsAlreadyOpened : Exception
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [NotNull]
#if NETCOREAPP3_1
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
#endif
        public override string Message => "Root span already opened.";

        /// <summary>
        ///     Creates an instance of <see cref="RootSpanIsAlreadyOpened" />.
        /// </summary>
        internal RootSpanIsAlreadyOpened() { }
    }
}

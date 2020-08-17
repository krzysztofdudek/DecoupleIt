using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing.Exceptions
{
    /// <summary>
    ///     Exception is thrown when request for creation of child span but there is not a root span opened.
    ///     Class is not inheritable.
    /// </summary>
    public sealed class RootSpanIsNotOpened : Exception
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [NotNull]
#if NETCOREAPP3_1
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
#endif
        public override string Message => "Root span is not opened.";

        /// <summary>
        ///     Creates an instance of <see cref="RootSpanIsNotOpened" />.
        /// </summary>
        internal RootSpanIsNotOpened() { }
    }
}

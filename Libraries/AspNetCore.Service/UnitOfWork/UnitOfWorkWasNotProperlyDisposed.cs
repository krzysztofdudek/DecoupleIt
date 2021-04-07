using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.AspNetCore.Service.UnitOfWork
{
    /// <summary>
    ///     Exception thrown when given unit of work wasn't disposed properly by one of inner levels.
    /// </summary>
    public sealed class UnitOfWorkWasNotProperlyDisposed : Exception
    {
        /// <inheritdoc />
        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public override string Message =>
            "Unit of work was not correctly disposed. It usually means that one of inner calls of IUnitOfWork " +
            "that created instance of unit of work was called without disposal of such instance (missing Dispose call)." + $"\nStack trace:\n{_stackTrace}";

        public UnitOfWorkWasNotProperlyDisposed([CanBeNull] string stackTrace)
        {
            _stackTrace = stackTrace;
        }

        [CanBeNull]
        private readonly string _stackTrace;
    }
}

using System;

namespace GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore
{
    /// <summary>
    ///     Exception thrown when given unit of work wasn't disposed properly by one of inner levels.
    /// </summary>
    public sealed class UnitOfWorkWasNotProperlyDisposed : Exception
    {
        /// <inheritdoc />
        public override string Message { get; } = "Unit of work was not correctly disposed. It usually means that one of inner calls of IUnitOfWork " +
                                                  "that created instance of unit of work was called without disposal of such instance (missing Dispose call).";
    }
}

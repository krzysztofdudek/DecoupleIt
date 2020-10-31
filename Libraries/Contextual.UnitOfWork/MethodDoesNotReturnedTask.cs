using System;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Exception called when method had to return a task but it didn't.
    /// </summary>
    public sealed class MethodDoesNotReturnedTask : Exception
    {
        /// <inheritdoc />
        public override string Message { get; } = "Method does not returned task.";
    }
}

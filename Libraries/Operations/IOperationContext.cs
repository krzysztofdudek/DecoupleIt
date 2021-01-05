using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Operation context.
    /// </summary>
    [ProvidesContext]
    public interface IOperationContext
    {
        /// <summary>
        ///     Opens new scope.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        IOperationContextScope OpenScope();
    }
}

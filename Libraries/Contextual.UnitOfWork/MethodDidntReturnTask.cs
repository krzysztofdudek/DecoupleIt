using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Exception called when method had to return a task but it didn't.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
    public sealed class MethodDidntReturnTask : Exception
    {
        /// <inheritdoc />
        [NotNull]
        public override string Message => "Method does not returned task.";
    }
}

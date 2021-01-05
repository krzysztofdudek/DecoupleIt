using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    internal interface ICommandWithResult : IOperation
    {
        [NotNull]
        Type ResultType { get; }
    }
}

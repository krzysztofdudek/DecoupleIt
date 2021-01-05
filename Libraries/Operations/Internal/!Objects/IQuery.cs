using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    internal interface IQuery : IOperation
    {
        [NotNull]
        Type ResultType { get; }
    }
}

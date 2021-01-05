using System;

namespace GS.DecoupleIt.Operations.Internal
{
    internal interface IOperation
    {
        DateTime CreateDateTime { get; }

        Guid OperationId { get; }
    }
}

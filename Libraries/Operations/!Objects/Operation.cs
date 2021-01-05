using System;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all operations.
    /// </summary>
    [PublicAPI]
    public abstract class Operation : IOperation
    {
        /// <summary>
        ///     Date and time of creation of this operation.
        /// </summary>
        public DateTime CreateDateTime { get; [UsedImplicitly] private set; }

        /// <summary>
        ///     Operation identifier.
        /// </summary>
        public Guid OperationId { get; [UsedImplicitly] private set; }

        protected Operation()
        {
            OperationId    = Guid.NewGuid();
            CreateDateTime = DateTime.UtcNow;
        }
    }
}

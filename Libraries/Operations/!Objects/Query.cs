using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all queries.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class Query<TResult> : Operation, IQuery
    {
        /// <summary>
        ///     Dispatches this query.
        /// </summary>
        /// <returns>Result.</returns>
        [CanBeNull]
        public TResult Dispatch()
        {
            return OperationDispatcher.DispatchQueryAsync(this, CancellationToken.None)
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                                      .AsTask()
#endif
                                      .GetAwaiter()
                                      .GetResult();
        }

        /// <summary>
        ///     Dispatches this query.
        /// </summary>
        /// <returns>Result.</returns>
        [NotNull]
        [ItemCanBeNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            DispatchAsync(CancellationToken cancellationToken = default)
        {
            return OperationDispatcher.DispatchQueryAsync(this, CancellationToken.None);
        }

        Type IQuery.ResultType => typeof(TResult);
    }
}

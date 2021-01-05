using System;
using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Dispatch()
        {
            return OperationDispatcher.DispatchQueryAsync(this, CancellationToken.None)
                                      .GetAwaiter()
                                      .GetResult();
        }

        /// <summary>
        ///     Dispatches this query.
        /// </summary>
        /// <returns>Result.</returns>
        [NotNull]
        [ItemCanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

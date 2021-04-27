using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all query handlers.
    /// </summary>
    /// <typeparam name="TQuery">Command type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    [Singleton]
    [RegisterManyTimes]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class QueryHandlerBase<TQuery, TResult> : IQueryHandler
        where TQuery : Query<TResult>
    {
        /// <summary>
        ///     Handles a query.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        [ItemCanBeNull]
        protected abstract
#if NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            HandleAsync([NotNull] TQuery query, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async
#if NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            IQueryHandler.HandleAsync(IQuery query, CancellationToken cancellationToken)
        {
            if (query is not TQuery typedQuery)
                throw new ArgumentException("Query is of invalid type.", nameof(query));

            return await HandleAsync(typedQuery, cancellationToken);
        }
    }
}

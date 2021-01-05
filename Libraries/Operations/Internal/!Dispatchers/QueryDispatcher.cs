using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations.Internal
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    internal sealed class QueryDispatcher : DispatcherBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        public QueryDispatcher(
            [NotNull] IExtendedLoggerFactory extendedLoggerFactory,
            [NotNull] OperationHandlerFactory operationHandlerFactory,
            [NotNull] ITracer tracer) : base(extendedLoggerFactory.Create<QueryDispatcher>(), operationHandlerFactory, tracer) { }

        [NotNull]
        [ItemCanBeNull]
        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            DispatchAsync([NotNull] IQuery query, CancellationToken cancellationToken = default)
        {
            using var span = Tracer.OpenSpan(query.GetType(), SpanType.Query);

            var handlers = OperationHandlerFactory.GetQueryHandlers(query)
                                                  .ToList();

            if (handlers.Count == 0)
            {
                Logger.LogInformation("Dispatching query {@OperationAction}, but no handlers found.", "started");

                return null;
            }

            Logger.LogInformation("Dispatching query {@OperationAction}, {@OperationHandlersCount} will handle it.", "started", handlers.Count);

            try
            {
                var result = await ProcessHandlers(query, handlers, cancellationToken);

                Logger.LogInformation("Dispatching query {@OperationAction} after {@OperationDuration}ms.", "finished", span.Duration.Milliseconds);

                return result;
            }
            catch
            {
                Logger.LogInformation("Dispatching query {@OperationAction} after {@OperationDuration}ms.", "failed", span.Duration.Milliseconds);

                throw;
            }
        }

        [NotNull]
        [ItemCanBeNull]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            ProcessHandler([NotNull] IQuery query, [NotNull] IQueryHandler handler, CancellationToken cancellationToken)
        {
            using var span = Tracer.OpenSpan(handler.GetType(), SpanType.QueryHandler);

            Logger.LogInformation("Query handler invocation {@OperationAction}.", "started");

            try
            {
                var result = await handler.HandleAsync(query, cancellationToken);

                Logger.LogInformation("Query handler invocation {@OperationAction} after {@OperationDuration}ms.", "finished", span.Duration.Milliseconds);

                return result;
            }
            catch (Exception exception)
            {
                Logger.LogInformation(exception,
                                      "Query handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                      "failed",
                                      span.Duration.Milliseconds);

                throw;
            }
        }

        [NotNull]
        [ItemCanBeNull]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            ProcessHandlers([NotNull] IQuery query, [NotNull] [ItemNotNull] IEnumerable<IQueryHandler> handlers, CancellationToken cancellationToken)
        {
            object result = null;

            foreach (var handler in handlers)
                result = await ProcessHandler(query, handler, cancellationToken);

            return result;
        }
    }
}

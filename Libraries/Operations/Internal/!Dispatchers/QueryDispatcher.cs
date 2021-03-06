using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations.Internal
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    internal sealed class QueryDispatcher : DispatcherBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        public QueryDispatcher([NotNull] IExtendedLoggerFactory extendedLoggerFactory, [NotNull] ITracer tracer, [NotNull] IServiceProvider serviceProvider) :
            base(extendedLoggerFactory.Create<QueryDispatcher>(), tracer, serviceProvider) { }

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

            using var serviceProviderScope = ServiceProvider.CreateScope();

            var handlers = OperationHandlerFactory.GetQueryHandlers(serviceProviderScope.ServiceProvider, query);

            Logger.LogDebug("Dispatching query {@OperationAction}.", "started");

            try
            {
                var result = await ProcessHandlers(query, handlers, cancellationToken);

                Logger.LogDebug("Dispatching query {@OperationAction} after {@OperationDuration}ms.", "finished", span.Duration.Milliseconds);

                return result;
            }
            catch
            {
                Logger.LogDebug("Dispatching query {@OperationAction} after {@OperationDuration}ms.", "failed", span.Duration.Milliseconds);

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
                Logger.LogError(exception,
                                "Query handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                "failed",
                                span.Duration.Milliseconds);

                exception.Data.Add("WasHandled", true);

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

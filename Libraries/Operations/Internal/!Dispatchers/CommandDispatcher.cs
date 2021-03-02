using System;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
    internal sealed class CommandDispatcher : DispatcherBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        public CommandDispatcher(
            [NotNull] IExtendedLoggerFactory extendedLoggerFactory,
            [NotNull] OperationHandlerFactory operationHandlerFactory,
            [NotNull] ITracer tracer,
            [NotNull] IOperationContext operationContext) : base(extendedLoggerFactory.Create<CommandDispatcher>(), operationHandlerFactory, tracer)
        {
            _operationContext = operationContext;
        }

        [NotNull]
        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchAsync([NotNull] ICommand command, CancellationToken cancellationToken = default)
        {
            await DispatchAsync((object) command, cancellationToken);
        }

        [NotNull]
        [ItemCanBeNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            DispatchAsync([NotNull] ICommandWithResult command, CancellationToken cancellationToken = default)
        {
            return DispatchAsync((object) command, cancellationToken);
        }

        [NotNull]
        private readonly IOperationContext _operationContext;

        [NotNull]
        [ItemCanBeNull]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            DispatchAsync([NotNull] object command, CancellationToken cancellationToken = default)
        {
            using var span = Tracer.OpenSpan(command.GetType(), SpanType.Command);

            Logger.LogDebug("Dispatching command {@OperationAction}.", "started");

            try
            {
                object result = null;

                switch (command)
                {
                    case ICommand typedCommand:
                        await InvokeCommandHandler(typedCommand, cancellationToken);

                        break;
                    case ICommandWithResult typedCommand:
                        result = await InvokeCommandWithResultHandler(typedCommand, cancellationToken);

                        break;
                    default:
                        return default;
                }

                Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "finished", span.Duration.Milliseconds);

                return result;
            }
            catch
            {
                Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "failed", span.Duration.Milliseconds);

                throw;
            }
        }

        private async Task InvokeCommandHandler([NotNull] ICommand typedCommand, CancellationToken cancellationToken)
        {
            foreach (var preCommandHandler in OperationHandlerFactory.GetPreCommandHandlers(typedCommand))
            {
                using var tracerSpan = Tracer.OpenSpan(preCommandHandler.GetType(), SpanType.PreCommandHandler);

                try
                {
                    await preCommandHandler.PreHandleAsync(typedCommand, cancellationToken);
                }
                catch (Exception preCommandHandlerException)
                {
                    Logger.LogError(preCommandHandlerException, "Pre command handler invocation {@OperationAction}.", "failed");

                    preCommandHandlerException.Data.Add("WasHandled", true);

                    throw;
                }
            }

            foreach (var commandHandler in OperationHandlerFactory.GetCommandHandlers(typedCommand))
            {
                var       tracerSpan            = Tracer.OpenSpan(commandHandler.GetType(), SpanType.CommandHandler);
                using var operationContextScope = _operationContext.OpenScope();

                Logger.LogInformation("Command handler invocation {@OperationAction}.", "started");

                try
                {
                    await operationContextScope.DispatchOperationsAsync(() => commandHandler.HandleAsync(typedCommand, cancellationToken), cancellationToken);

                    Logger.LogInformation("Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                          "finished",
                                          tracerSpan.Duration.Milliseconds);

                    tracerSpan.Dispose();

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandHandlers(typedCommand))
                    {
                        using var _ = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     operationContextScope.InternalEvents,
                                                                     null,
                                                                     cancellationToken);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException, "Post command handler invocation {@OperationAction}.", "failed");
                        }
                    }
                }
                catch (Exception commandHandlerException)
                {
                    Logger.LogError(commandHandlerException,
                                    "Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                    "failed",
                                    tracerSpan.Duration.Milliseconds);

                    tracerSpan.Dispose();

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandHandlers(typedCommand))
                    {
                        using var _ = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     operationContextScope.InternalEvents,
                                                                     commandHandlerException,
                                                                     cancellationToken);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException, "Post command handler invocation {@OperationAction}.", "failed");
                        }
                    }

                    commandHandlerException.Data.Add("WasHandled", true);

                    throw;
                }
            }
        }

        private async Task<object> InvokeCommandWithResultHandler([NotNull] ICommandWithResult typedCommand, CancellationToken cancellationToken)
        {
            object result = null;

            foreach (var preCommandHandler in OperationHandlerFactory.GetPreCommandWithResultHandlers(typedCommand))
            {
                using var tracerSpan = Tracer.OpenSpan(preCommandHandler.GetType(), SpanType.PreCommandHandler);

                try
                {
                    await preCommandHandler.PreHandleAsync(typedCommand, cancellationToken);
                }
                catch (Exception preCommandHandlerException)
                {
                    Logger.LogError(preCommandHandlerException, "Pre command handler invocation {@OperationAction}.", "failed");

                    preCommandHandlerException.Data.Add("WasHandled", true);

                    throw;
                }
            }

            foreach (var commandHandler in OperationHandlerFactory.GetCommandHandlersWithResult(typedCommand))
            {
                var       tracerSpan            = Tracer.OpenSpan(commandHandler.GetType(), SpanType.CommandHandler);
                using var operationContextScope = _operationContext.OpenScope();
                object    tempResult            = null;

                Logger.LogInformation("Command handler invocation {@OperationAction}.", "started");

                try
                {
                    await operationContextScope.DispatchOperationsAsync(
                        async () => tempResult = await commandHandler.HandleAsync(typedCommand, cancellationToken),
                        cancellationToken);

                    Logger.LogInformation("Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                          "finished",
                                          tracerSpan.Duration.Milliseconds);

                    tracerSpan.Dispose();

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandWithResultHandlers(typedCommand))
                    {
                        using var _ = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     tempResult,
                                                                     operationContextScope.InternalEvents,
                                                                     null,
                                                                     cancellationToken);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException, "Post command handler invocation {@OperationAction}.", "failed");
                        }
                    }

                    result = tempResult;
                }
                catch (Exception commandHandlerException)
                {
                    Logger.LogError(commandHandlerException,
                                    "Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                    "failed",
                                    tracerSpan.Duration.Milliseconds);

                    tracerSpan.Dispose();

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandWithResultHandlers(typedCommand))
                    {
                        using var _ = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     tempResult,
                                                                     operationContextScope.InternalEvents,
                                                                     commandHandlerException,
                                                                     cancellationToken);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException, "Post command handler invocation {@OperationAction}.", "failed");
                        }
                    }

                    commandHandlerException.Data.Add("WasHandled", true);

                    throw;
                }
            }

            return result;
        }
    }
}

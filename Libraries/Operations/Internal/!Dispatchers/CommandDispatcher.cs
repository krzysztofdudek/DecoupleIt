using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            [NotNull] ITracer tracer,
            [NotNull] IOperationContext operationContext,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IOptions<Options> options) : base(extendedLoggerFactory.Create<CommandDispatcher>(),
                                                        tracer,
                                                        serviceProvider,
                                                        options)
        {
            _operationContext = operationContext;
        }

        [NotNull]
        public async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchAsync([NotNull] ICommand command, CancellationToken cancellationToken = default)
        {
            using var span = Tracer.OpenSpan(command.GetType(), SpanType.Command);

            if (Options.Logging.EnableNonErrorLogging)
                Logger.LogDebug("Dispatching command {@OperationAction}.", "started");

            try
            {
                using var serviceProviderScope = ServiceProvider.CreateScope();

                await InvokeCommandHandler(command, serviceProviderScope!.ServiceProvider!, cancellationToken);

                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "finished", span.Duration.Milliseconds);
            }
            catch (OperationCanceledException)
            {
                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "cancelled", span.Duration.Milliseconds);

                throw;
            }
            catch
            {
                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "failed", span.Duration.Milliseconds);

                throw;
            }
        }

        [NotNull]
        [ItemCanBeNull]
        public async
#if NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            DispatchAsync([NotNull] ICommandWithResult command, CancellationToken cancellationToken = default)
        {
            using var span = Tracer.OpenSpan(command.GetType(), SpanType.Command);

            if (Options.Logging.EnableNonErrorLogging)
                Logger.LogDebug("Dispatching command {@OperationAction}.", "started");

            try
            {
                using var serviceProviderScope = ServiceProvider.CreateScope();

                var result = await InvokeCommandWithResultHandler(command, serviceProviderScope!.ServiceProvider!, cancellationToken);

                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "finished", span.Duration.Milliseconds);

                return result;
            }
            catch (OperationCanceledException)
            {
                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "cancelled", span.Duration.Milliseconds);

                throw;
            }
            catch
            {
                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching command {@OperationAction} after {@OperationDuration}ms.", "failed", span.Duration.Milliseconds);

                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MarkExceptionAsHandled([NotNull] Exception commandHandlerException)
        {
            if (commandHandlerException.Data.Contains("WasHandled"))
                commandHandlerException.Data["WasHandled"] = true;
            else
                commandHandlerException.Data.Add("WasHandled", true);
        }

        [NotNull]
        private readonly IOperationContext _operationContext;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task InvokeCommandHandler(
            [NotNull] ICommand typedCommand,
            [NotNull] IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            foreach (var preCommandHandler in OperationHandlerFactory.GetPreCommandHandlers(serviceProvider, typedCommand))
            {
                using var preCommandHandlerSpan = Tracer.OpenSpan(preCommandHandler.GetType(), SpanType.PreCommandHandler);

                try
                {
                    await preCommandHandler.PreHandleAsync(typedCommand, cancellationToken);
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    if (Options.Logging.EnableNonErrorLogging)
                        Logger.LogDebug(operationCanceledException, "Pre command handler invocation has been {@OperationAction}.", "cancelled");

                    MarkExceptionAsHandled(operationCanceledException);

                    throw;
                }
                catch (Exception preCommandHandlerException)
                {
                    Logger.LogError(preCommandHandlerException, "Pre command handler invocation {@OperationAction}.", "failed");

                    MarkExceptionAsHandled(preCommandHandlerException);

                    throw;
                }
            }

            foreach (var commandHandler in OperationHandlerFactory.GetCommandHandlers(serviceProvider, typedCommand))
            {
                using var commandHandlerSpan = Tracer.OpenSpan(commandHandler.GetType(), SpanType.CommandHandler);

                IOperationContextScope operationContextScope = default;

                if (!Options.CommandDoNotCreateOwnScope)
                    operationContextScope = _operationContext.OpenScope();

                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Command handler invocation {@OperationAction}.", "started");

                var internalEvents = ArrayPool<InternalEvent>.Shared!.Rent(Options.InternalEventsPoolSize)
                                                             .AsNotNull();

                try
                {
                    if (!Options.CommandDoNotCreateOwnScope)
                        await operationContextScope!.DispatchOperationsAsync(() => commandHandler.HandleAsync(typedCommand, cancellationToken),
                                                                             internalEvents,
                                                                             cancellationToken);
                    else
                        await commandHandler.HandleAsync(typedCommand, cancellationToken);

                    if (Options.Logging.EnableNonErrorLogging)
                        Logger.LogDebug("Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                        "finished",
                                        commandHandlerSpan.Duration.Milliseconds);

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandHandlers(serviceProvider, typedCommand))
                    {
                        using var postCommandHandlerSpan = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     internalEvents,
                                                                     null,
                                                                     cancellationToken);
                        }
                        catch (OperationCanceledException operationCanceledException)
                        {
                            if (Options.Logging.EnableNonErrorLogging)
                                Logger.LogDebug(operationCanceledException,
                                                "Post command handler invocation has been {@OperationAction} after {@OperationDuration}ms.",
                                                "cancelled",
                                                postCommandHandlerSpan.Duration.Milliseconds);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException,
                                            "Post command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                            "failed",
                                            postCommandHandlerSpan.Duration.Milliseconds);
                        }
                    }
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    if (Options.Logging.EnableNonErrorLogging)
                        Logger.LogDebug(operationCanceledException,
                                        "Command handler invocation has been {@OperationAction} after {@OperationDuration}ms.",
                                        "cancelled",
                                        commandHandlerSpan.Duration.Milliseconds);

                    MarkExceptionAsHandled(operationCanceledException);

                    throw;
                }
                catch (Exception commandHandlerException)
                {
                    Logger.LogError(commandHandlerException,
                                    "Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                    "failed",
                                    commandHandlerSpan.Duration.Milliseconds);

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandHandlers(serviceProvider, typedCommand))
                    {
                        using var postCommandHandlerSpan = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     internalEvents,
                                                                     commandHandlerException,
                                                                     cancellationToken);
                        }
                        catch (OperationCanceledException operationCanceledException)
                        {
                            if (Options.Logging.EnableNonErrorLogging)
                                Logger.LogDebug(operationCanceledException,
                                                "Post command handler invocation has been {@OperationAction} after {@OperationDuration}ms.",
                                                "cancelled",
                                                postCommandHandlerSpan.Duration.Milliseconds);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException,
                                            "Post command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                            "failed",
                                            postCommandHandlerSpan.Duration.Milliseconds);
                        }
                    }

                    MarkExceptionAsHandled(commandHandlerException);

                    throw;
                }
                finally
                {
                    ArrayPool<InternalEvent>.Shared.Return(internalEvents);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "FunctionComplexityOverflow")]
        private async Task<object> InvokeCommandWithResultHandler(
            [NotNull] ICommandWithResult typedCommand,
            [NotNull] IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            object result = null;

            foreach (var preCommandHandler in OperationHandlerFactory.GetPreCommandWithResultHandlers(serviceProvider, typedCommand))
            {
                using var preCommandHandlerSpan = Tracer.OpenSpan(preCommandHandler.GetType(), SpanType.PreCommandHandler);

                try
                {
                    await preCommandHandler.PreHandleAsync(typedCommand, cancellationToken);
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    if (Options.Logging.EnableNonErrorLogging)
                        Logger.LogDebug(operationCanceledException, "Pre command handler invocation has been {@OperationAction}.", "cancelled");

                    MarkExceptionAsHandled(operationCanceledException);

                    throw;
                }
                catch (Exception preCommandHandlerException)
                {
                    Logger.LogError(preCommandHandlerException, "Pre command handler invocation {@OperationAction}.", "failed");

                    MarkExceptionAsHandled(preCommandHandlerException);

                    throw;
                }
            }

            foreach (var commandHandler in OperationHandlerFactory.GetCommandHandlersWithResult(serviceProvider, typedCommand))
            {
                using var commandHandlerSpan = Tracer.OpenSpan(commandHandler.GetType(), SpanType.CommandHandler);
                object    tempResult         = null;

                IOperationContextScope operationContextScope = default;

                if (!Options.CommandDoNotCreateOwnScope)
                    operationContextScope = _operationContext.OpenScope();

                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Command handler invocation {@OperationAction}.", "started");

                var internalEvents = ArrayPool<InternalEvent>.Shared!.Rent(Options.InternalEventsPoolSize)
                                                             .AsNotNull();

                try
                {
                    if (!Options.CommandDoNotCreateOwnScope)
                        await operationContextScope!.DispatchOperationsAsync(
                            async () => tempResult = await commandHandler.HandleAsync(typedCommand, cancellationToken),
                            internalEvents,
                            cancellationToken);
                    else
                        tempResult = await commandHandler.HandleAsync(typedCommand, cancellationToken);

                    if (Options.Logging.EnableNonErrorLogging)
                        Logger.LogDebug("Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                        "finished",
                                        commandHandlerSpan.Duration.Milliseconds);

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandWithResultHandlers(serviceProvider, typedCommand))
                    {
                        using var postCommandHandlerSpan = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     tempResult,
                                                                     internalEvents,
                                                                     null,
                                                                     cancellationToken);
                        }
                        catch (OperationCanceledException operationCanceledException)
                        {
                            if (Options.Logging.EnableNonErrorLogging)
                                Logger.LogDebug(operationCanceledException,
                                                "Post command handler invocation has been {@OperationAction} after {@OperationDuration}ms.",
                                                "cancelled",
                                                postCommandHandlerSpan.Duration.Milliseconds);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException,
                                            "Post command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                            "failed",
                                            postCommandHandlerSpan.Duration.Milliseconds);
                        }
                    }

                    result = tempResult;
                }
                catch (OperationCanceledException commandHandlerException)
                {
                    if (Options.Logging.EnableNonErrorLogging)
                        Logger.LogDebug(commandHandlerException,
                                        "Command handler invocation has been {@OperationAction} after {@OperationDuration}ms.",
                                        "cancelled",
                                        commandHandlerSpan.Duration.Milliseconds);

                    MarkExceptionAsHandled(commandHandlerException);

                    throw;
                }
                catch (Exception commandHandlerException)
                {
                    Logger.LogError(commandHandlerException,
                                    "Command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                    "failed",
                                    commandHandlerSpan.Duration.Milliseconds);

                    foreach (var postCommandHandler in OperationHandlerFactory.GetPostCommandWithResultHandlers(serviceProvider, typedCommand))
                    {
                        using var postCommandHandlerSpan = Tracer.OpenSpan(postCommandHandler.GetType(), SpanType.PostCommandHandler);

                        try
                        {
                            await postCommandHandler.PostHandleAsync(typedCommand,
                                                                     tempResult,
                                                                     internalEvents,
                                                                     commandHandlerException,
                                                                     cancellationToken);
                        }
                        catch (OperationCanceledException operationCanceledException)
                        {
                            if (Options.Logging.EnableNonErrorLogging)
                                Logger.LogDebug(operationCanceledException,
                                                "Post command handler invocation has been {@OperationAction} after {@OperationDuration}ms.",
                                                "cancelled",
                                                postCommandHandlerSpan.Duration.Milliseconds);
                        }
                        catch (Exception postCommandHandlerException)
                        {
                            Logger.LogError(postCommandHandlerException,
                                            "Post command handler invocation {@OperationAction} after {@OperationDuration}ms.",
                                            "failed",
                                            postCommandHandlerSpan.Duration.Milliseconds);
                        }
                    }

                    MarkExceptionAsHandled(commandHandlerException);

                    throw;
                }
                finally
                {
                    ArrayPool<InternalEvent>.Shared.Return(internalEvents);
                }
            }

            return result;
        }
    }
}

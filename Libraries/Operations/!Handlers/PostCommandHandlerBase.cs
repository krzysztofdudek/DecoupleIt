using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all post command handlers.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class PostCommandHandlerBase<TCommand> : IPostCommandHandler
        where TCommand : Command
    {
        /// <summary>
        ///     Post handles a command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="internalEvents">Internal events.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            PostHandleAsync(
                [NotNull] TCommand command,
                [NotNull] [ItemNotNull] IReadOnlyCollection<InternalEvent> internalEvents,
                [CanBeNull] Exception exception,
                CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IPostCommandHandler.PostHandleAsync(
                ICommand command,
                IReadOnlyCollection<InternalEvent> internalEvents,
                Exception exception,
                CancellationToken cancellationToken)
        {
            if (!(command is TCommand typedCommand))
                throw new ArgumentException("Command is of invalid type.", nameof(command));

            return PostHandleAsync(typedCommand,
                                   internalEvents,
                                   exception,
                                   cancellationToken);
        }
    }

    /// <summary>
    ///     Base class for all post command handlers.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class PostCommandHandlerBase<TCommand, TResult> : IPostCommandWithResultHandler
        where TCommand : Command<TResult>
    {
        /// <summary>
        ///     Post handles a command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="result">Result.</param>
        /// <param name="internalEvents">Internal events.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync(
                [NotNull] TCommand command,
                [CanBeNull] TResult result,
                [NotNull] [ItemNotNull] IReadOnlyCollection<InternalEvent> internalEvents,
                [CanBeNull] Exception exception,
                CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IPostCommandWithResultHandler.PostHandleAsync(
                ICommandWithResult command,
                object result,
                IReadOnlyCollection<InternalEvent> internalEvents,
                Exception exception,
                CancellationToken cancellationToken)
        {
            if (!(command is TCommand typedCommand))
                throw new ArgumentException("Command is of invalid type.", nameof(command));

            return HandleAsync(typedCommand,
                               (TResult) result,
                               internalEvents,
                               exception,
                               cancellationToken);
        }
    }
}

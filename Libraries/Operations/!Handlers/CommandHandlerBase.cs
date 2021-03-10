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
    ///     Base class for all command handlers.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class CommandHandlerBase<TCommand> : ICommandHandler
        where TCommand : Command
    {
        /// <summary>
        ///     Handles a command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync([NotNull] TCommand command, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            ICommandHandler.HandleAsync(ICommand command, CancellationToken cancellationToken)
        {
            if (!(command is TCommand typedCommand))
                throw new ArgumentException("Command is of invalid type.", nameof(command));

            return HandleAsync(typedCommand, cancellationToken);
        }
    }

    /// <summary>
    ///     Base class for all command handlers.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class CommandHandlerBase<TCommand, TResult> : ICommandWithResultHandler
        where TCommand : Command<TResult>
    {
        /// <summary>
        ///     Handles a command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        [ItemCanBeNull]
        protected abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            HandleAsync([NotNull] TCommand command, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            ICommandWithResultHandler.HandleAsync(ICommandWithResult command, CancellationToken cancellationToken)
        {
            if (!(command is TCommand typedCommand))
                throw new ArgumentException("Command is of invalid type.", nameof(command));

            return await HandleAsync(typedCommand, cancellationToken);
        }
    }
}

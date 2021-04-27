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
    ///     Base class for all pre command handlers.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    [Singleton]
    [RegisterManyTimes]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class PreCommandHandlerBase<TCommand> : IPreCommandHandler
        where TCommand : Command
    {
        /// <summary>
        ///     Pre handles a command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            PreHandleAsync([NotNull] TCommand command, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IPreCommandHandler.PreHandleAsync(ICommand command, CancellationToken cancellationToken)
        {
            if (command is not TCommand typedCommand)
                throw new ArgumentException("Command is of invalid type.", nameof(command));

            return PreHandleAsync(typedCommand, cancellationToken);
        }
    }

    /// <summary>
    ///     Base class for all pre command handlers.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    [Singleton]
    [RegisterManyTimes]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class PreCommandHandlerBase<TCommand, TResult> : IPreCommandWithResultHandler
        where TCommand : Command<TResult>
    {
        /// <summary>
        ///     Pre handles a command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            PreHandleAsync([NotNull] TCommand command, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IPreCommandWithResultHandler.PreHandleAsync(ICommandWithResult command, CancellationToken cancellationToken)
        {
            if (command is not TCommand typedCommand)
                throw new ArgumentException("Command is of invalid type.", nameof(command));

            return PreHandleAsync(typedCommand, cancellationToken);
        }
    }
}

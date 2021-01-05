using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    internal static class OperationDispatcher
    {
        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchCommandAsync([NotNull] Command command, CancellationToken cancellationToken = default)
        {
            return ExecuteWithContext(scope => scope!.DispatchCommandAsync(command, cancellationToken));
        }

        [NotNull]
        [ItemCanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            DispatchCommandAsync<TResult>([NotNull] Command<TResult> command, CancellationToken cancellationToken = default)
        {
            return (TResult) await ExecuteWithResultWithContext(scope => scope!.DispatchCommandAsync(command, cancellationToken));
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchInternalEventAsync([NotNull] InternalEvent internalEvent, CancellationToken cancellationToken = default)
        {
            return ExecuteWithContext(scope => scope!.DispatchInternalEventAsync(internalEvent, cancellationToken));
        }

        [NotNull]
        [ItemCanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            DispatchQueryAsync<TResult>([NotNull] Query<TResult> query, CancellationToken cancellationToken = default)
        {
            return (TResult) await ExecuteWithResultWithContext(scope => scope!.DispatchQueryAsync(query, cancellationToken));
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ExecuteWithContext([NotNull] ExecuteWithContextDelegate action)
        {
            return OperationContext.CurrentScope == null
                ?
#if NETCOREAPP2_2 || NETSTANDARD2_0
                Task.CompletedTask
#else
                new ValueTask()
#endif
                : action(OperationContext.CurrentScope)
                    .AsNotNull();
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            ExecuteWithResultWithContext([NotNull] ExecuteWithResultWithContextDelegate action)
        {
            return OperationContext.CurrentScope == null
                ?
#if NETCOREAPP2_2 || NETSTANDARD2_0
                Task.FromResult<object>(null)
#else
                new ValueTask<object>()
#endif
                : action(OperationContext.CurrentScope)
                    .AsNotNull();
        }

        [NotNull]
        private delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ExecuteWithContextDelegate([NotNull] OperationContextScope scope);

        [NotNull]
        private delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            ExecuteWithResultWithContextDelegate([NotNull] OperationContextScope scope);
    }
}

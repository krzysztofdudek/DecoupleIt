using System;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [Transient]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    internal sealed class OperationContext : IOperationContext
    {
        [CanBeNull]
        internal static OperationContextScope CurrentScope
        {
            get => CurrentOperationContextScopeStorage.Value;
            private set => CurrentOperationContextScopeStorage.Value = value;
        }

        public OperationContext([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IOperationContextScope OpenScope()
        {
            var scope = new OperationContextScope(_serviceProvider, CurrentScope);

            scope.Closed += ScopeOnClosed;

            CurrentScope = scope;

            return scope;
        }

        [NotNull]
        private static readonly AsyncLocal<OperationContextScope> CurrentOperationContextScopeStorage = new();

        private static void ScopeOnClosed([NotNull] OperationContextScope scope)
        {
            scope.Closed -= ScopeOnClosed;

            CurrentScope = scope.Parent;
        }

        [NotNull]
        private readonly IServiceProvider _serviceProvider;
    }
}

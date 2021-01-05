using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    internal delegate void OnOperationContextScopeClosedEventHandlerDelegate([NotNull] OperationContextScope operationContextScope);
}

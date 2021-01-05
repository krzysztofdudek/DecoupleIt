using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GS.DecoupleIt.Operations.AspNetCore
{
    [Transient]
    internal sealed class DispatchOperationsMiddleware : IMiddleware
    {
        public DispatchOperationsMiddleware([NotNull] IOperationContext operationContext)
        {
            _operationContext = operationContext;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            using var scope = _operationContext.OpenScope();

            await scope.DispatchOperationsAsync(
#if NETCOREAPP2_2 || NETSTANDARD2_0
                () => next(context)!,
#else
                async () => await next(context)!,
#endif
                context.RequestAborted);
        }

        [NotNull]
        private readonly IOperationContext _operationContext;
    }
}

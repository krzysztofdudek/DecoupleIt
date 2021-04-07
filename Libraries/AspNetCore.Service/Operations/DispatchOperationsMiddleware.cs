using System;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GS.DecoupleIt.AspNetCore.Service.Operations
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

            await scope.DispatchOperationsAsync(() => next(context)!.AsValueTask(), context.RequestAborted);
        }

        [NotNull]
        private readonly IOperationContext _operationContext;
    }
}

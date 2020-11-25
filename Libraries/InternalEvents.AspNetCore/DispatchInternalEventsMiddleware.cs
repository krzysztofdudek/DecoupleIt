using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GS.DecoupleIt.InternalEvents.AspNetCore
{
    [Transient]
    internal sealed class DispatchInternalEventsMiddleware : IMiddleware
    {
        public DispatchInternalEventsMiddleware([NotNull] IInternalEventDispatcher internalEventDispatcher)
        {
            _internalEventDispatcher = internalEventDispatcher;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            InternalEventsScope.Initialize();

            try
            {
                using var scope = InternalEventsScope.OpenScope();

                await scope.DispatchEventsAsync(_internalEventDispatcher,
#if NETCOREAPP2_2 || NETSTANDARD2_0
                                                () => next(context)!,
#else
                                                async () => await next(context),
#endif
                                                context.RequestAborted);
            }
            finally
            {
                InternalEventsScope.Clear();
            }
        }

        [NotNull]
        private readonly IInternalEventDispatcher _internalEventDispatcher;
    }
}

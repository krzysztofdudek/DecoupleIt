using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
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

        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            using (var scope = Scope.InternalEventsScope.OpenScope())
            {
                await scope.DispatchEventsAsync(_internalEventDispatcher,
                                                () => next(context)
                                                    .AsNotNull(),
                                                context.RequestAborted);
            }
        }

        [NotNull]
        private readonly IInternalEventDispatcher _internalEventDispatcher;
    }
}

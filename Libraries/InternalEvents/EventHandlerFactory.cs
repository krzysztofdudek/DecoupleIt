using System;
using System.Collections.Generic;
using System.Linq;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.InternalEvents
{
    [Transient]
    internal sealed class EventHandlerFactory : IEventHandlerFactory
    {
        public EventHandlerFactory([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReadOnlyCollection<IOnEmissionEventHandler> ResolveOnEmissionEventHandlers(Type eventType)
        {
            var serviceType = typeof(IEnumerable<>).MakeGenericType(typeof(IOnEmissionEventHandler<>).MakeGenericType(eventType));

            var services = (IEnumerable<IOnEmissionEventHandler>) _serviceProvider.GetRequiredService(serviceType)
                                                                                  .AsNotNull();

            return services.ToList();
        }

        public IReadOnlyCollection<IOnFailureEventHandler> ResolveOnFailureEventHandlers(Type eventType)
        {
            var serviceType = typeof(IEnumerable<>).MakeGenericType(typeof(IOnFailureEventHandler<>).MakeGenericType(eventType));

            var services = (IEnumerable<IOnFailureEventHandler>) _serviceProvider.GetRequiredService(serviceType)
                                                                                 .AsNotNull();

            return services.ToList();
        }

        public IReadOnlyCollection<IOnSuccessEventHandler> ResolveOnSuccessEventHandlers(Type eventType)
        {
            var serviceType = typeof(IEnumerable<>).MakeGenericType(typeof(IOnSuccessEventHandler<>).MakeGenericType(eventType));

            var services = (IEnumerable<IOnSuccessEventHandler>) _serviceProvider.GetRequiredService(serviceType)
                                                                                 .AsNotNull();

            return services.ToList();
        }

        [NotNull]
        private readonly IServiceProvider _serviceProvider;
    }
}

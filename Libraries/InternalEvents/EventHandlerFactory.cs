using System;
using System.Collections.Generic;
using System.Linq;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.InternalEvents
{
    [Singleton]
    internal sealed class EventHandlerFactory : IEventHandlerFactory
    {
        public EventHandlerFactory([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReadOnlyCollection<IOnEmissionEventHandler> ResolveOnEmissionEventHandlers(Type eventType)
        {
            return GetServiceType<IOnEmissionEventHandler>(eventType, typeof(IOnEmissionEventHandler<>));
        }

        public IReadOnlyCollection<IOnFailureEventHandler> ResolveOnFailureEventHandlers(Type eventType)
        {
            return GetServiceType<IOnFailureEventHandler>(eventType, typeof(IOnFailureEventHandler<>));
        }

        public IReadOnlyCollection<IOnSuccessEventHandler> ResolveOnSuccessEventHandlers(Type eventType)
        {
            return GetServiceType<IOnSuccessEventHandler>(eventType, typeof(IOnSuccessEventHandler<>));
        }

        [NotNull]
        private static readonly Dictionary<(Type, Type), Type> Cache = new Dictionary<(Type, Type), Type>();

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        [ItemNotNull]
        private IReadOnlyCollection<TEventHandler> GetServiceType<TEventHandler>([NotNull] Type eventType, [NotNull] Type eventHandlerType)
        {
            var cacheKey = (eventType, eventHandlerType);

            if (!Cache.TryGetValue(cacheKey, out var serviceType))
            {
                serviceType = typeof(IEnumerable<>).MakeGenericType(typeof(IOnEmissionEventHandler<>).MakeGenericType(eventType));

                Cache.Add(cacheKey, serviceType);
            }

            var services = (IEnumerable<IOnEmissionEventHandler>) _serviceProvider.GetRequiredService(serviceType.AsNotNull());

            return (IReadOnlyCollection<TEventHandler>) services.ToList();
        }
    }
}

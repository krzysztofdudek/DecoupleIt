using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public IEnumerable<IOnEmissionEventHandler> ResolveOnEmissionEventHandlers(Type eventType)
        {
            return GetServiceType<IOnEmissionEventHandler>(eventType, typeof(IOnEmissionEventHandler<>));
        }

        public IEnumerable<IOnFailureEventHandler> ResolveOnFailureEventHandlers(Type eventType)
        {
            return GetServiceType<IOnFailureEventHandler>(eventType, typeof(IOnFailureEventHandler<>));
        }

        public IEnumerable<IOnSuccessEventHandler> ResolveOnSuccessEventHandlers(Type eventType)
        {
            return GetServiceType<IOnSuccessEventHandler>(eventType, typeof(IOnSuccessEventHandler<>));
        }

        [NotNull]
        private static readonly ConcurrentDictionary<(Type, Type), Type> Cache = new();

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        [ItemNotNull]
        private IEnumerable<TEventHandler> GetServiceType<TEventHandler>([NotNull] Type eventType, [NotNull] Type eventHandlerType)
        {
            var cacheKey = (eventType, eventHandlerType);

            if (!Cache.TryGetValue(cacheKey, out var serviceType))
            {
                serviceType = typeof(IEnumerable<>).MakeGenericType(eventHandlerType.MakeGenericType(eventType));

                Cache.TryAdd(cacheKey, serviceType);
            }

            var services = _serviceProvider.GetRequiredService(serviceType.AsNotNull())
                                           .AsNotNull();

            return (IEnumerable<TEventHandler>) services;
        }
    }
}

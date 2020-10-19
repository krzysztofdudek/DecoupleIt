using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Factory of event handlers.
    /// </summary>
    public interface IEventHandlerFactory
    {
        /// <summary>
        ///     Creates instances of on emission event handlers dedicated for given event type.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <returns>A collection of event handlers.</returns>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<IOnEmissionEventHandler> ResolveOnEmissionEventHandlers([NotNull] Type eventType);

        /// <summary>
        ///     Creates instances of on failure event handlers dedicated for given event type.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <returns>A collection of event handlers.</returns>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<IOnFailureEventHandler> ResolveOnFailureEventHandlers([NotNull] Type eventType);

        /// <summary>
        ///     Creates instances of on success event handlers dedicated for given event type.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <returns>A collection of event handlers.</returns>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<IOnSuccessEventHandler> ResolveOnSuccessEventHandlers([NotNull] Type eventType);
    }
}

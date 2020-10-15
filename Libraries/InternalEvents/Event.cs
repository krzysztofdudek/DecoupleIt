using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.InternalEvents.Scope;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Base class of an event.
    /// </summary>
    [PublicAPI]
    public abstract class Event
    {
        /// <summary>
        ///     Identifier.
        /// </summary>
        public Guid EventIdentifier { get; }

        /// <summary>
        ///     Timestamp from moment of creation. It's in UTC time zone.
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <inheritdoc cref="IInternalEventsScope.EmitEvent" />
        public void Emit()
        {
            InternalEventsScope.CurrentScope.EmitEvent(this);
        }

        /// <inheritdoc cref="IInternalEventsScope.EmitEventAsync" />
        [NotNull]
        public Task EmitAsync(CancellationToken cancellationToken = default)
        {
            return InternalEventsScope.CurrentScope.EmitEventAsync(this, cancellationToken);
        }

        /// <summary>
        ///     Creates an instance of <see cref="Event" />.
        /// </summary>
        protected Event()
        {
            EventIdentifier = Guid.NewGuid();
            TimeStamp       = DateTime.UtcNow;
        }
    }
}

using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Type of a span.
    /// </summary>
    [PublicAPI]
    public enum SpanType
    {
#pragma warning disable 1591
        OutgoingRequest,
        OutgoingEvent,
        ExternalRequest,
        ExternalRequestHandler,
        InternalEvent,
        InternalEventHandler,
        ExternalEvent,
        ExternalEventHandler,
        Command,
        CommandHandler,
        Query,
        QueryHandler,
        Job,
        InternalProcess
#pragma warning restore 1591
    }
}

using System;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Delegate handling closing of span.
    /// </summary>
    /// <param name="span">Closed span.</param>
    /// <param name="duration">Duration of span.</param>
    public delegate void SpanClosedDelegate(SpanDescriptor span, TimeSpan duration);
}

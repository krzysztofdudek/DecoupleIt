namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Delegate handling opening of spans.
    /// </summary>
    /// <param name="span">Opened span.</param>
    public delegate void SpanOpenedDelegate(Span span);
}

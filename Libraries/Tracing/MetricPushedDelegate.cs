namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Delegate handling metric push.
    /// </summary>
    /// <param name="span">Span.</param>
    /// <param name="metric">Metric.</param>
    public delegate void MetricPushedDelegate(Span span, Metric metric);
}

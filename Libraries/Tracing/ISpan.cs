using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Represents span.
    /// </summary>
    [PublicAPI]
    public interface ISpan : IDisposable
    {
        /// <summary>
        ///     Description of span.
        /// </summary>
        Span Descriptor { get; }

        /// <summary>
        ///     Gets the duration of scope.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        ///     Metrics.
        /// </summary>
        [NotNull]
        IReadOnlyCollection<Metric> Metrics { get; }

        /// <summary>
        ///     Event is invoked when metric is pushed.
        /// </summary>
        [CanBeNull]
        event MetricPushedDelegate MetricPushed;

        /// <summary>
        ///     Attached disposable resource to this instance. It will be disposed on scope disposal.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="key">Key of resource.</param>
        /// <exception cref="ObjectDisposedException">Tracer has been disposed.</exception>
        [NotNull]
        ISpan AttachResource([NotNull] object resource, [CanBeNull] object key = default);

        /// <summary>
        ///     Pushes a metric.
        /// </summary>
        /// <param name="metric">Metric.</param>
        void PushMetric(Metric metric);
    }
}

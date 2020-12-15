using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Span represents single tracing span.
    /// </summary>
    internal readonly struct TracerSpan : ITracerSpan, IEquatable<TracerSpan>
    {
        /// <summary>
        ///     Description of span.
        /// </summary>
        public SpanDescriptor Descriptor { get; }

        /// <summary>
        ///     Gets the duration of scope.
        /// </summary>
        public TimeSpan Duration => _stopwatch.Elapsed;

        /// <summary>
        ///     Parent span.
        /// </summary>
        public ITracerSpan Parent { get; }

        [NotNull]
        private readonly Action<TracerSpan> _closed;

        private readonly Guid _id;

        internal TracerSpan(
            SpanDescriptor descriptor,
            [CanBeNull] ITracerSpan parent,
            [NotNull] Action<TracerSpan> onCloseCallback,
            [CanBeNull] IDisposable loggerScope)
        {
            Descriptor   = descriptor;
            Parent       = parent;
            _stopwatch   = Stopwatch.StartNew();
            _closed      = onCloseCallback;
            _loggerScope = loggerScope;
            _id          = Guid.NewGuid();
        }

        private void Close()
        {
            if (!_stopwatch.IsRunning)
                return;

            _stopwatch.Stop();

            _loggerScope?.Dispose();

            _closed.Invoke(this);
        }

        [CanBeNull]
        private readonly IDisposable _loggerScope;

        [NotNull]
        private readonly Stopwatch _stopwatch;

        /// <inheritdoc />
        public void Dispose()
        {
            Close();
        }

        public bool Equals(TracerSpan other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            return obj is TracerSpan other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}

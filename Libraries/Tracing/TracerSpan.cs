using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Span represents single tracing span.
    ///     Class is not inheritable.
    /// </summary>
    public readonly struct TracerSpan : IDisposable, IEquatable<TracerSpan>
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
        [CanBeNull]
        public TracerSpan? ParentSpan { get; }

        [NotNull]
        private readonly Action<TracerSpan> _closed;

        private readonly Guid _id;

        internal TracerSpan(SpanDescriptor descriptor, [CanBeNull] TracerSpan? parentTracerSpan, [NotNull] Action<TracerSpan> onCloseCallback, [NotNull] IDisposable loggerScope)
        {
            Descriptor   = descriptor;
            ParentSpan   = parentTracerSpan;
            _stopwatch   = Stopwatch.StartNew();
            _closed      = onCloseCallback;
            _loggerScope = loggerScope;
            _id          = Guid.NewGuid();
        }

        internal void Close()
        {
            _loggerScope.Dispose();

            _stopwatch.Stop();

            _closed.Invoke(this);
        }

        [NotNull]
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

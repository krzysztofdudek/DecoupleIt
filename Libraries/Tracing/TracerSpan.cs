using System;
using System.Collections.Generic;
using System.Diagnostics;
using GS.DecoupleIt.Shared;
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

        [NotNull]
        private readonly Action<TracerSpan> _closed;

        private readonly Guid _id;

        internal TracerSpan(SpanDescriptor span, [NotNull] Action<TracerSpan> onCloseCallback)
        {
            Descriptor         = span;
            _stopwatch         = Stopwatch.StartNew();
            _attachedResources = new List<IDisposable>();
            _closed            = onCloseCallback;
            _id                = Guid.NewGuid();
        }

        public void AttachResource(IDisposable resource)
        {
            ContractGuard.IfArgumentIsNull(nameof(resource), resource);

            _attachedResources.Add(resource);
        }

        internal void Close()
        {
            foreach (var attachedResource in _attachedResources.ToNotNullList())
            {
                attachedResource.Dispose();

                _attachedResources.Remove(attachedResource);
            }

            _stopwatch.Stop();

            _closed.Invoke(this);
        }

        [NotNull]
        [ItemNotNull]
        private readonly List<IDisposable> _attachedResources;

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

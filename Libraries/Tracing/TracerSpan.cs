using System;
using System.Collections.Generic;
using System.Diagnostics;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Span represents single tracing span.
    ///     Class is not inheritable.
    /// </summary>
    internal sealed class TracerSpan : ITracerSpan
    {
        public SpanDescriptor Descriptor { get; }

        /// <inheritdoc />
        public TimeSpan Duration => _stopwatch.Elapsed;

        internal event Action<TracerSpan> Closed;

        internal TracerSpan(SpanDescriptor span)
        {
            Descriptor = span;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <inheritdoc />
        public void AttachResource(IDisposable resource)
        {
            ContractGuard.IfArgumentIsNull(nameof(resource), resource);

            CheckIfDisposed();

            _attachedResources.Add(resource);
        }

        internal void Close()
        {
            CheckIfDisposed();

            foreach (var attachedResource in _attachedResources.ToNotNullList())
            {
                attachedResource.Dispose();

                _attachedResources.Remove(attachedResource);
            }

            _stopwatch.Stop();

            _isDisposed = true;

            Closed?.Invoke(this);
        }

        [NotNull]
        [ItemNotNull]
        private readonly List<IDisposable> _attachedResources = new List<IDisposable>();

        private bool _isDisposed;

        [NotNull]
        private readonly Stopwatch _stopwatch;

        private void CheckIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Tracer has been disposed.");
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            Close();
        }
    }
}

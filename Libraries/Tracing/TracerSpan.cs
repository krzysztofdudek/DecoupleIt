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

        internal TracerSpan(SpanDescriptor span, [NotNull] Tracer tracer)
        {
            Descriptor = span;
            _tracer    = tracer;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <inheritdoc />
        public void AttachResource(IDisposable resource)
        {
            ContractGuard.IfArgumentIsNull(nameof(resource), resource);

            CheckIfDisposed();

            _attachedResources.Add(resource);
        }

        public void Close()
        {
            CheckIfDisposed();

            if (_tracer.CurrentSpan != this)
                throw new InvalidOperationException(
                    $"Can not dispose span if it's not current span. Current is called \"{_tracer.CurrentSpan.Descriptor.Name}\".");

            _tracer.Trace.Pop();

            foreach (var attachedResource in _attachedResources.ToNotNullList())
            {
                attachedResource.Dispose();

                _attachedResources.Remove(attachedResource);
            }

            _stopwatch.Stop();

            _isDisposed = true;

            _tracer.InvokeSpanClosed(Descriptor, Duration);
        }

        [NotNull]
        [ItemNotNull]
        private readonly List<IDisposable> _attachedResources = new List<IDisposable>();

        private bool _isDisposed;

        [NotNull]
        private readonly Stopwatch _stopwatch;

        [NotNull]
        private readonly Tracer _tracer;

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

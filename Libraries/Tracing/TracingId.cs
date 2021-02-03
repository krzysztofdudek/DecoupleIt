using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Identifier used for tracking. Makes tracer universal that can use <see cref="Guid" /> as also <see cref="string" />.
    /// </summary>
    public readonly struct TracingId : IEquatable<TracingId>
    {
        [NotNull]
        private readonly string _value;

        /// <summary>
        ///     Creates an instance of <see cref="TracingId" />.
        /// </summary>
        /// <param name="value">Value.</param>
        [PublicAPI]
        public TracingId([NotNull] string value)
        {
            _value = value;
        }

        /// <inheritdoc />
        public bool Equals(TracingId other)
        {
            return _value == other._value;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TracingId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static implicit operator TracingId(Guid value)
        {
            return new(value.ToString("D"));
        }

        public static implicit operator TracingId([NotNull] string value)
        {
            return new(value);
        }

        public static implicit operator Guid(TracingId tracingId)
        {
            return Guid.ParseExact(tracingId._value, "D");
        }

        [NotNull]
        public static implicit operator string(TracingId tracingId)
        {
            return tracingId._value;
        }

        /// <inheritdoc />
        [NotNull]
        public override string ToString()
        {
            return _value;
        }
    }
}

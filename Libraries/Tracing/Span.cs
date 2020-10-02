using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Scope describes single level of tracing stack. It allows to trace back to origin from every log from the system.
    /// </summary>
    [PublicAPI]
    public readonly struct Span
    {
        /// <summary>
        ///     Opens an instance of <see cref="Span" />.
        /// </summary>
        /// <param name="traceId">Trace identifier.</param>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="parentId">Parent span identifier.</param>
        /// <param name="type">Type.</param>
        internal Span(
            TracingId traceId,
            TracingId id,
            [NotNull] string name,
            TracingId? parentId,
            SpanType type)
        {
            TraceId  = traceId;
            Id       = id;
            Name     = name;
            ParentId = parentId;
            Type     = type;
        }

        /// <summary>
        ///     Unique identifier of caller operation.
        /// </summary>
        public TracingId TraceId { get; }

        /// <summary>
        ///     Unique identifier of scope.
        /// </summary>
        public TracingId Id { get; }

        /// <summary>
        ///     Name of scope.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        ///     Identifier of parent span.
        /// </summary>
        public TracingId? ParentId { get; }

        /// <summary>
        ///     Type of an scope indicates what is the "owner" of it.
        /// </summary>
        public SpanType Type { get; }
    }
}

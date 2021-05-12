using System;
using System.Collections.Generic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    [PublicAPI]
    [ConfigureAsNamespace]
    public sealed class Options
    {
        /// <summary>
        ///     Logger properties options.
        /// </summary>
        [NotNull]
        public LoggerPropertiesOptions LoggerProperties { get; set; } = new();

        /// <summary>
        ///     New tracing id generator.
        /// </summary>
        [NotNull]
        public Func<TracingId> NewTracingIdGenerator
        {
            get => _newTracingIdGenerator;
            set
            {
                ContractGuard.IfArgumentIsNull(nameof(value), value);

                _newTracingIdGenerator = value;
            }
        }

        /// <summary>
        ///     Options providing information about properties that are attached to logger.
        /// </summary>
        [Configure]
        public class LoggerPropertiesOptions
        {
            /// <summary>
            ///     Properties for ParentSpanId.
            /// </summary>
            [NotNull]
            [ItemNotNull]
            public List<string> ParentSpanId { get; set; } = new()
            {
                "ParentSpanId"
            };

            /// <summary>
            ///     Properties for SpanId.
            /// </summary>
            [NotNull]
            [ItemNotNull]
            public List<string> SpanId { get; set; } = new()
            {
                "SpanId"
            };

            /// <summary>
            ///     Properties for SpanName.
            /// </summary>
            [NotNull]
            [ItemNotNull]
            public List<string> SpanName { get; set; } = new()
            {
                "SpanName"
            };

            /// <summary>
            ///     Properties for SpanType.
            /// </summary>
            [NotNull]
            [ItemNotNull]
            public List<string> SpanType { get; set; } = new()
            {
                "SpanType"
            };

            /// <summary>
            ///     Properties for TraceId.
            /// </summary>
            [NotNull]
            [ItemNotNull]
            public List<string> TraceId { get; set; } = new()
            {
                "TraceId"
            };
        }

        [NotNull]
        private Func<TracingId> _newTracingIdGenerator = () => new TracingId(Guid.NewGuid()
                                                                                 .ToString());
    }
}

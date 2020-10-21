using System.Collections.Generic;
using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
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
        public List<string> ParentSpanId { get; set; } = new List<string>
        {
            "ParentSpanId"
        };

        /// <summary>
        ///     Properties for SpanId.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<string> SpanId { get; set; } = new List<string>
        {
            "SpanId"
        };

        /// <summary>
        ///     Properties for SpanName.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<string> SpanName { get; set; } = new List<string>
        {
            "SpanName"
        };

        /// <summary>
        ///     Properties for SpanType.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<string> SpanType { get; set; } = new List<string>
        {
            "SpanType"
        };

        /// <summary>
        ///     Properties for TraceId.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<string> TraceId { get; set; } = new List<string>
        {
            "TraceId"
        };
    }
}

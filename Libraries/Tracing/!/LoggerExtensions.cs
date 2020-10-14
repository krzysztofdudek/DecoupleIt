using System;
using System.Collections.Generic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Extends <see cref="ILogger" />.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
    public static class LoggerExtensions
    {
        /// <summary>
        ///     Begins a logical tracer span and sets span properties within logger scope.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <returns>Disposable scope.</returns>
        [NotNull]
        public static IDisposable BeginTracerSpan([NotNull] this ILogger logger)
        {
            var span = Tracer.CurrentSpan;

            return logger.BeginScope(new Dictionary<string, object>
                         {
                             {
                                 "TraceId", span.TraceId
                             },

                             {
                                 "SpanId", span.Id
                             },

                             {
                                 "ParentSpanId", span.ParentId
                             },

                             {
                                 "SpanName", span.Name
                             },

                             {
                                 "SpanType", span.Type
                             }
                         })
                         .AsNotNull();
        }
    }
}

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Middleware logging requests and responses.
    /// </summary>
    [Transient]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    public sealed class LoggingMiddleware : IMiddleware
    {
        public LoggingMiddleware([NotNull] ILogger<LoggingMiddleware> logger, [NotNull] IOptions<Options> serviceOptions, [NotNull] ITracer tracer)
        {
            _logger  = logger;
            _tracer  = tracer;
            _options = serviceOptions.Value;

            var blockSize                 = 1024;
            var largeBufferMultiple       = 1024 * 1024;
            var maximumBufferSize         = 16 * largeBufferMultiple;
            var maximumFreeLargePoolBytes = maximumBufferSize * 4;
            var maximumFreeSmallPoolBytes = 250 * blockSize;

            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager(blockSize, largeBufferMultiple, maximumBufferSize)
            {
                AggressiveBufferReturn    = true,
                GenerateCallStacks        = false,
                MaximumFreeLargePoolBytes = maximumFreeLargePoolBytes,
                MaximumFreeSmallPoolBytes = maximumFreeSmallPoolBytes
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            context.Request.EnableBuffering();

            var endpoint = context.GetEndpoint();

            if (endpoint is null)
            {
                await next(context);

                return;
            }

            var controllerActionDescriptor = endpoint.Metadata.OfType<ControllerActionDescriptor>()
                                                     .Single();

            var controllerName = controllerActionDescriptor.ControllerTypeInfo.FullName;
            var actionName     = controllerActionDescriptor.ActionName;

            using var scope = _tracer.OpenSpan($"{controllerName}.{actionName}", SpanType.ExternalRequestHandler);

            context.Request.Body.Position = 0;

            var requestBody = await ReadStreamAsync(context.Request.Body);

            if (_options.Logging.LogRequests)
                LogStart(context, requestBody);

            await using (var memoryStream = (RecyclableMemoryStream) _recyclableMemoryStreamManager.GetStream())
            {
                var originalResponseBodyStream = context.Response.Body;
                context.Response.Body = memoryStream;

                Exception exception = default;

                try
                {
                    await next(context)
                        .AsNotNull();
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                }

                var length = memoryStream.Position;

                memoryStream.Seek(0, SeekOrigin.Begin);

                var responseBody = memoryStream.GetMemory()[..(int) length];

                await originalResponseBodyStream.WriteAsync(responseBody, context.RequestAborted);

                context.Response.Body          = originalResponseBodyStream;
                context.Response.ContentLength = memoryStream.Position;

                if (_options.Logging.LogResponses)
                    LogFinish(context,
                              responseBody,
                              scope.Duration,
                              exception);
            }
        }

        [NotNull]
        [ItemNotNull]
        private static async Task<string> ReadStreamAsync([NotNull] Stream stream)
        {
            using var reader = new StreamReader(stream,
                                                Encoding.UTF8,
                                                false,
                                                -1,
                                                true);

            var requestBody = await reader.ReadToEndAsync();

            stream.Position = 0;

            return requestBody;
        }

        [NotNull]
        private readonly ILogger<LoggingMiddleware> _logger;

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        [NotNull]
        private readonly ITracer _tracer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateFormatStringProblem")]
        private void LogFinish(
            [NotNull] HttpContext context,
            ReadOnlyMemory<byte> responseBody,
            TimeSpan duration,
            [CanBeNull] Exception exception)
        {
            const string message =
                "External request handling {@OperationAction} after {@OperationDuration}ms.\nStatus code: {@StatusCode}\nHeaders: {@Headers}\nBody: {@Body:l}";

            var wasHandled = exception is not null && exception.Data.Contains("WasHandled") && exception.Data["WasHandled"] is true;

            var args = ArrayPool<object>.Shared.Rent(5);

            args[0] = wasHandled || exception is null ? "finished" : "failed";
            args[1] = (int) duration.TotalMilliseconds;
            args[2] = context.Response.StatusCode;
            args[3] = context.Request.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);
            args[4] = _options.Logging.LogResponsesBodies ? Encoding.UTF8.GetString(responseBody.Span) : string.Empty;

            if (wasHandled || exception is null)
                _logger.LogInformation(message, args);
            else
                _logger.LogInformation(exception, message, args);

            for (var i = 0; i < 5; i++)
                args[i] = default;

            ArrayPool<object>.Shared.Return(args);
        }

        private void LogStart([NotNull] HttpContext context, [CanBeNull] string requestBody)
        {
            const string message =
                "External request handling {@OperationAction}.\nMethod: {@Method}\nPath: {@Path}\nQuery string: {@Query}\nHeaders: {@Headers}\nBody: {@Body:l}";

            var args = ArrayPool<object>.Shared.Rent(6);

            args[0] = "started";
            args[1] = context.Request.Method;
            args[2] = context.Request.Path.Value;
            args[3] = context.Request.QueryString.Value;
            args[4] = context.Request.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);
            args[5] = _options.Logging.LogRequestsBodies ? requestBody : string.Empty;

            _logger.LogInformation(message, args);

            for (var i = 0; i < 6; i++)
                args[i] = default;

            ArrayPool<object>.Shared.Return(args);
        }
    }
}

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
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Middleware logging requests and responses.
    /// </summary>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    internal sealed class LoggingMiddleware : IMiddleware
    {
        public LoggingMiddleware([NotNull] ILogger<LoggingMiddleware> logger, [NotNull] IOptions<Options> serviceOptions, [NotNull] ITracer tracer)
        {
            _logger  = logger;
            _tracer  = tracer;
            _options = serviceOptions.Value;

            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager(_options.Logging.Middleware.SmallBufferBlockSize,
                                                                               _options.Logging.Middleware.LargeBufferBlockSizeMultiple,
                                                                               _options.Logging.Middleware.MaximumSingleBufferSize)
            {
                AggressiveBufferReturn    = true,
                GenerateCallStacks        = false,
                MaximumFreeSmallPoolBytes = _options.Logging.Middleware.SmallBufferMaximumPoolBytes,
                MaximumFreeLargePoolBytes = _options.Logging.Middleware.LargeBufferMaximumPoolBytes
            };

            _logStartTemplate = "External request handling {@OperationAction}.\nMethod: {@HttpMethod}\nPath: {@HttpPath}\nQuery string: {@HttpQuery}" +
                                (_options.Logging.LogRequestsHeaders ? "\nHeaders: {@OperationMetadata}" : string.Empty) +
                                (_options.Logging.LogRequestsBodies ? "\nBody: {@OperationContent:l}" : string.Empty);

            _logFinishTemplate = "External request handling {@OperationAction} after {@OperationDuration}ms.\nStatus code: {@HttpStatusCode}" +
                                 (_options.Logging.LogResponsesHeaders ? "\nHeaders: {@OperationMetadata}" : string.Empty) +
                                 (_options.Logging.LogResponsesBodies ? "\nBody: {@OperationContent:l}" : string.Empty);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint is null)
            {
                await next(context);

                return;
            }

            using var span = OpenTracerSpanForRoute(endpoint);

            if (_options.Logging.LogRequests)
            {
                if (_options.Logging.LogRequestsBodies)
                    await LogRequestWithBody(context);
                else
                    LogStart(context, null);
            }

            if (_options.Logging.LogResponses)
            {
                if (_options.Logging.LogResponsesBodies)
                    await LogResponseWithBody(context, next, span);
                else
                    await LogResponseWithoutBody(context, next, span);
            }
            else
            {
                await next(context)
                    .AsNotNull();
            }
        }

        [NotNull]
        [ItemNotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        private readonly string _logFinishTemplate;

        [NotNull]
        private readonly ILogger<LoggingMiddleware> _logger;

        [NotNull]
        private readonly string _logStartTemplate;

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        [NotNull]
        private readonly ITracer _tracer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LogFinish(
            [NotNull] HttpContext context,
            [CanBeNull] ReadOnlySequence<byte>? responseBody,
            TimeSpan duration,
            [CanBeNull] Exception exception)
        {
            var wasHandled = exception is not null && exception.Data.Contains("WasHandled") && exception.Data["WasHandled"] is true;

            var args = ArrayPool<object>.Shared.Rent(5);
            var i    = 0;

            args[i++] = wasHandled || exception is null ? "finished" : "failed";
            args[i++] = (int) duration.TotalMilliseconds;
            args[i++] = context.Response.StatusCode;

            if (_options.Logging.LogResponsesHeaders)
                args[i++] = context.Request.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);

            if (_options.Logging.LogResponsesBodies)
                args[i] = responseBody is null ? null : Encoding.UTF8.GetString(responseBody.Value.FirstSpan);

            if (wasHandled || exception is null)
                _logger.LogInformation(_logFinishTemplate, args);
            else
                _logger.LogInformation(exception, _logFinishTemplate, args);

            for (var j = 0; j < i; j++)
                args[j] = default;

            ArrayPool<object>.Shared.Return(args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LogRequestWithBody([NotNull] HttpContext context)
        {
            context.Request.EnableBuffering();

            context.Request.Body.Position = 0;

            var requestBody = await ReadStreamAsync(context.Request.Body);

            LogStart(context, requestBody);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LogResponseWithBody([NotNull] HttpContext context, [NotNull] RequestDelegate next, [NotNull] ITracerSpan span)
        {
            await using var memoryStream =
                (RecyclableMemoryStream) _recyclableMemoryStreamManager.GetStream(Guid.NewGuid(),
                                                                                  nameof(LoggingMiddleware),
                                                                                  _options.Logging.Middleware.SmallBufferBlockSize);

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

            memoryStream.Seek(0, SeekOrigin.Begin);

            var responseBody = memoryStream.GetReadOnlySequence();

            foreach (var memory in responseBody)
                await originalResponseBodyStream.WriteAsync(memory, context.RequestAborted);

            context.Response.Body          = originalResponseBodyStream;
            context.Response.ContentLength = memoryStream.Position;

            LogFinish(context,
                      responseBody,
                      span.Duration,
                      exception);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LogResponseWithoutBody([NotNull] HttpContext context, [NotNull] RequestDelegate next, [NotNull] ITracerSpan span)
        {
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

            LogFinish(context,
                      null,
                      span.Duration,
                      exception);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LogStart([NotNull] HttpContext context, [CanBeNull] string requestBody)
        {
            var args = ArrayPool<object>.Shared.Rent(6);
            var i    = 0;

            args[i++] = "started";
            args[i++] = context.Request.Method;
            args[i++] = context.Request.Path.Value;
            args[i++] = context.Request.QueryString.Value;

            if (_options.Logging.LogRequestsHeaders)
                args[i++] = context.Request.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);

            if (_options.Logging.LogRequestsBodies)
                args[i] = requestBody;

            _logger.LogInformation(_logStartTemplate, args);

            for (var j = 0; j < i; j++)
                args[j] = default;

            ArrayPool<object>.Shared.Return(args);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ITracerSpan OpenTracerSpanForRoute([NotNull] Endpoint endpoint)
        {
            var controllerActionDescriptor = endpoint.Metadata.OfType<ControllerActionDescriptor>()
                                                     .Single();

            var controllerName = controllerActionDescriptor.ControllerTypeInfo.FullName;
            var actionName     = controllerActionDescriptor.ActionName;

            var span = _tracer.OpenSpan($"{controllerName}.{actionName}", SpanType.ExternalRequestHandler);

            return span;
        }
    }
}

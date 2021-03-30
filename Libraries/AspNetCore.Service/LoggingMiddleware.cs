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
#if NETCOREAPP2_2
using Microsoft.AspNetCore.Routing;
#elif NETCOREAPP3_1 || NET5_0
using Microsoft.AspNetCore.Mvc.Controllers;

#endif

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
        public LoggingMiddleware([NotNull] ILogger<LoggingMiddleware> logger, [NotNull] IOptions<ServiceOptions> serviceOptions, [NotNull] ITracer tracer)
        {
            _logger         = logger;
            _tracer         = tracer;
            _serviceOptions = serviceOptions.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            context.Request.EnableBuffering();

#if NETCOREAPP3_1 || NET5_0
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
#elif NETCOREAPP2_2
            var routeData = context.GetRouteData();

            if (!routeData.Values.TryGetValue("controller", out var controllerName) || !routeData.Values.TryGetValue("action", out var actionName))
            {
                await next(context);

                return;
            }
#endif

            using var scope = _tracer.OpenSpan($"{controllerName}.{actionName}", SpanType.ExternalRequestHandler);

            context.Request.Body.Position = 0;

            using var reader = new StreamReader(context.Request.Body,
                                                Encoding.UTF8,
                                                false,
                                                -1,
                                                true);

            var requestBody = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;

            if (_serviceOptions.LogRequests)
                LogStart(context, requestBody);

            Memory<byte> responseBody;
            Exception    exception = default;

#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
            await
#endif
                using (var memoryStream = _recyclableMemoryStreamManager.GetStream())
            {
                var originalResponseBodyStream = context.Response.Body;
                context.Response.Body = memoryStream;

                try
                {
                    await next(context)
                        .AsNotNull();
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                }

                responseBody = memoryStream.GetBuffer()
                                           .AsMemory(0, (int) memoryStream.Length);

                context.Response.Body          = originalResponseBodyStream;
                context.Response.ContentLength = memoryStream.Length;

                await context.Response.Body.WriteAsync(responseBody, context.RequestAborted);
            }

            if (_serviceOptions.LogResponses)
                LogFinish(context,
                          responseBody,
                          scope.Duration,
                          exception);
        }

        [NotNull]
        private readonly ILogger<LoggingMiddleware> _logger;

        [NotNull]
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();

        [NotNull]
        private readonly ServiceOptions _serviceOptions;

        [NotNull]
        private readonly ITracer _tracer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateFormatStringProblem")]
        private void LogFinish(
            [NotNull] HttpContext context,
            Memory<byte> responseBody,
            TimeSpan duration,
            [CanBeNull] Exception exception)
        {
            const string message =
                "External request handling {@OperationAction} after {@OperationDuration}ms.\nStatus code: {@StatusCode}\nHeaders: {@Headers}\nBody: {@Body:l}";

            var wasHandled = exception is not null && exception.Data.Contains("WasHandled") && exception.Data["WasHandled"] is bool x && x;

            var args = ArrayPool<object>.Shared.Rent(5);

            args[0] = wasHandled || exception is null ? "finished" : "failed";
            args[1] = (int) duration.TotalMilliseconds;
            args[2] = context.Response.StatusCode;
            args[3] = context.Request.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);
            args[4] = _serviceOptions.LogResponsesBodies && responseBody.Length > 0 ? Encoding.UTF8.GetString(responseBody.Span) : string.Empty;

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
            args[5] = _serviceOptions.LogRequestsBodies ? requestBody : string.Empty;

            _logger.LogInformation(message, args);

            for (var i = 0; i < 6; i++)
                args[i] = default;

            ArrayPool<object>.Shared.Return(args);
        }
    }
}

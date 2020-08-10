using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
#if NETCOREAPP2_2
using Microsoft.AspNetCore.Routing;

#elif NETCOREAPP3_1
using Microsoft.AspNetCore.Mvc.Controllers;

#endif

namespace GS.DecoupleIt.AspNetCore.Service
{
    [Transient]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    internal sealed class LoggingMiddleware : IMiddleware
    {
        public LoggingMiddleware([NotNull] ILogger<LoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            context.Request.EnableBuffering();

#if NETCOREAPP3_1
            var endpoint = context.GetEndpoint();

            if (endpoint is null)
            {
                await next(context);

                return;
            }

            var controllerActionDescriptor = endpoint.Metadata.OfType<ControllerActionDescriptor>()
                                                     .Single();

            var controllerName = controllerActionDescriptor.ControllerTypeInfo.FullName;
            var actionName = controllerActionDescriptor.ActionName;
#elif NETCOREAPP2_2
            var routeData = context.GetRouteData();

            if (!routeData.Values.TryGetValue("controller", out var controllerName) || !routeData.Values.TryGetValue("action", out var actionName))
            {
                await next(context);

                return;
            }
#endif

            var requestBody = await ReadStream(context.Request.Body);

            using (var scope = Tracer.OpenChildSpan($"{controllerName}.{actionName}", SpanType.ExternalRequestHandler))
            {
                LogStart(context, requestBody);

                try
                {
                    string responseBody;

                    using (var memoryStream = new MemoryStream())
                    {
                        var responseBodyStream = context.Response.Body;
                        context.Response.Body = memoryStream;

                        await next(context)
                            .AsNotNull();

                        responseBody          = await ReadStream(memoryStream);
                        context.Response.Body = responseBodyStream;

                        var bytes = Encoding.UTF8.GetBytes(responseBody);

                        context.Response.ContentLength = bytes.Length;
                        await context.Response.Body.WriteAsync(bytes);
                    }

                    LogFinish(context, responseBody, scope.Duration);
                }
                catch (Exception exception)
                {
                    _logger.LogInformation(exception, "External request handling failure after {@Duration}ms.", scope.Duration.TotalMilliseconds);
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        private static async Task<string> ReadStream([NotNull] Stream stream)
        {
            stream.Position = 0;

            using (var reader = new StreamReader(stream,
                                                 Encoding.UTF8,
                                                 true,
                                                 -1,
                                                 true))
            {
                var result = await reader.ReadToEndAsync();

                stream.Position = 0;

                return result;
            }
        }

        [NotNull]
        private readonly ILogger<LoggingMiddleware> _logger;

        private void LogFinish([NotNull] HttpContext context, [CanBeNull] string responseBody, TimeSpan duration)
        {
            var message = new StringBuilder("External request handling finished after {@Duration}ms.\nHeaders: {@Headers}");

            var args = new List<object>
            {
                duration.TotalMilliseconds,
                context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)
            };

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                message.Append("\nBody: {@Body}");
                args.Add(responseBody);
            }

            _logger.LogInformation(message.ToString(), args.ToArray());
        }

        private void LogStart([NotNull] HttpContext context, [CanBeNull] string requestBody)
        {
            var message = new StringBuilder("External request handling started.\nMethod: {@Method}\nPath: {@Path}\nHeaders: {@Headers}");

            var args = new List<object>
            {
                context.Request.Method,
                context.Request.Path.Value,
                context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)
            };

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                message.Append("\nBody: {@Body}");
                args.Add(requestBody);
            }

            _logger.LogInformation(message.ToString(), args.ToArray());
        }
    }
}
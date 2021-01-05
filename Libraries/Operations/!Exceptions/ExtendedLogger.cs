using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Operations
{
    internal sealed class ExtendedLogger<T> : ILogger<T>
    {
        public ExtendedLogger([NotNull] ILoggerFactory factory, [NotNull] IOptions<Options> options)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _options = options.Value!;

            _logger = factory.CreateLogger(typeof(T).FullName)!;
        }

        [NotNull]
        private static readonly string TypeName = typeof(T).FullName!;

        [NotNull]
        private readonly ILogger _logger;

        [NotNull]
        private readonly Options _options;

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (exception is ExtendedException exceptionBase && _options.Logging.ExceptionRemap.ContainsKey(exceptionBase.Category))
            {
                _logger.Log(_options.Logging.ExceptionRemap[exceptionBase.Category],
                            eventId,
                            state,
                            exception,
                            formatter);

                return;
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logger.Log(_options.Logging.TraceRemap.ContainsKey(TypeName) ? _options.Logging.TraceRemap[TypeName] : logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                case LogLevel.Debug:
                    _logger.Log(_options.Logging.DebugRemap.ContainsKey(TypeName) ? _options.Logging.DebugRemap[TypeName] : logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                case LogLevel.Information:
                    _logger.Log(_options.Logging.InformationRemap.ContainsKey(TypeName) ? _options.Logging.InformationRemap[TypeName] : logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                case LogLevel.Warning:
                    _logger.Log(_options.Logging.WarningRemap.ContainsKey(TypeName) ? _options.Logging.WarningRemap[TypeName] : logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                case LogLevel.Error:
                    _logger.Log(_options.Logging.ErrorRemap.ContainsKey(TypeName) ? _options.Logging.ErrorRemap[TypeName] : logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                case LogLevel.Critical:
                    _logger.Log(_options.Logging.CriticalRemap.ContainsKey(TypeName) ? _options.Logging.CriticalRemap[TypeName] : logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                case LogLevel.None:
                    _logger.Log(logLevel,
                                eventId,
                                state,
                                exception,
                                formatter);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
    }
}

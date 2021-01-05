using System;
using System.Collections.Generic;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Operations
{
    [Singleton]
    internal sealed class ExtendedLoggerFactory : IExtendedLoggerFactory
    {
        public ExtendedLoggerFactory([NotNull] ILoggerFactory loggerFactory, [NotNull] IOptions<Options> options)
        {
            _loggerFactory = loggerFactory;
            _options       = options;
        }

        public ILogger<T> Create<T>()
        {
            if (_loggersCache.ContainsKey(typeof(T)))
                return ((ILogger<T>) _loggersCache[typeof(T)])!;

            var logger = new ExtendedLogger<T>(_loggerFactory, _options);

            _loggersCache.Add(typeof(T), logger);

            return logger;
        }

        [NotNull]
        private readonly ILoggerFactory _loggerFactory;

        [NotNull]
        private readonly Dictionary<Type, ILogger> _loggersCache = new();

        [NotNull]
        private readonly IOptions<Options> _options;
    }
}

using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Tracing
{
    public sealed class Builder : ExtensionBuilderBase
    {
        public Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }

        [NotNull]
        public Builder WithLoggerPropertiesConfiguration([NotNull] Action<LoggerPropertiesOptions> configure)
        {
            ServiceCollection.PostConfigure(configure);

            return this;
        }
    }
}

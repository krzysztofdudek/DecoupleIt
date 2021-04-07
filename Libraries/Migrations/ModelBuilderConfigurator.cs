using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GS.DecoupleIt.Migrations
{
    internal sealed class ModelBuilderConfigurator
    {
        [NotNull]
        public readonly Action<ModelBuilder> ConfigureModelBuilder;

        public ModelBuilderConfigurator([NotNull] Action<ModelBuilder> configureModelBuilder)
        {
            ConfigureModelBuilder = configureModelBuilder;
        }
    }
}

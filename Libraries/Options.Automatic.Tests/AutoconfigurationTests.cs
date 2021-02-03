using System;
using System.Collections.Generic;
using System.Linq;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GS.DecoupleIt.Options.Automatic.Tests
{
    public sealed class AutoconfigurationTests
    {
        [NotNull]
        private static IServiceProvider GetServiceProvider([NotNull] Dictionary<string, string> properties)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(properties)
                                                          .AsNotNull()
                                                          .Build()
                                                          .AsNotNull();

            var serviceCollection = new ServiceCollection();

            serviceCollection.ScanAssemblyForOptions(typeof(AutoconfigurationTests).Assembly, configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider()
                                                   .AsNotNull();

            return serviceProvider;
        }

        [Configure]
        public sealed class DefaultSectionOptions
        {
            public int Property { get; set; }
        }

        [Configure("CustomSection")]
        public sealed class CustomSectionOptions
        {
            public int Property { get; set; }
        }

        [ConfigureAsNamespace]
        public sealed class NamespaceSectionOptions
        {
            public int Property { get; set; }
        }

        [ConfigureAsNamespace(Priority = 0)]
        [Configure(Priority            = 1)]
        public sealed class PrioritizedConfigurationOptions
        {
            public int Property { get; set; }
        }

        [Configure("NewSection")]
        internal sealed class ConfigurePropertyOptions
        {
            [ConfigureProperty("VeryOldSection:Property", Priority = 0)]
            [ConfigureProperty("OldSection:Property", Priority     = 2)]
            public int Property { get; set; }
        }

        [Configure("NewSection")]
        internal sealed class ConfigurePropertyWithNullOptions
        {
            [ConfigureProperty("OldSection:Property", AssignNull = true)]
            public string Property { get; set; }
        }

        [Configure("NewSection")]
        internal sealed class DontConfigurePropertyWithNullOptions
        {
            [ConfigureProperty("OldSection:Property")]
            public string Property { get; set; }
        }

        [Configure("NewSection")]
        internal sealed class ConfigureCollectionPropertyOptions
        {
            [NotNull]
            [ConfigureProperty("OldSection:Property")]
            public List<string> Property { get; set; } = new();
        }

        [Configure("NewSection")]
        internal sealed class ConfigureObjectOptions
        {
            [NotNull]
            [ConfigureProperty("OldSection:Object")]
            public ConfigurationObject Object { get; set; } = new();

            public sealed class ConfigurationObject
            {
                public string Property { get; [UsedImplicitly] set; }
            }
        }

        [Fact]
        public void ConfigureCollectionProperty()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "OldSection:Property:0", "1"
                },
                {
                    "OldSection:Property:1", "2"
                },
                {
                    "OldSection:Property:2", "3"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<ConfigureCollectionPropertyOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.NotNull(options.Property);
            Assert.Equal(3, options.Property.Count);
            Assert.Equal("1", options.Property.First());

            Assert.Equal("2",
                         options.Property.Skip(1)
                                .First());

            Assert.Equal("3",
                         options.Property.Skip(2)
                                .First());
        }

        [Fact]
        public void ConfigureCollectionPropertyForEmptyEntry()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>());

            var options = serviceProvider.GetRequiredService<IOptions<ConfigureCollectionPropertyOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.NotNull(options.Property);
            Assert.Empty(options.Property);
        }

        [Fact]
        public void ConfigureNullProperty()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "NewSection:Property", "1"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<ConfigurePropertyWithNullOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Null(options.Property);
        }

        [Fact]
        public void ConfigureObject()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "OldSection:Object:Property", "1"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<ConfigureObjectOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal("1", options.Object.Property);
        }

        [Fact]
        public void ConfigureProperty()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "VeryOldSection:Property", "1"
                },
                {
                    "OldSection:Property", "2"
                },
                {
                    "NewSection:Property", "3"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<ConfigurePropertyOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal(2, options.Property);
        }

        [Fact]
        public void ConfigureWithCustomSection()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "CustomSection:Property", "1"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<CustomSectionOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal(1, options.Property);
        }

        [Fact]
        public void ConfigureWithDefaultSection()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "GS:DecoupleIt:Options:Automatic:Tests:AutoconfigurationTests:DefaultSection:Property", "1"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<DefaultSectionOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal(1, options.Property);
        }

        [Fact]
        public void ConfigureWithNamespaceSection()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "GS:DecoupleIt:Options:Automatic:Tests:Property", "1"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<NamespaceSectionOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal(1, options.Property);
        }

        [Fact]
        public void ConfigureWithPrioritizedConfiguration()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "GS:DecoupleIt:Options:Automatic:Tests:AutoconfigurationTests:DefaultSection:Property", "1"
                },
                {
                    "GS:DecoupleIt:Options:Automatic:Tests:Property", "2"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<PrioritizedConfigurationOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal(2, options.Property);
        }

        [Fact]
        public void DoNotConfigureNullProperty()
        {
            var serviceProvider = GetServiceProvider(new Dictionary<string, string>
            {
                {
                    "NewSection:Property", "1"
                }
            });

            var options = serviceProvider.GetRequiredService<IOptions<DontConfigurePropertyWithNullOptions>>()
                                         .AsNotNull()
                                         .Value.AsNotNull();

            Assert.Equal("1", options.Property);
        }
    }
}

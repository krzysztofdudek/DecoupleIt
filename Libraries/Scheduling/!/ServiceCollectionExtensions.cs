using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Scheduling.Exceptions;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" /> with methods registering jobs to be scheduled.
    /// </summary>
    [NotNull]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configures jobs.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configureOptions">Configure options delegate.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection ConfigureJobs(
            [NotNull] [ItemNotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [CanBeNull] Action<Options> configureOptions = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(configuration), configuration);

            serviceCollection.ScanAssemblyForOptions(typeof(ServiceCollectionExtensions).Assembly, configuration);

            if (configureOptions is not null)
                serviceCollection.PostConfigure(configureOptions);

            return serviceCollection;
        }

        /// <summary>
        ///     Registers all jobs that are annotated with <see cref="CyclicSchedule" />. They'll be run later by selected
        ///     implementation, for ex. Quartz.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="assembly">Assembly to scan.</param>
        /// <returns>Service collection.</returns>
        /// <exception cref="AmbiguousSchedulingAttributes">
        ///     Exception is thrown when scan of the assembly founds jobs with more
        ///     than one scheduling attribute.
        /// </exception>
        [NotNull]
        [PublicAPI]
        public static IServiceCollection ScanAssemblyForJobs([NotNull] [ItemNotNull] this IServiceCollection serviceCollection, [NotNull] Assembly assembly)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(assembly), assembly);

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);

            var jobs = GetJobs(assembly);

            ValidateAmbiguousAttributes(jobs);
            ValidateInvalidConfiguration(jobs);

            var existingCollection = serviceCollection.SingleOrDefault(x => x.ImplementationInstance is JobsCollection)
                                                      ?.ImplementationInstance;

            var jobsCollection = (JobsCollection) existingCollection ?? new JobsCollection();

            jobsCollection.AddRange(jobs.Select(x => new JobEntry(x.x.AsNotNull(),
                                                                  x.Item2.AsNotNull()
                                                                   .First()
                                                                   .AsNotNull()))
                                        .Where(x => jobsCollection.All(y => y.JobType != x.JobType)));

            if (existingCollection is null)
                serviceCollection.AddSingleton<IRegisteredJobs>(jobsCollection);

            return serviceCollection;
        }

        [NotNull]
        private static List<(Type x, List<IScheduleAttribute>)> GetJobs([NotNull] Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(x => x.GetCustomAttributes()
                                        .OfType<IScheduleAttribute>()
                                        .Any())
                           .Select(x => (x, x.GetCustomAttributes()
                                             .OfType<IScheduleAttribute>()
                                             .ToList()))
                           .AsCollectionWithNotNullItems()
                           .ToList();
        }

        private static void ValidateAmbiguousAttributes([NotNull] IEnumerable<(Type x, List<IScheduleAttribute>)> jobs)
        {
            var jobsWithAmbiguousAttributes = jobs.Where(x => x.Item2.AsNotNull()
                                                               .Count > 1)
                                                  .ToList();

            if (jobsWithAmbiguousAttributes.Any())
                throw new AmbiguousSchedulingAttributes(jobsWithAmbiguousAttributes.Select(x => x.x)
                                                                                   .ToList());
        }

        private static void ValidateInvalidConfiguration([NotNull] IEnumerable<(Type x, List<IScheduleAttribute>)> jobs)
        {
            var errors = new StringBuilder();

            foreach (var (type, attributes) in jobs)
            {
                var attribute = attributes.AsNotNull()
                                          .First()
                                          .AsNotNull();

                var validationDescription = attribute.Validate();

                if (validationDescription != null)
                    errors.Append($"\n{type.AsNotNull().FullName}: {validationDescription}");
            }

            if (errors.Length > 0)
                throw new InvalidScheduleConfiguration(errors.ToString());
        }
    }
}

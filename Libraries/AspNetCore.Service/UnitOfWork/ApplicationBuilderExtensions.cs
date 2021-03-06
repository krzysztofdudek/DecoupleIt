﻿using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace GS.DecoupleIt.AspNetCore.Service.UnitOfWork
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" />.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Uses middleware that initializes async local storage at the
        ///     beginning of a request and clears it after the execution for <see cref="UnitOfWorkAccessor" />.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>Application builder.</returns>
        [NotNull]
        public static IApplicationBuilder MaintainStorageOfContextualUnitOfWork([NotNull] this IApplicationBuilder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.Use(async (_, next) =>
            {
                UnitOfWorkAccessor.Initialize();

                try
                {
                    await next.AsNotNull()
                              .Invoke()
                              .AsNotNull();
                }
                finally
                {
                    UnitOfWorkAccessor.Clear();
                }
            });

            return builder;
        }

        /// <summary>
        ///     Uses middleware to create unit of work for all incoming requests.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>Application builder.</returns>
        /// <typeparam name="TUnitOfWork">Unit of work type.</typeparam>
        [NotNull]
        public static IApplicationBuilder UseContextualUnitOfWork<TUnitOfWork>([NotNull] this IApplicationBuilder builder)
            where TUnitOfWork : class, IUnitOfWork
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.UseMiddleware<UnitOfWorkContextMiddleware<TUnitOfWork>>();

            return builder;
        }
    }
}

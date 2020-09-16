using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" />.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
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

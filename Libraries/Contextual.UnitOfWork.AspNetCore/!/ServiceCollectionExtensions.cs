using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds support of contextual unit of work for ASP .NET Core.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <returns>Builder.</returns>
        /// <typeparam name="TUnitOfWork">Unit of work type.</typeparam>
        [NotNull]
        public static Builder AddContextMiddlewareFor<TUnitOfWork>([NotNull] this Builder builder)
            where TUnitOfWork : class, IUnitOfWork
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.ServiceCollection.AddSingleton<UnitOfWorkContextMiddleware<TUnitOfWork>>();

            return builder;
        }
    }
}

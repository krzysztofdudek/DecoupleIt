using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
    internal sealed class UnitOfWorkContextMiddleware<TUnitOfWork> : IMiddleware
        where TUnitOfWork : class, IUnitOfWork
    {
        public UnitOfWorkContextMiddleware([NotNull] IUnitOfWorkAccessor unitOfWorkAccessor)
        {
            _unitOfWorkAccessor = unitOfWorkAccessor;
        }

        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
            await
#endif
            using (_unitOfWorkAccessor.GetLazy<TUnitOfWork>())
            {
                await next(context)
                    .AsNotNull();
            }

            if (UnitOfWorkAccessor.IsAvailable<TUnitOfWork>())
                throw new UnitOfWorkWasNotProperlyDisposed();
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;
    }
}

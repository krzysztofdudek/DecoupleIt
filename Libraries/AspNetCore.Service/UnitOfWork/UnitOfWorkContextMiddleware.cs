using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GS.DecoupleIt.AspNetCore.Service.UnitOfWork
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
            await using (_unitOfWorkAccessor.GetLazy<TUnitOfWork>())
            {
                await next(context)
                    .AsNotNull();
            }

            if (UnitOfWorkAccessor.IsAvailable<TUnitOfWork>(out var stackTrace))
                throw new UnitOfWorkWasNotProperlyDisposed(stackTrace);
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;
    }
}

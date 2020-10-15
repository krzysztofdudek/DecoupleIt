using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Base job interface.
    /// </summary>
    [Singleton]
    public interface IJob
    {
        /// <summary>
        ///     Executes job.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        Task ExecuteAsync([PublicAPI] CancellationToken cancellationToken = default);
    }
}

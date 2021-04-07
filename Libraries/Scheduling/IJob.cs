using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Base interface of the job.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        ///     Executes the job.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
#if NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            ExecuteAsync([PublicAPI] CancellationToken cancellationToken = default);
    }
}

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Migrations
{
    /// <summary>
    ///     Base interface of a migration. It allows to upgrade or downgrade application state.
    /// </summary>
    [PublicAPI]
    public interface IMigration
    {
        /// <summary>
        ///     Downgrade action.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public ValueTask DowngradeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Upgrade action.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public ValueTask UpgradeAsync(CancellationToken cancellationToken = default);
    }
}

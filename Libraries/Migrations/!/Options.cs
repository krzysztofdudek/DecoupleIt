using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GS.DecoupleIt.Migrations
{
    /// <summary>
    ///     Migration options.
    /// </summary>
    [ConfigureAsNamespace]
    [PublicAPI]
    public sealed class Options
    {
        /// <summary>
        ///     Enables migration processing.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Delegate used for retrieving migrations from DbContext. It can be used for locking purposes across many hosts.
        /// </summary>
        [NotNull]
        public GetMigrationsDelegate GetMigrations { get; set; } = async (
            dbContext,
            number,
            name,
            cancellationToken) => (await dbContext.Migrations.AsNoTracking()!.Where(x => x.Number == number && x.Name == name)
                                                  .ToListAsync(cancellationToken)!)!;

        /// <summary>
        ///     Delegate used by <see cref="Options.GetMigrations" />.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public delegate ValueTask<IEnumerable<Migration>> GetMigrationsDelegate(
            [NotNull] MigrationsDbContext migrationsDbContext,
            long number,
            [NotNull] string name,
            CancellationToken cancellationToken = default);
    }
}

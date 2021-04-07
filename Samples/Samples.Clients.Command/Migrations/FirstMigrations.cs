using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Migrations;

namespace Samples.Clients.Command.Migrations
{
    [Migration(1, "First migration created for test.")]
    internal sealed class FirstMigrations : IMigration
    {
        public ValueTask DowngradeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask UpgradeAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Example migration.");

            return new ValueTask();
        }
    }
}

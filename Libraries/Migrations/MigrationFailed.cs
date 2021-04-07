using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Migrations
{
    /// <summary>
    ///     Exception thrown when migration process failed.
    /// </summary>
    [PublicAPI]
    public sealed class MigrationFailed : Exception
    {
        internal MigrationFailed([CanBeNull] string message) : base(message) { }

        internal MigrationFailed([CanBeNull] Exception innerException) : base("Migration failed.", innerException) { }
    }
}

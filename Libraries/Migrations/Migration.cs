using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Migrations
{
    public sealed class Migration
    {
        [NotNull]
        public string Description { get; [UsedImplicitly] private set; }

        public DateTime ExecutedOn { get; [UsedImplicitly] private set; }

        public Guid HostIdentifier { get; [UsedImplicitly] private set; }

        [NotNull]
        public string HostName { get; [UsedImplicitly] private set; }

        [CanBeNull]
        public string HostVersion { get; [UsedImplicitly] private set; }

        public Guid Id { get; [UsedImplicitly] private set; }

        [NotNull]
        public string Name { get; [UsedImplicitly] private set; }

        public long Number { get; [UsedImplicitly] private set; }

        public Migration(
            long number,
            [NotNull] string name,
            [NotNull] string description,
            DateTime executedOn,
            Guid hostIdentifier,
            [NotNull] string hostName,
            [CanBeNull] string hostVersion)
        {
            Id             = Guid.NewGuid();
            Number         = number;
            Name           = name;
            Description    = description;
            ExecutedOn     = executedOn;
            HostIdentifier = hostIdentifier;
            HostName       = hostName;
            HostVersion    = hostVersion;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        private Migration() { }
    }
}

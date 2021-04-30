using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    public sealed class HostInformation : IHostInformation
    {
        public string Environment { get; }

        public Guid Identifier { get; }

        public string Name { get; }

        public string Version { get; }

        public HostInformation(
            Guid identifier,
            [NotNull] string name,
            [CanBeNull] string version,
            [CanBeNull] string environment)
        {
            Identifier  = identifier;
            Name        = name;
            Version     = version;
            Environment = environment;
        }
    }
}

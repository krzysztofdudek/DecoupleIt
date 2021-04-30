using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    [PublicAPI]
    public interface IHostInformation
    {
        [CanBeNull]
        public string Environment { get; }

        public Guid Identifier { get; }

        [NotNull]
        public string Name { get; }

        [CanBeNull]
        public string Version { get; }
    }
}
